using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwimMovementManager : AbstractMovementManager
{
    bool submerged;
    float submergedGracePeriod;

    public SwimMovementManager(AbstractMovementManager movementManager) : base(movementManager)
    {
        SetCurrentState(MovementState.Swim);
        SetSwimming(true);
        Debug.Log(rigidbody.velocity.y);
        if (rigidbody.velocity.y < -5f)
        {
            submerged = true;
        }
        else
        {
            transform.position = new Vector3(transform.position.x, collider.transform.position.y, transform.position.z);
        }
    }

    public override bool AttemptInteract(Vector3 lookingDirection)
    {
        //if (CanGrabHold(lookDirection))
        {
            Debug.Log("Here123");
            SetIntendedState(MovementState.Climb);
            return true;
        }
        return false;
    }

    public override bool AttemptSetDown(bool desired)
    {
        if (!submerged && null == Raycast(transform.position, Vector3.down, character.GetTraitValue("height") * .5f, ~0, Color.yellow).collider)
        {
            down = desired;
        }
        return down == desired;
    }

    public override bool AttemptSetUp(bool desired)
    {
        if (!submerged && desired && 0 != character.GetAbilityValue("fly"))
        {
            SetIntendedState(MovementState.Fly);
            transform.position = new Vector3(transform.position.x, transform.position.y + .1f, transform.position.z);
        }
        return desired;
    }

    public override void CleanUp()
    {
        SetSwimming(false);
    }

    public override void OnCollisionStay(Collision collision)
    {
        //throw new System.NotImplementedException();
    }

    public override void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == WATER_INDEX && Time.time > submergedGracePeriod
            && graphics.Find("Armature/Hip/Stomach/Chest").position.y > collider.transform.position.y)
        {
            submerged = false;
            transform.position = new Vector3(transform.position.x, collider.transform.position.y, transform.position.z);
        }
    }

    public override void UpdateAnimation()
    {
        //throw new System.NotImplementedException();
    }

    public override void UpdateMovement(float horizontal, float vertical, Vector3 lookDirection)
    {
        Vector3 targetVelocity = new Vector3(horizontal, 0, vertical);
        targetVelocity = transform.TransformDirection(targetVelocity);
        targetVelocity *= character.GetAbilityValue("swim");

        // Prepare Velocity
        var velocity = rigidbody.velocity;
        var velocityChange = (targetVelocity - velocity);
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);


        if (!submerged)
        {
            // Rotate Player
            float targetRotation = Mathf.Atan2(lookDirection.x, lookDirection.z) * Mathf.Rad2Deg + lookDirection.y;
            transform.eulerAngles = (Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref velocityTurnSmooth, turnSmoothTime));

            if (down)
            {
                Debug.Log("Dive");
                // Dive Down
                down = false;
                submerged = true;
                submergedGracePeriod = Time.time + 1;
                float verticalSpeed = Mathf.Sqrt(2 * character.GetAbilityValue("jump") * gravity);
                velocityChange.y = -verticalSpeed * 2;
            }
        }
        else
        {
            // Similar to flying
            transform.forward = lookDirection;
        }

        // Apply a force that attempts to reach our target velocity
        rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);

        if (!submerged)
        {
            // Can Stand?
            RaycastHit hitinfo = Raycast(transform.position, Vector3.down, character.GetTraitValue("height") * .5f, DEFAULT, Color.red);
            if (hitinfo.collider != null)
            {
                collider = hitinfo.collider;
                pointOfContact = hitinfo.point;
                SetIntendedState(MovementState.Ground);
            }
        }
    }

    private void SetSwimming(bool swimming)
    {
        animator.SetBool("swimming", swimming);
    }
}
