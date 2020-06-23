using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager
{
    // Provided
    Transform transform;
    Transform graphics;
    Character character;

    // Movement
    AbstractMovementManager movementManager;

    // Tracker
    string chunkTag;

    public CharacterManager(Character character, Transform transform)
    {
        this.transform = transform;
        this.character = character;
        UpdateMovementManager();
        movementManager.gravity = GameManager.GetGravity(chunkTag);
    }

    public void AttemptSetDown(bool desired)
    {
        movementManager.AttemptSetDown(desired);
    }

    public void AttemptSetQuick(bool desired)
    {
        movementManager.AttemptSetQuick(desired);
    }

    public void AttemptSetUp(bool desired)
    {
        movementManager.AttemptSetUp(desired);
    }

    public void AttemptInteract(Vector3 lookingDirection)
    {
        movementManager.AttemptInteract(lookingDirection);
    }

    public bool GetDown()
    {
        return movementManager.down;
    }

    public bool GetQuick()
    {
        return movementManager.quick;
    }

    public bool GetUp()
    {
        return movementManager.up;
    }

    public void OnCollisionStay(Collision collision)
    {
        string tag = collision.gameObject.tag;
        if (chunkTag != null || chunkTag != tag)
        {
            MapManager.PlayerChangedChunks(chunkTag, tag);
            movementManager.gravity = GameManager.GetGravity(tag);
            chunkTag = tag;
        }
        movementManager.OnCollisionStay(collision);
    }

    public void OnTriggerStay(Collider collider)
    {
        movementManager.OnTriggerStay(collider);
    }

    public void UpdateGraphics(Transform graphics)
    {
        this.graphics = graphics;
        this.graphics.gameObject.SetActive(true);
        movementManager.SetGraphics(graphics);
    }

    public void UpdateAnimation()
    {
        movementManager.UpdateAnimation();
    }

    public void UpdateMovement(float horizontal, float vertical, Vector3 lookDirection)
    {
        movementManager.UpdateMovement(horizontal, vertical, lookDirection);
        if (movementManager.StateChanged())
        {
            UpdateMovementManager();
        }
    }

    private void UpdateMovementManager()
    {
        if (movementManager == null)
        {
            movementManager = new GroundMovementManager(character, transform);
            movementManager.intendedState = AbstractMovementManager.MovementState.Ground;
        }
        else
        {
            if (movementManager.intendedState == AbstractMovementManager.MovementState.Climb)
            {
                movementManager = new ClimbMovementManager(movementManager);
            }
            else if (movementManager.intendedState == AbstractMovementManager.MovementState.Fly)
            {
                movementManager = new FlyMovementManager(movementManager);
            }
            else if (movementManager.intendedState == AbstractMovementManager.MovementState.Ground)
            {
                movementManager = new GroundMovementManager(movementManager);
            }
            else if (movementManager.intendedState == AbstractMovementManager.MovementState.Swim)
            {
                movementManager = new SwimMovementManager(movementManager);
            }
            else
            {
                throw new System.NotImplementedException("Movement State not implemented=" + movementManager.intendedState);
            }
        }
    }
}
