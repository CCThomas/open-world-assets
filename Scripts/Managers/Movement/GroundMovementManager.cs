using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundMovementManager : AbstractMovementManager
{
    float fallingBuffer = .5f;

    // Trackers
    bool grounded, slide, falling;
    float fallingTimeStart, timeSpentSliding;
    CapsuleCollider capsuleCollider;
    Vector3 animationSpeed;

    public GroundMovementManager(AbstractMovementManager movementManager) : base(movementManager)
    {
        SetCurrentState(MovementState.Ground);
        capsuleCollider = graphics.GetComponent<CapsuleCollider>();
    }

    public GroundMovementManager(Character character, Transform transform) : base(character, transform)
    {
        SetCurrentState(MovementState.Ground);
        SetIntendedState(MovementState.Ground);
    }

    public override bool AttemptInteract(Vector3 lookDirection)
    {
        if (TryClimbing(lookDirection))
        {
            SetIntendedState(MovementState.Climb);
            return true;
        }
        return false;
    }

    public override bool AttemptSetDown(bool desired)
    {
        if (slide)
        {
            return down == desired;
        }
        if (!desired && CanStandUp())
        {
            down = desired;
        }
        else if (desired)
        {
            down = desired;
            if (quick)
            {
                AttemptSetQuick(false);
                SetSliding(true);
            }
        }
        SetCrouching(down);

        if (down)
        {
            capsuleCollider.height = .8f;
            capsuleCollider.center = new Vector3(0, .5f * .8f, 0);
        }
        else if (!down)
        {
            capsuleCollider.height = 1;
            capsuleCollider.center = new Vector3(0, .5f, 0);
        }

        return down == desired;
    }

    public override bool AttemptSetQuick(bool desired)
    {
        if (slide)
        {
            return quick == desired;
        }
        if (up && desired)
        {
            // Do nothing
        }
        else if (desired && down && AttemptSetDown(false))
        {
            quick = desired;
        }
        else
        {
            quick = desired;
        }
        SetRunning(quick);
        return quick == desired;
    }

    public override bool AttemptSetUp(bool desired)
    {
        if (slide)
        {
            return up == desired;
        }
        if (desired && !up && 0 != character.GetAbilityValue("jump"))
        {
            if (grounded && AttemptSetDown(false))
            {
                up = true;
                AttemptSetQuick(false);
            }
            else if (!grounded && TryFlying())
            {
                SetIntendedState(MovementState.Fly);
            }
        }
        return up == desired;
    }

    public override void CleanUp()
    {
        quick = false;
        SetRunning(false);
        down = false;
        SetCrouching(false);
    }

    public override void OnCollisionStay(Collision collision)
    {
        fallingTimeStart = 0;
        grounded = true;
    }

    public override void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == WATER_INDEX
            && other.transform.position.y > graphics.Find("Armature/Hip/Stomach/Chest").position.y
            && 0 != character.GetAbilityValue("swim"))
        {
            this.collider = other;
            SetIntendedState(MovementState.Swim);
        }
    }

    public override void SetGraphics(Transform graphics)
    {
        base.SetGraphics(graphics);
        capsuleCollider = graphics.GetComponent<CapsuleCollider>();
    }

    public override void UpdateAnimation()
    {
        float animationSpeedPercentZ = animationSpeed.z;
        float animationSpeedPercentY = grounded ? 0 : rigidbody.velocity.y;
        float animationSpeedPercentX = animationSpeed.x;
        SetSpeedForward(animationSpeedPercentZ, speedSmoothTime);
        SetSpeedRight(animationSpeedPercentX, speedSmoothTime);
        SetSpeedUp(animationSpeedPercentY, speedSmoothTime);
    }

    public override void UpdateMovement(float horizontal, float vertical, Vector3 lookDirection)
    {
        if (slide && timeSpentSliding + .5 < Time.time)
        {
            SetSliding(false);
        }

        if (grounded)
        {
            Vector3 direction = transform.forward * vertical + transform.right * horizontal;
            RaycastHit hit = Raycast(transform.position + transform.up * character.GetTraitValue("height") * .5f, direction, capsuleCollider.radius * 1.5f, ~0, Color.red);
            if (hit.collider != null)
            {
                horizontal = 0;
                vertical = 0;
                falling = true;
            }
            else
            {
                falling = false;
            }
        }

        // Calculate how fast we should be moving
        animationSpeed.z = vertical;
        animationSpeed.x = horizontal;

        Vector3 targetVelocity = slide ? new Vector3(0, 0, 1) : new Vector3(horizontal, 0, vertical);

        if (grounded && targetVelocity.x == 0 && targetVelocity.z == 0)
        {
            if (quick)
            {
                AttemptSetQuick(false);
            }
            if (slide)
            {
                SetSliding(false);
            }
        }

        targetVelocity = transform.TransformDirection(targetVelocity);
        targetVelocity *= GetSpeed();

        // Prepare Velocity
        var velocity = rigidbody.velocity;
        var velocityChange = (targetVelocity - velocity);
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);

        // Rotate Player if not sliding
        if (!slide)
        {
            float targetRotation = Mathf.Atan2(lookDirection.x, lookDirection.z) * Mathf.Rad2Deg + lookDirection.y;
            transform.eulerAngles = (Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref velocityTurnSmooth, GetModifiedSmoothTime(turnSmoothTime)));
        }

        // If Player is on the gound and not falling
        if (grounded)
        {
            // Apply a force that attempts to reach our target velocity
            velocityChange.y = 0;
            rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);

            // Jump
            if (up)
            {
                up = false;
                if (!falling)
                {
                    float verticalSpeed = Mathf.Sqrt(2 * character.GetAbilityValue("jump") * gravity);
                    rigidbody.velocity = new Vector3(velocity.x, verticalSpeed, velocity.z);
                }
            }

        }
        else
        {
            if (CanGrabHold(lookDirection) && !down)
            {
                Debug.Log("Guru2");
                SetIntendedState(MovementState.Climb);
                return;
            }
        }

        // We apply gravity manually for more tuning control
        rigidbody.AddForce(new Vector3(0, -gravity * rigidbody.mass, 0));

        if (fallingTimeStart == 0)
        {
            fallingTimeStart = Time.time;
        }
        else if (fallingTimeStart + fallingBuffer > Time.time)
        {
            SetFalling();
        }
    }

    private bool CanGrabHold(Vector3 lookingDirection)
    {
        if (0 == character.GetAbilityValue("climb"))
        {
            // Player cannot climb
            return false;
        }

        Vector3 origin = character.getBodyPartHead(graphics).position;
        RaycastHit hit = Raycast(origin, lookingDirection, .8f, DEFAULT, Color.green);
        if (hit.collider != null && hit.collider.gameObject.name.Contains(ClimbMovementManager.HOLD_NAME))
        {
            collider = hit.collider;
            pointOfContact = hit.point;
            return true;
        }
        return false;
    }

    private bool CanStandUp()
    {
        Vector3 origin = transform.position;
        origin.y += .1f;
        Vector3 direction = Vector3.up.normalized;
        RaycastHit hit = Raycast(origin, direction, 1.7f, transform.gameObject.layer, Color.blue);
        return hit.collider == null;
    }

    private float GetModifiedSmoothTime(float smoothTime)
    {
        if (grounded)
        {
            return smoothTime;
        }

        float airControlPercent = character.GetAbilityValue("air_control_percent");
        if (airControlPercent == 0)
        {
            return float.MaxValue;
        }
        return smoothTime / airControlPercent;
    }

    private float GetSpeed()
    {
        float speed = character.GetAbilityValue("speed");
        if (slide || quick)
        {
            speed *= 1.7f;
        }
        else if (down)
        {
            speed *= .7f;
        }
        return speed;
    }

    private void SetCrouching(bool crouching)
    {
        animator.SetBool("crouching", crouching);
    }

    private void SetFalling()
    {
        grounded = false;
        if (down)
        {
            AttemptSetDown(false);
        }
    }

    private void SetRunning(bool running)
    {
        animator.SetBool("running", running);
    }

    private void SetSliding(bool sliding)
    {
        slide = sliding;
        animator.SetBool("sliding", sliding);
        if (slide)
        {
            timeSpentSliding = Time.time;
        }
    }
}
