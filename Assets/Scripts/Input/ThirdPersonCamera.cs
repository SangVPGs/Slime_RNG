using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 focusOffset = new Vector3(0f, 1.5f, 0f);

    [Header("Rotation")]
    [SerializeField] private float mouseSensitivity = 0.12f;
    [SerializeField] private float gamepadSensitivity = 120f;
    [SerializeField] private float touchSensitivity = 0.15f;
    [SerializeField] private float minPitch = -25f;
    [SerializeField] private float maxPitch = 65f;

    [Header("Zoom")]
    [SerializeField] private float defaultDistance = 10f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 20f;
    [SerializeField] private float zoomSpeed = 0.5f;
    [SerializeField] private float touchZoomSpeed = 0.01f;
    [SerializeField] private float zoomSmoothSpeed = 12f;

    [Header("Collision")]
    [SerializeField] private LayerMask collisionLayers;
    [SerializeField] private float collisionRadius = 0.25f;
    [SerializeField] private float collisionOffset = 0.2f;

    [Header("Smooth")]
    [SerializeField] private float followSmoothSpeed = 15f;

    [Header("Input")]
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference zoomAction;

    [Header("Mobile")]
    [SerializeField] private bool enableTouchInput = true;
    [SerializeField] private bool ignoreTouchOverUI = true;

    [Tooltip("Touch bắt đầu từ tỉ lệ này trở sang phải thì được dùng để xoay camera. 0.5 = nửa phải màn hình.")]
    [SerializeField, Range(0f, 1f)] private float cameraTouchStartScreenRatio = 0.5f;

    private Vector3 currentFocusPoint;

    private float yaw;
    private float pitch = 20f;

    private float targetDistance;
    private float currentDistance;

    private int cameraTouchId = -1;

    private void Awake()
    {
        targetDistance = defaultDistance;
        currentDistance = defaultDistance;

        if (target != null)
            currentFocusPoint = target.position + focusOffset;
    }

    private void OnEnable()
    {
        lookAction?.action.Enable();
        zoomAction?.action.Enable();
    }

    private void OnDisable()
    {
        lookAction?.action.Disable();
        zoomAction?.action.Disable();

        cameraTouchId = -1;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        HandleRotationInput();
        HandleZoomInput();
        HandleTouchInput();

        UpdateCameraPosition();
    }

    private void HandleRotationInput()
    {
        Vector2 lookInput = lookAction != null
            ? lookAction.action.ReadValue<Vector2>()
            : Vector2.zero;

        if (lookInput.sqrMagnitude < 0.01f)
            return;

        bool mouseIsMoving = Mouse.current != null &&
                             Mouse.current.delta.ReadValue().sqrMagnitude > 0.01f;

        float sensitivity = mouseIsMoving
            ? mouseSensitivity
            : gamepadSensitivity * Time.deltaTime;

        yaw += lookInput.x * sensitivity;
        pitch -= lookInput.y * sensitivity;

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    private void HandleZoomInput()
    {
        float zoomInput = zoomAction != null
            ? zoomAction.action.ReadValue<float>()
            : 0f;

        if (Mathf.Abs(zoomInput) < 0.01f)
            return;

        targetDistance -= zoomInput * zoomSpeed;
        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
    }

    private void HandleTouchInput()
    {
        if (!enableTouchInput)
            return;

        if (Touchscreen.current == null)
            return;

        int activeTouchCount = GetActiveTouchCount();

        if (activeTouchCount == 0)
        {
            cameraTouchId = -1;
            return;
        }

        if (TryGetPinchTouches(out TouchControl pinchTouch0, out TouchControl pinchTouch1))
        {
            cameraTouchId = -1;
            HandlePinchZoom(pinchTouch0, pinchTouch1);
            return;
        }

        TouchControl cameraTouch = GetCameraTouch();

        if (cameraTouch == null)
        {
            TryStartCameraTouch();
            return;
        }

        Vector2 delta = cameraTouch.delta.ReadValue();

        yaw += delta.x * touchSensitivity;
        pitch -= delta.y * touchSensitivity;

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    private void TryStartCameraTouch()
    {
        foreach (TouchControl touch in Touchscreen.current.touches)
        {
            if (!touch.press.isPressed)
                continue;

            int touchId = touch.touchId.ReadValue();
            Vector2 position = touch.position.ReadValue();

            if (position.x < Screen.width * cameraTouchStartScreenRatio)
                continue;

            if (ignoreTouchOverUI && IsPointerOverUI(touchId))
                continue;

            cameraTouchId = touchId;
            return;
        }
    }

    private TouchControl GetCameraTouch()
    {
        if (cameraTouchId < 0)
            return null;

        foreach (TouchControl touch in Touchscreen.current.touches)
        {
            if (!touch.press.isPressed)
                continue;

            if (touch.touchId.ReadValue() == cameraTouchId)
                return touch;
        }

        cameraTouchId = -1;
        return null;
    }

    private bool TryGetPinchTouches(out TouchControl touch0, out TouchControl touch1)
    {
        touch0 = null;
        touch1 = null;

        if (Touchscreen.current == null)
            return false;

        foreach (TouchControl touch in Touchscreen.current.touches)
        {
            if (!touch.press.isPressed)
                continue;

            int touchId = touch.touchId.ReadValue();

            if (ignoreTouchOverUI && IsPointerOverUI(touchId))
                continue;

            if (touch0 == null)
            {
                touch0 = touch;
            }
            else
            {
                touch1 = touch;
                return true;
            }
        }

        return false;
    }

    private void HandlePinchZoom(TouchControl touch0, TouchControl touch1)
    {
        if (touch0 == null || touch1 == null)
            return;

        if (!touch0.press.isPressed || !touch1.press.isPressed)
            return;

        Vector2 touch0Position = touch0.position.ReadValue();
        Vector2 touch1Position = touch1.position.ReadValue();

        Vector2 touch0PreviousPosition = touch0Position - touch0.delta.ReadValue();
        Vector2 touch1PreviousPosition = touch1Position - touch1.delta.ReadValue();

        float previousDistance = Vector2.Distance(
            touch0PreviousPosition,
            touch1PreviousPosition
        );

        float currentTouchDistance = Vector2.Distance(
            touch0Position,
            touch1Position
        );

        float pinchDelta = currentTouchDistance - previousDistance;

        targetDistance -= pinchDelta * touchZoomSpeed;
        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
    }

    private int GetActiveTouchCount()
    {
        if (Touchscreen.current == null)
            return 0;

        int count = 0;

        foreach (TouchControl touch in Touchscreen.current.touches)
        {
            if (touch.press.isPressed)
                count++;
        }

        return count;
    }

    private bool IsPointerOverUI(int pointerId)
    {
        if (UnityEngine.EventSystems.EventSystem.current == null)
            return false;

        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(pointerId);
    }

    private void UpdateCameraPosition()
    {
        Vector3 targetFocusPoint = target.position + focusOffset;

        currentFocusPoint = Vector3.Lerp(
            currentFocusPoint,
            targetFocusPoint,
            followSmoothSpeed * Time.deltaTime
        );

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 cameraDirection = rotation * Vector3.back;

        float correctedDistance = GetCorrectedDistance(
            currentFocusPoint,
            cameraDirection,
            targetDistance
        );

        currentDistance = Mathf.Lerp(
            currentDistance,
            correctedDistance,
            zoomSmoothSpeed * Time.deltaTime
        );

        Vector3 cameraPosition = currentFocusPoint + cameraDirection * currentDistance;

        transform.SetPositionAndRotation(cameraPosition, rotation);
    }

    private float GetCorrectedDistance(
        Vector3 focusPoint,
        Vector3 cameraDirection,
        float desiredDistance
    )
    {
        bool hitSomething = Physics.SphereCast(
            focusPoint,
            collisionRadius,
            cameraDirection,
            out RaycastHit hit,
            desiredDistance,
            collisionLayers,
            QueryTriggerInteraction.Ignore
        );

        if (!hitSomething)
            return desiredDistance;

        return Mathf.Clamp(
            hit.distance - collisionOffset,
            minDistance,
            desiredDistance
        );
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        if (target != null)
            currentFocusPoint = target.position + focusOffset;
    }
}