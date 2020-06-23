using UnityEngine;
using System.Linq;
using System;

public class ClimbMovementManager : AbstractMovementManager
{
    Transform sphere;
    Rigidbody sphereRigidbody;

    // Properties
    float wallGravity = 20.0f;
    float ignoreSameHoldDistance = .3f;
    float radius = .4f;

    // Trackers
    bool beenReset, onWall, hanging;
    float reach, horizontal, vertical;
    Vector3 max, min;

    public ClimbMovementManager(AbstractMovementManager movementManager) : base(movementManager)
    {
        // Set Animation
        SetClimbing(true);

        // Character's Reach
        reach = character.GetTraitValue("height") * .45f;

        // Disable Player Colliders and Rigidbody
        graphics.GetComponent<CapsuleCollider>().enabled = false;
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;

        // Add Sphere Game Object
        GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere = gameObject.transform;
        sphere.localScale *= radius;
        sphere.parent = transform;

        // Set Sphere Mono Behavior to handle Collision and Trigger Events
        SphereMonoBehaviour sphereMonoBehaviour = gameObject.AddComponent<SphereMonoBehaviour>();
        sphereMonoBehaviour.SetClimbMovementManager(this);

        // Set SphereRigidbody
        sphereRigidbody = gameObject.AddComponent<Rigidbody>();
        sphereRigidbody.angularDrag = 0;
        sphereRigidbody.mass = 0;
        sphereRigidbody.freezeRotation = true;
        sphereRigidbody.useGravity = false;

        // Set RayCast/Hold
        SetHitInfo(collider, pointOfContact);

        SetCurrentState(MovementState.Climb);
    }

    public override bool AttemptSetDown(bool desired)
    {
        if (desired)
        {
            // Set Position off the wall
            transform.position = character.getBodyPartHead(graphics).position;
            SetIntendedState(MovementState.Ground, .5f);
        }
        return desired;
    }

    public override void CleanUp()
    {
        GameObject.Destroy(sphere.gameObject);
        SetClimbing(false);
        SetHanging(false);
        graphics.GetComponent<CapsuleCollider>().enabled = true;
    }

    public override void OnCollisionStay(Collision collision)
    {
        if (midTransition)
        {
            return;
        }

        if (collision.gameObject.layer == DEFAULT_INDEX)
        {
            // Change forward direction to look in the direction of the hold. 
            // This solves for a hold being on a different angle than the current player/sphere
            Vector3 forward = sphere.forward;
            sphere.forward = Vector3.down;

            // Raycast, in a plus formation, for the hold the sphere triggered
            RaycastHit hitInfo = RaycastSpread(sphere.position, sphere.forward, .2f, HOLD, Color.yellow, collision.transform);
            sphere.forward = forward;
            if (hitInfo.collider != null)
            {
                SetHitInfo(hitInfo);
                SetIntendedState(MovementState.Ground, 10f);
            }
        }
        onWall = true;
    }

    public override void OnTriggerStay(Collider other)
    {
        if (midTransition)
        {
            return;
        }

        float distance = Mathf.Abs(Vector3.Distance(pointOfContact, sphere.position));
        if (other.transform == collider.transform && ignoreSameHoldDistance > distance)
        {
            return;
        }

        if (other.gameObject.layer == HOLD_INDEX)
        {
            // Change forward direction to look in the direction of the hold. 
            // This solves for a hold being on a different angle than the current player/sphere
            Vector3 forward = sphere.forward;
            sphere.forward = other.transform.forward;

            // Raycast, in a plus formation, for the hold the sphere triggered
            RaycastHit hitInfo = RaycastSpread(sphere.position, sphere.forward, .2f, HOLD, Color.yellow, other.transform);
            sphere.forward = forward;
            if (hitInfo.collider != null)
            {
                SetHitInfo(hitInfo);
            }
        }
    }

    public override void SetHitInfo(RaycastHit raycastHit)
    {
        SetHitInfo(raycastHit.collider, raycastHit.point);
    }

    public override void SetHitInfo(Collider collider, Vector3 pointOfContact)
    {
        base.SetHitInfo(collider, pointOfContact);
        sphere.forward = collider.transform.forward;
        transform.forward = collider.transform.forward;
        transform.position = pointOfContact;
        min = new Vector3(pointOfContact.x - reach, pointOfContact.y - reach, pointOfContact.z - reach);
        max = new Vector3(pointOfContact.x + reach, pointOfContact.y + reach, pointOfContact.z + reach);
        ResetSphereLocation();
    }

    public override void UpdateAnimation()
    {
        // TODO
    }

