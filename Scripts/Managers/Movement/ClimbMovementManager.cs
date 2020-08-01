using UnityEngine;
using System.Collections;
using TMPro;

public class ClimbMovementManager : AbstractMovementManager
{
    public static readonly string HOLD_NAME = "Hold";
    public static readonly string TOP_OUT_NAME = "TopOut";
    SphereMonoBehaviour sphere;
    float radius = .2f;
    float wallGravity = 20f;
    float wantsToTopOutCount = 10;
    float reach, topOutCounter;
    bool needsReset, onWall, topOut;
    Vector3 max, min;

    public ClimbMovementManager(AbstractMovementManager movementManager) : base(movementManager)
    {
        // Set State
        SetCurrentState(MovementState.Climb);

        // Set Animation
        SetClimbing(true);

        // Character's Reach
        reach = character.GetTraitValue("height") * .45f;

        // Disable Player Colliders and Rigidbody
        graphics.GetComponent<CapsuleCollider>().enabled = false;
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;

        // Add Sphere Game Object
        GameObject sphereGameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphereGameObject.transform.parent = transform;
        sphereGameObject.transform.localScale *= radius * 2; // diamiter

        // Set Sphere Mono Behavior to handle Collision and Trigger Events
        sphere = sphereGameObject.AddComponent<SphereMonoBehaviour>();
        sphere.SetClimbMovementManager(this);

        // Set Hold
        SetHitInfo(collider, pointOfContact);
    }

    public override bool AttemptSetDown(bool desired)
    {
        if (desired)
        {
            // Set Position off the wall to avoid collision
            transform.position = character.getBodyPartHead(graphics).position;
            SetIntendedState(MovementState.Ground);
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
        onWall = true;
    }

    public override void OnTriggerStay(Collider other)
    {
        if (other.transform == collider.transform && radius > Vector3.Distance(other.transform.position, pointOfContact))
        {
            return;
        }
        if (other.gameObject.name.Contains(HOLD_NAME))
        {
            RaycastHit hit = Raycast(sphere.transform.position, other.transform.forward, radius, DEFAULT, Color.yellow);
            if (hit.collider != null && hit.collider.transform == other.transform)
            {
                SetHitInfo(hit.collider, hit.point);
            }
            else
            {
                Debug.Log("Did not Hit Hold: " + other.gameObject.name);
            }
        }
    }

    public override void SetHitInfo(Collider collider, Vector3 pointOfContact)
    {
        base.SetHitInfo(collider, pointOfContact);
        if (collider.name.Contains(TOP_OUT_NAME))
        {
            topOut = true;
            topOutCounter = 0;
        }
        transform.forward = collider.transform.forward;
        transform.position = pointOfContact;
        min = new Vector3(pointOfContact.x - reach, pointOfContact.y - reach, pointOfContact.z - reach);
        max = new Vector3(pointOfContact.x + reach, pointOfContact.y + reach, pointOfContact.z + reach);
        ResetSphere();
    }

    public override void UpdateAnimation()
    {
        //throw new System.NotImplementedException();
    }

    public override void UpdateMovement(float horizontal, float vertical, Vector3 lookDirection)
    {
        if (topOut && vertical > 0)
        {
            if (topOutCounter > wantsToTopOutCount)
            {
                SetIntendedState(MovementState.Ground);
            }
            else
            {
                topOutCounter++;
            }

            return;

        }
        else if (topOut)
        {
            topOutCounter = 0;
        }

        if (horizontal == 0 && vertical == 0)
        {
            if (needsReset)
            {
                ResetSphere();
            }
            return;
        }
        else if (onWall)
        {
            Vector3 targetVelocity = new Vector3(horizontal, vertical, 0);
            targetVelocity = sphere.transform.TransformDirection(targetVelocity);
            targetVelocity *= character.GetAbilityValue("climb");
            var velocity = sphere.rigidbody.velocity;
            var velocityChange = (targetVelocity - velocity);
            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
            velocityChange.y = Mathf.Clamp(velocityChange.y, -maxVelocityChange, maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
            sphere.rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
        }
        else
        {
            // We apply gravity manually for more tuning control
            float gravityForceDown = wallGravity * sphere.rigidbody.mass;
            sphere.rigidbody.AddForce(sphere.transform.up * -gravityForceDown);
        }

        // We apply gravity manually for more tuning control
        float gravityForce = wallGravity * sphere.rigidbody.mass;
        sphere.rigidbody.AddForce(sphere.transform.forward * gravityForce);

        // Check if Sphere is outside the desired bounds
        float x = sphere.transform.position.x > max.x ? max.x : sphere.transform.position.x;
        x = x < min.x ? min.x : x;
        float y = sphere.transform.position.y > max.y ? max.y : sphere.transform.position.y;
        y = y < min.y ? min.y : y;
        float z = sphere.transform.position.z > max.z ? max.z : sphere.transform.position.z;
        z = z < min.z ? min.z : z;
        if (x != sphere.transform.position.x ||
            y != sphere.transform.position.y ||
            z != sphere.transform.position.z)
        {
            sphere.transform.position = new Vector3(x, y, z);
            sphere.rigidbody.velocity = Vector3.zero; sphere.rigidbody.angularVelocity = Vector3.zero;
        }

        needsReset = true;
        onWall = false;
    }

    private void ResetSphere()
    {
        needsReset = false;
        sphere.transform.forward = transform.forward;
        sphere.transform.position = transform.position + (transform.forward * -.15f);
        sphere.rigidbody.velocity = Vector3.zero;
        sphere.rigidbody.angularVelocity = Vector3.zero;
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
        public new Rigidbody rigidbody;
        ClimbMovementManager climbMovementManager;

        private void Awake()
        {
            rigidbody = gameObject.AddComponent<Rigidbody>();
            rigidbody.angularDrag = 0;
            rigidbody.mass = 0;
            rigidbody.freezeRotation = true;
            rigidbody.useGravity = false;
        }

        internal void SetClimbMovementManager(ClimbMovementManager climbMovementManager)
        {
            this.climbMovementManager = climbMovementManager;
        }

        private void OnCollisionStay(Collision collision)
        {
            climbMovementManager.OnCollisionStay(collision);
        }

        private void OnTriggerStay(Collider other)
        {
            climbMovementManager.OnTriggerStay(other);

        }

    }
}
