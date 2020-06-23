using System;
using System.Linq;
using UnityEngine;

public class ClimbMovementManagerV0 : AbstractMovementManager
{
    private Transform sphere;
    private Rigidbody sphereRigidbody;

    private LayerMask hold = LayerMask.NameToLayer("Hold");

    float gravity = 12.0f;
    float radius = .4f;

    // Trackers
    bool beenReset, onWall, hanging;
    float reach, horizontal, vertical;
    Vector3 max, min;

    public ClimbMovementManagerV0(AbstractMovementManager movementManager) : base(movementManager)
    {
        currentState = MovementState.Climb;
        intendedState = MovementState.Climb;

        // Clean up Current Object
        graphics.GetComponent<CapsuleCollider>().enabled = false;
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        SetClimbing(true);

        // Add new Game Object
        GameObject sphereGameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        SphereMonoBehaviour smb = sphereGameObject.AddComponent<SphereMonoBehaviour>();
        //smb.SetClimbMovementManager(this);
        sphere = sphereGameObject.transform;
        sphere.localScale *= radius;
        sphere.parent = transform;

        // Set sphereRigidbody
        sphereRigidbody = sphereGameObject.AddComponent<Rigidbody>();
        sphereRigidbody.angularDrag = 0;
        sphereRigidbody.useGravity = false;
        sphereRigidbody.mass = 0;
        sphereRigidbody.freezeRotation = true;

        // Character's Reach
        reach = character.GetTraitValue("height") * .45f;

        // Set RayCast/Hold
        SetRaycastHit(raycastHit);
    }

    //public ClimbMovementManager(Character character, Transform transform, Transform graphics) : base(character, transform)
    //{
    //    currentState = MovementState.Climb;

    //    SetGraphics(graphics);

    //    // Clean up Current Object
    //    graphics.GetComponent<CapsuleCollider>().enabled = false;
    //    sphereRigidbody.velocity = Vector3.zero;
    //    sphereRigidbody.angularVelocity = Vector3.zero;
    //    SetClimbing(true);

    //    // Add new Game Object
    //    GameObject sphereGameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //    SphereMonoBehaviour smb = sphereGameObject.AddComponent<SphereMonoBehaviour>();
    //    smb.SetClimbMovementManager(this);
    //    sphere = sphereGameObject.transform;
    //    sphere.localScale *= .3f;
    //    sphere.parent = transform;

    //    // Set sphereRigidbody
    //    sphereRigidbody = sphereGameObject.AddComponent<sphereRigidbody>();
    //    sphereRigidbody.angularDrag = 0;
    //    sphereRigidbody.useGravity = false;
    //    sphereRigidbody.mass = 0;
    //    sphereRigidbody.freezeRotation = true;

    //    // Character's Reach
    //    reach = character.GetTraitValue("height") * .45f;
    //}

    public override bool AttemptSetDown(bool desired)
    {
        if (desired)
        {
            intendedState = MovementState.Ground;
        }
        return desired;
    }

    public override void CleanUp()
    {
        GameObject.Destroy(sphere.gameObject);
        SetClimbing(false);
        SetHanging(false);
        transform.position = character.getBodyPartHead(graphics).position;
        graphics.GetComponent<CapsuleCollider>().enabled = true;
    }

