using UnityEngine;

public class CameraManager
{
    // Needed From Parent
    Camera playerCamera;
    Transform transform;
    Transform target;
    public LayerMask firstPersonLayers;
    public LayerMask thirdPersonLayers;

    // Properties
    float distanceFromTarget = 2f;
    float rotationSmoothTime = .12f;
    Vector2 pitchMinMax = new Vector2(-40, 85);
    Vector2 scrollMinMax = new Vector2(1f, 3f);

    // Trackers
    bool inverted = false;
    float yaw, pitch;
    CameraMode currentCameaMode;
    Vector3 rotationSmoothVelocity;
    Vector3 currentRotation;

    public CameraManager(Transform parent, LayerMask firstPersonLayers, LayerMask thirdPersonLayers)
    {
        GameObject cameraGameObject = new GameObject("Camera for " + parent.name);
        playerCamera = cameraGameObject.AddComponent<Camera>();
        cameraGameObject.transform.parent = parent;
        transform = cameraGameObject.transform;
        this.firstPersonLayers = firstPersonLayers;
        this.thirdPersonLayers = thirdPersonLayers;
        currentCameaMode = CameraMode.FirstPerson;

        // Lock Cursor to Game
        Cursor.lockState = CursorLockMode.Locked;

        UpdateCamera();
    }

    public void Rotate(float yawModifier, float pitchModifier)
    {
        yaw += yawModifier;
        pitch -= pitchModifier * (inverted ? -1 : 1);
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);
    }

    public void LateUpdate()
    {
        // Rotate Camera
        currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);
        transform.eulerAngles = currentRotation;

        // Move Camera to target location
        if (currentCameaMode == CameraMode.FirstPerson)
        {
            transform.position = target.position;
        }
        else if (currentCameaMode == CameraMode.ThirdPerson)
        {
            transform.position = target.position - transform.forward * distanceFromTarget;
        }
    }

    public void SwitchCamera()
    {
        if (currentCameaMode == CameraMode.ThirdPerson)
        {
            currentCameaMode = CameraMode.FirstPerson;
        }
        else
        {
            currentCameaMode = CameraMode.ThirdPerson;
        }
        UpdateCamera();
    }

    internal Vector3 LookingDirection()
    {
        return transform.forward;
    }

    void UpdateCamera()
    {
        if (currentCameaMode == CameraMode.FirstPerson)
        {
            playerCamera.cullingMask = firstPersonLayers;

        }
        else
        {
            playerCamera.cullingMask = thirdPersonLayers;
        }
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    internal void AdjustDistance(Vector2 scroll)
    {
        distanceFromTarget += scroll.y * .2f;
        if (distanceFromTarget < scrollMinMax.x)
        {
            distanceFromTarget = scrollMinMax.x;
        }
        else if (distanceFromTarget > scrollMinMax.y)
        {
            distanceFromTarget = scrollMinMax.y;
        }
    }

    public enum CameraMode
    {
        FirstPerson, ThirdPerson
    }
}
