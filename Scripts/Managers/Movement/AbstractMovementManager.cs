using UnityEngine;
using UnityEditor;
using System;

public abstract class AbstractMovementManager
{
    public Animator animator;
    public Character character;
    public Rigidbody rigidbody;
    public Transform graphics;
    public Transform transform;

    // Layers
    protected readonly LayerMask DEFAULT = LayerMask.GetMask("Default");
    protected readonly LayerMask DEFAULT_INDEX = LayerMask.NameToLayer("Default");
    protected readonly LayerMask HOLD = LayerMask.GetMask("Hold");
    protected readonly LayerMask HOLD_INDEX = LayerMask.NameToLayer("Hold");
    protected readonly LayerMask WALL = LayerMask.GetMask("Wall");
    protected readonly LayerMask WALL_INDEX = LayerMask.NameToLayer("Wall");
    protected readonly LayerMask WATER = LayerMask.GetMask("Water");
    protected readonly LayerMask WATER_INDEX = LayerMask.NameToLayer("Water");

    // Properties
    protected readonly float maxVelocityChange = 10.0f;
    protected readonly float speedSmoothTime = 0.1f;
    protected readonly float turnSmoothTime = 0.1f;

    // Trackers
    public bool down, quick, up;
    public float gravity;
    protected float velocityTurnSmooth;
    protected Collider collider;
    protected Vector3 pointOfContact;
    private MovementState currentState;
    private MovementState intendedState;

    public enum MovementState
    {
        Climb, Fly, Ground, Swim, Unkown
    }

    public static AbstractMovementManager GetMovementManager(AbstractMovementManager movementManager)
    {
        if (movementManager.intendedState == MovementState.Climb)
        {
            Debug.Log("Climbing");
            movementManager = new ClimbMovementManager(movementManager);
        }
        else if (movementManager.intendedState == MovementState.Fly)
        {
            Debug.Log("Flying");
            movementManager = new FlyMovementManager(movementManager);
        }
        else if (movementManager.intendedState == MovementState.Ground)
        {
            Debug.Log("Grounding");
            movementManager = new GroundMovementManager(movementManager);
        }
        else if (movementManager.intendedState == MovementState.Swim)
        {
            Debug.Log("Swimming");
            movementManager = new SwimMovementManager(movementManager);
        }
        else
        {
            throw new System.NotImplementedException("Movement State not implemented=" + movementManager.intendedState);
        }
        return movementManager;
    }

    protected AbstractMovementManager(AbstractMovementManager movementManager)
    {
        animator = movementManager.animator;
        character = movementManager.character;
        rigidbody = movementManager.rigidbody;
        graphics = movementManager.graphics;
        transform = movementManager.transform;
        down = movementManager.down;
        quick = movementManager.quick;
        up = movementManager.up;
        gravity = movementManager.gravity;
        collider = movementManager.collider;
        pointOfContact = movementManager.pointOfContact;
        intendedState = movementManager.intendedState;
        currentState = movementManager.currentState;
        movementManager.CleanUp();
    }

    public AbstractMovementManager(Character character, Transform transform)
    {
        this.character = character;
        this.transform = transform;
        this.rigidbody = transform.GetComponent<Rigidbody>();
    }

    public abstract void OnCollisionStay(Collision collision);

    public abstract void OnTriggerStay(Collider other);

    public abstract void UpdateAnimation();

    public abstract void UpdateMovement(float horizontal, float vertical, Vector3 lookDirection);

    public virtual bool AttemptInteract(Vector3 lookingDirection)
    {
        return false;
    }

    public virtual bool AttemptSetDown(bool desired)
    {
        this.down = desired;
        return true;
    }

    public virtual bool AttemptSetQuick(bool desired)
    {
        this.quick = desired;
        return true;
    }

    public virtual bool AttemptSetUp(bool desired)
    {
        this.up = desired;
        return true;
    }

    public virtual void CleanUp()
    {
        // Override in child
    }

    public virtual void SetGraphics(Transform graphics)
    {
        this.graphics = graphics;
        animator = graphics.GetComponent<Animator>();
    }

    public virtual void SetHitInfo(RaycastHit raycastHit)
    {
        SetHitInfo(raycastHit.collider, raycastHit.point);
    }

    public virtual void SetHitInfo(Collider collider, Vector3 pointOfContact)
    {
        this.collider = collider;
        this.pointOfContact = pointOfContact;
    }

    public virtual bool StateChanged()
    {
        return currentState != intendedState;
    }

    protected RaycastHit Raycast(Vector3 origin, Vector3 direction, float distance, LayerMask layerMask, Color color)
    {
        RaycastHit hit;
        Physics.Raycast(origin, direction, out hit, distance, layerMask.value);
        Debug.DrawRay(origin, direction * distance, color, 2, false);
        return hit;
    }

    protected void SetCurrentState(MovementState currentState)
    {
        this.currentState = currentState;
    }

    protected void SetIntendedState(MovementState intendedState)
    {
        this.intendedState = intendedState;
    }

    protected void SetSpeedForward(float speedForward, float dampTime)
    {
        animator.SetFloat("speedForward", speedForward, dampTime, Time.deltaTime);
    }

    protected void SetSpeedRight(float speedRight, float dampTime)
    {
        animator.SetFloat("speedRight", speedRight, dampTime, Time.deltaTime);
    }

    protected void SetSpeedUp(float speedUp, float dampTime)
    {
        animator.SetFloat("speedUp", speedUp, dampTime, Time.deltaTime);
    }

    protected bool TryClimbing(Vector3 lookingDirection)
    {
        float climbingSkill = character.GetAbilityValue("climb");
        if (0 == climbingSkill)
        {
            // Player cannot climb
            return false;
        }

        Vector3 origin = character.getBodyPartHead(graphics).position;
        RaycastHit hit = Raycast(origin, lookingDirection, character.GetTraitValue("height") * .8f, DEFAULT, Color.green);
        if (hit.collider != null)
        {
            Climbable climbable = hit.collider.gameObject.GetComponent<Climbable>();

            if (climbable != null && climbingSkill >= climbable.difficulty)
            {
                collider = hit.collider;
                pointOfContact = hit.point;
                return true;
            }
        }

        return false;
    }

    protected bool TryFlying()
    {
        return 0 != character.GetAbilityValue("fly");
    }

    protected bool TrySwiming(Collider other)
    {
        return other.gameObject.layer == WATER_INDEX
           && other.transform.position.y > graphics.Find("Armature/Hip/Stomach/Chest").position.y
           && 0 != character.GetAbilityValue("swim") ? true : false;
    }
}