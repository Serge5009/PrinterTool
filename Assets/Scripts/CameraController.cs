using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float orbitSpeed = 0.5f;
    public float touchOrbitSpeed = 0.2f;
    public float zoomSpeed = 0.1f;
    public float pinchZoomSpeed = 1.5f;
    public float smoothing = 10f;

    [Header("Limits")]
    public float minDistance = 10f;
    public float maxDistance = 1500f;
    public float minPitch = 0f;
    public float maxPitch = 85f;

    private Camera cam;

    private Vector3 targetPivot;
    private float targetDistance = 300f;
    private float targetYaw = 45f;
    private float targetPitch = 30f;

    private Vector3 currentPivot;
    private float currentDistance;
    private float currentYaw;
    private float currentPitch;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        currentDistance = targetDistance;
        currentYaw = targetYaw;
        currentPitch = targetPitch;
        currentPivot = targetPivot;
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void LateUpdate()
    {
        HandleInput();
        UpdateCameraTransform();
    }

    private void HandleInput()
    {
        if (Touch.activeTouches.Count > 0)
        {
            if (Touch.activeTouches.Count == 1)
            {
                Vector2 delta = Touch.activeTouches[0].delta;
                targetYaw += delta.x * touchOrbitSpeed;
                targetPitch -= delta.y * touchOrbitSpeed;
            }
            else if (Touch.activeTouches.Count == 2)
            {
                var t0 = Touch.activeTouches[0];
                var t1 = Touch.activeTouches[1];

                float prevTouchDist = Vector2.Distance(t0.screenPosition - t0.delta, t1.screenPosition - t1.delta);
                float currTouchDist = Vector2.Distance(t0.screenPosition, t1.screenPosition);

                float deltaDist = currTouchDist - prevTouchDist;
                targetDistance -= deltaDist * pinchZoomSpeed;
            }
        }
        else if (Mouse.current != null)
        {
            if (Mouse.current.rightButton.isPressed || Mouse.current.middleButton.isPressed)
            {
                Vector2 delta = Mouse.current.delta.ReadValue();
                targetYaw += delta.x * orbitSpeed;
                targetPitch -= delta.y * orbitSpeed;
            }

            float scroll = Mouse.current.scroll.y.ReadValue();
            if (scroll != 0)
            {
                targetDistance -= scroll * zoomSpeed;
            }
        }

        targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);
        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
    }

    private void UpdateCameraTransform()
    {
        float t = Time.deltaTime * smoothing;
        currentYaw = Mathf.Lerp(currentYaw, targetYaw, t);
        currentPitch = Mathf.Lerp(currentPitch, targetPitch, t);
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, t);
        currentPivot = Vector3.Lerp(currentPivot, targetPivot, t);

        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);

        Vector3 position = currentPivot - (rotation * Vector3.forward * currentDistance);

        transform.position = position;
        transform.rotation = rotation;
    }

    public void FrameBounds(Bounds bounds, float marginMultiplier = 1.2f)
    {
        targetPivot = bounds.center;

        float boundingRadius = bounds.extents.magnitude;

        float fovRadians = cam.fieldOfView * Mathf.Deg2Rad;

        float requiredDistance = (boundingRadius * marginMultiplier) / Mathf.Sin(fovRadians / 2f);

        targetDistance = Mathf.Clamp(requiredDistance, minDistance, maxDistance);
    }
}