using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwimMovementManager : AbstractMovementManager
{
    bool submerged;

    public SwimMovementManager(AbstractMovementManager movementManager) : base(movementManager)
    {
        currentState = MovementState.Swim;
        intendedState = MovementState.Swim;
        SetSwimming(true);
        transform.position = new Vector3(transform.position.x, collider.transform.position.y, transform.position.z);
    }

    public override bool AttemptSetDown(bool desired)
    {
        if (desired && null == Raycast(transform.position, Vector3.down, .5f, ~0, Color.red).collider)
        {
            down = desired;
        }
        return down == desired;
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
        if (other.gameObject.layer == WATER_INDEX)
        {
            submerged = false;
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
                // Dive Down
                down = false;
                submerged = true;
                float verticalSpeed = Mathf.Sqrt(2 * character.GetAbilityValue("jump") * gravity);
                velocityChange.y = -verticalSpeed;
            }
            else
            {
                // Adjust Y Axis to simulate swimming being in water
                if (collider.transform.position.y < transform.position.y)
                {
                    velocityChange.y = -.01f;
                }
                else if (collider.transform.position.y > transform.position.y)
                {
                    velocityChange.y = .01f;
                }
                else
                {
                    velocityChange.y = 0;
                }
            }
        } else
        {
            // Similar to flying
            transform.forward = lookDirection;
        }

        // Apply a force that attempts to reach our target velocity
        rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);

        if (!submerged)
        {
            // Can Stand?
            RaycastHit hitinfo = Raycast(transform.position, Vector3.down, character.GetTraitValue("height") * .9f, DEFAULT, Color.red);
            if (hitinfo.collider != null)
            {
                collider = hitinfo.collider;
                pointOfContact = hitinfo.point;
                intendedState = MovementState.Ground;
            }
        }
    }

    private void SetSwimming(bool swimming)
    {
        animator.SetBool("swimming", swimming);
    }
}