    public override void UpdateMovement(float horizontal, float vertical, Vector3 lookDirection)
    {
        this.horizontal = horizontal;
        this.vertical = vertical;
        if (horizontal == 0 && vertical == 0)
        {
            if (!beenReset)
            {
                ResetSphereLocation();
            }
            return;
        }
        else if (onWall)
        {
            Vector3 targetVelocity = new Vector3(horizontal, vertical, 0);
            targetVelocity = sphere.TransformDirection(targetVelocity);
            targetVelocity *= character.GetAbilityValue("climb");
            var velocity = sphereRigidbody.velocity;
            var velocityChange = (targetVelocity - velocity);
            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
            velocityChange.y = Mathf.Clamp(velocityChange.y, -maxVelocityChange, maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
            sphereRigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
        }
        else
        {
            // We apply gravity manually for more tuning control
            float gravityForceDown = wallGravity * sphereRigidbody.mass;
            sphereRigidbody.AddForce(sphere.up * -gravityForceDown);
        }
        beenReset = false;

        // We apply gravity manually for more tuning control
        float gravityForce = wallGravity * sphereRigidbody.mass;
        sphereRigidbody.AddForce(sphere.forward * gravityForce);

        // Check if Sphere is outside the desired bounds
        float x = sphere.position.x > max.x ? max.x : sphere.position.x;
        x = x < min.x ? min.x : x;
        float y = sphere.position.y > max.y ? max.y : sphere.position.y;
        y = y < min.y ? min.y : y;
        float z = sphere.position.z > max.z ? max.z : sphere.position.z;
        z = z < min.z ? min.z : z;
        if (x != sphere.position.x ||
            y != sphere.position.y ||
            z != sphere.position.z)
        {
            sphere.position = new Vector3(x, y, z);
            sphereRigidbody.velocity = Vector3.zero; sphereRigidbody.angularVelocity = Vector3.zero;
        }

        bool shouldHang = Raycast(graphics.Find("Armature/Hip").position, transform.forward, .1f, ~0, Color.gray).collider == null;
        if (hanging != shouldHang)
        {
            hanging = shouldHang;
            SetHanging(shouldHang);
        }

        onWall = false;
    }

    private RaycastHit RaycastSpread(Vector3 origin, Vector3 direction, float distance, LayerMask layerMask, Color color, Transform holdCollidedWith)
    {
        RaycastHit hit = Raycast(origin, direction, distance, layerMask, color);
        if (hit.collider != null)
        {
            return hit;
        }

        float[] priorityMods = new float[] {
            radius * -.5f,
            radius * -.4f,
            radius * -.3f,
            radius * -.2f,
            radius * -.1f,
            0,
            radius * .1f,
            radius * .2f,
            radius * .3f,
            radius * .4f,
            radius * .5f,
        };
        float[] secondaryMods = new float[] {
            radius * -.5f,
            radius * -.4f,
            radius * -.3f,
            radius * -.2f,
            radius * -.1f,
            0,
            radius * .1f,
            radius * .2f,
            radius * .3f,
            radius * .4f,
            radius * .5f,
        };

        if (vertical != 0 || horizontal == 0)
        {
            // Vertical is Prority
            if (vertical < 0) { priorityMods = priorityMods.Reverse().ToArray(); }
            if (horizontal < 0) { secondaryMods = secondaryMods.Reverse().ToArray(); }
        }
        else
        {
            // Horizontal is Priority
            if (vertical < 0) { secondaryMods = secondaryMods.Reverse().ToArray(); }
            if (horizontal < 0) { priorityMods = priorityMods.Reverse().ToArray(); }
        }

        foreach (float priority in priorityMods)
        {
            foreach (float secondary in secondaryMods)
            {
                if (priority == 0 && secondary == 0)
                {
                    // Checked before nested for loops
                    continue;
                }

                Vector3 originV2 = origin;
                if (vertical != 0 || horizontal == 0)
                {
                    originV2 = origin + sphere.right * secondary + sphere.up * priority;
                }
                else
                {
                    originV2 = origin + sphere.right * priority + sphere.up * secondary;
                }
                hit = Raycast(originV2, direction, distance, layerMask, color);

                // Just incase the raycast picks up a different hold
                if (hit.transform != holdCollidedWith)
                {
                    continue;
                }

                if (hit.collider != null && hit.transform == collider.transform)
                {
                    float distanceFromSpehere = Vector3.Distance(hit.point, transform.position);
                    if (distanceFromSpehere > ignoreSameHoldDistance)
                    {
                        return hit;
                    }
                }
                else if (hit.collider != null)
                {
                    return hit;
                }
            }
        }
        return Raycast(origin, direction, distance, layerMask, color);
    }

    private void ResetSphereLocation()
    {
        beenReset = true;
        sphere.forward = transform.forward;
        sphere.position = transform.position + (transform.forward * -.15f);
        sphereRigidbody.velocity = Vector3.zero;
        sphereRigidbody.angularVelocity = Vector3.zero;
    }


    private void SetClimbing(bool climbing)
    {
        animator.SetBool("climbing", climbing);
    }
    private void SetHanging(bool hanging)
    {
        animator.SetBool("hanging", hanging);
    }


    private class SphereMonoBehaviour : MonoBehaviour
    {
        ClimbMovementManager climbMovementManager;

        // Use this for initialization
        void Start()
        {

        }

        internal void SetClimbMovementManager(ClimbMovementManager climbMovementManager)
        {
            this.climbMovementManager = climbMovementManager;
        }

        private void OnCollisionStay(Collision collision)
        {
            climbMovementManager.OnCollisionStay(collision);
        }

        private void OnTriggerEnter(Collider collision)
        {
            climbMovementManager.OnTriggerStay(collision);
        }

        private void OnTriggerStay(Collider other)
        {
            climbMovementManager.OnTriggerStay(other);

        }

    }
}
