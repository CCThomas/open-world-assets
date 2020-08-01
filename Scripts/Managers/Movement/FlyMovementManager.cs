using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyMovementManager : AbstractMovementManager
{

    public FlyMovementManager(AbstractMovementManager movementManager) : base(movementManager)
    {
        SetCurrentState(MovementState.Fly);
        SetSwimming(true);
    }

    public override void CleanUp()
    {
        SetSwimming(false);
    }

    public override void OnCollisionStay(Collision collision)
    {
        SetIntendedState(MovementState.Ground);
    }

    public override void OnTriggerStay(Collider other)
    {
        SetIntendedState(MovementState.Ground);
    }

    public override void UpdateAnimation()
    {
        //throw new System.NotImplementedException();
    }

    public override void UpdateMovement(float horizontal, float vertical, Vector3 lookDirection)
    {
        Vector3 targetVelocity = new Vector3(0, 0, 1);
        targetVelocity = transform.TransformDirection(targetVelocity);
        targetVelocity *= character.GetAbilityValue("fly");

        // Prepare Velocity
        var velocity = rigidbody.velocity;
        var velocityChange = (targetVelocity - velocity);
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);

        transform.forward = lookDirection;


        // Apply a force that attempts to reach our target velocity
        rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    private void SetSwimming(bool swimming)
    {
        animator.SetBool("swimming", swimming);
    }
}