    public override void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == DEFAULT_INDEX)
        {
            intendedState = MovementState.Ground;
        }
        onWall = true;
    }

    public override void OnTriggerStay(Collider collision)
    {
        float distance = Mathf.Abs(Vector3.Distance(raycastHit.point, sphere.position));
        if (collision.transform == raycastHit.collider.transform &&
            .3f > distance)
        {
            return;
        }

        if (collision.gameObject.layer == hold)
        {
            // Change forward direction to look in the direction of the hold. This solves for a hold being on a different angle than the current player/sphere
            Vector3 forward = sphere.forward;
            sphere.forward = collision.transform.forward;

            // Raycase, in a plus formation, for the hold the sphere triggered
            RaycastHit hitInfo = RaycastPlus(sphere.position, sphere.forward, .2f, HOLD, Color.yellow);
            sphere.forward = forward;
            if (hitInfo.collider != null)
            {
                SetRaycastHit(hitInfo);
            }
            else
            {
                Debug.Log("Error getting hold");
            }
        }
        else
        {
            Debug.Log("Dammit");
        }
    }

    public override void UpdateAnimation()
    {
        // TODO move Player
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
        beenReset = false;


        // We apply gravity manually for more tuning control
        float gravityForce = gravity * sphereRigidbody.mass;
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
            hanging = true;
            SetHanging(true);
        }

        onWall = false;
    }

    private void SetHanging(bool hanging)
    {
        animator.SetBool("hanging", hanging);
    }

    public override void SetRaycastHit(RaycastHit raycastHit)
    {
        base.SetRaycastHit(raycastHit);
        sphere.forward = raycastHit.collider.transform.forward;
        transform.forward = raycastHit.collider.transform.forward;
        transform.position = raycastHit.point;
        min = new Vector3(raycastHit.point.x - reach, raycastHit.point.y - reach, raycastHit.point.z - reach);
        max = new Vector3(raycastHit.point.x + reach, raycastHit.point.y + reach, raycastHit.point.z + reach);
        ResetSphereLocation();
    }

    private void ResetSphereLocation()
    {
        beenReset = true;
        sphere.position = transform.position + (transform.forward * -.15f);
        sphereRigidbody.velocity = Vector3.zero;
        sphereRigidbody.angularVelocity = Vector3.zero;
    }

    private RaycastHit RaycastPlus(Vector3 origin, Vector3 direction, float distance, LayerMask layerMask, Color color)
    {
        RaycastHit hit = Raycast(origin, direction, distance, layerMask, color);
        if (hit.collider != null && hit.transform != raycastHit.transform)
        {
            return hit;
        }


        float ignoreSameHold = radius * .5f;
        float[] priorityMods = new float[] { ignoreSameHold, radius * .25f, 0, radius * -.25f, -ignoreSameHold };
        float[] secondaryMods = new float[] { ignoreSameHold, radius * .25f, 0, radius * -.25f, -ignoreSameHold };


        if (vertical != 0 || horizontal == 0)
        {
            // Vertical is Prority
            if (vertical < 0)
            {
                priorityMods = priorityMods.Reverse().ToArray();
            }
            if (horizontal < 0)
            {
                secondaryMods = secondaryMods.Reverse().ToArray();
            }
        }
        else
        {
            // Horizontal is Priority
            if (vertical < 0)
            {
                secondaryMods = secondaryMods.Reverse().ToArray();
            }
            if (horizontal < 0)
            {
                priorityMods = priorityMods.Reverse().ToArray();
            }
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
                if (hit.collider != null && hit.transform == raycastHit.transform)
                {
                    if (Mathf.Abs(priority) < ignoreSameHold && Mathf.Abs(secondary) < ignoreSameHold)
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

    private void SetClimbing(bool climbing)
    {
        Debug.Log("Here");
        animator.SetBool("climbing", climbing);
    }

    private class SphereMonoBehaviour : MonoBehaviour
    {
        ClimbMovementManager climbMovementManager;

        private void OnCollisionStay(Collision collision)
        {
            Debug.Log("On Stay");
            climbMovementManager.OnCollisionStay(collision);
        }

        private void OnTriggerEnter(Collider collision)
        {
            Debug.Log("On Enter");
            climbMovementManager.OnTriggerStay(collision);
        }

        private void OnTriggerStay(Collider other)
        {
            climbMovementManager.OnTriggerStay(other);

        }

        internal void SetClimbMovementManager(ClimbMovementManager climbMovementManager)
        {
            this.climbMovementManager = climbMovementManager;
        }
    }
}
