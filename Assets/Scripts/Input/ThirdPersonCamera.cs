using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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
    [SerializeField, Range(0f, 1f)] private float cameraTouchStartScreenRatio = 0.5f;

    [Header("Touch Rules")]
    [SerializeField] private Joystick moveJoystick;
    [SerializeField] private RectTransform zoomArea;
    [SerializeField] private float simultaneousTouchWindow = 0.12f;

    [Header("UI Layers")]
    [SerializeField] private LayerMask blockingUILayers;
    [SerializeField] private LayerMask joystickUILayers;

    private readonly HashSet<int> blockingUITouchIds = new();
    private readonly HashSet<int> knownTouchIds = new();
    private readonly List<RaycastResult> uiRaycastResults = new();

    private Vector3 currentFocusPoint;

    private float yaw;
    private float pitch = 20f;

    private float targetDistance;
    private float currentDistance;

    private int cameraTouchId = -1;

    private bool isPinchZooming;
    private int zoomTouchId0 = -1;
    private int zoomTouchId1 = -1;

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

        ResetTouchState();
        ClearTrackedTouches();
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

        bool mouseIsMoving =
            Mouse.current != null &&
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

        if (Mathf.Abs(zoomInput) < 0.1f)
            return;

        targetDistance -= zoomInput * zoomSpeed;
        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
    }

    private void HandleTouchInput()
    {
        if (!enableTouchInput || Touchscreen.current == null)
            return;

        UpdateTrackedTouches();

        if (GetActiveTouchCount() == 0)
        {
            ResetTouchState();
            ClearTrackedTouches();
            return;
        }

        if (isPinchZooming)
        {
            HandleActivePinchZoom();
            return;
        }

        if (TryStartPinchZoom())
            return;

        HandleRotateTouch();
    }

    private void UpdateTrackedTouches()
    {
        HashSet<int> aliveTouchIds = new();

        foreach (TouchControl touch in Touchscreen.current.touches)
        {
            if (!touch.press.isPressed)
                continue;

            int touchId = touch.touchId.ReadValue();
            aliveTouchIds.Add(touchId);

            if (knownTouchIds.Contains(touchId))
                continue;

            knownTouchIds.Add(touchId);

            Vector2 position = touch.position.ReadValue();

            if (IsScreenPointOnBlockingUI(position))
                blockingUITouchIds.Add(touchId);
        }

        blockingUITouchIds.RemoveWhere(id => !aliveTouchIds.Contains(id));
        knownTouchIds.RemoveWhere(id => !aliveTouchIds.Contains(id));
    }

    private void ClearTrackedTouches()
    {
        blockingUITouchIds.Clear();
        knownTouchIds.Clear();
    }

    private bool TryStartPinchZoom()
    {
        if (!TryGetTwoZoomCandidateTouches(out TouchControl touch0, out TouchControl touch1))
            return false;

        double timeDifference = System.Math.Abs(
            touch0.startTime.ReadValue() - touch1.startTime.ReadValue()
        );

        if (timeDifference > simultaneousTouchWindow)
            return false;

        isPinchZooming = true;

        zoomTouchId0 = touch0.touchId.ReadValue();
        zoomTouchId1 = touch1.touchId.ReadValue();

        cameraTouchId = -1;

        MobileTouchLock.IsZooming = true;
        MobileTouchLock.CameraTouchId = -1;

        if (moveJoystick != null)
            moveJoystick.ForceReset();

        ApplyPinchZoom(touch0, touch1);

        return true;
    }

    private void HandleActivePinchZoom()
    {
        TouchControl touch0 = GetTouchById(zoomTouchId0);
        TouchControl touch1 = GetTouchById(zoomTouchId1);

        if (touch0 == null || touch1 == null)
        {
            StopPinchZoom();
            return;
        }

        cameraTouchId = -1;

        MobileTouchLock.IsZooming = true;
        MobileTouchLock.CameraTouchId = -1;

        if (moveJoystick != null && moveJoystick.IsDragging)
            moveJoystick.ForceReset();

        ApplyPinchZoom(touch0, touch1);
    }

    private void StopPinchZoom()
    {
        isPinchZooming = false;
        zoomTouchId0 = -1;
        zoomTouchId1 = -1;

        MobileTouchLock.IsZooming = false;
    }

    private void HandleRotateTouch()
    {
        TouchControl cameraTouch = GetCameraTouch();

        if (cameraTouch == null)
        {
            TryStartCameraTouch();
            return;
        }

        if (!IsValidCameraTouch(cameraTouch))
        {
            cameraTouchId = -1;
            MobileTouchLock.CameraTouchId = -1;
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

            if (!IsValidCameraTouch(touch))
                continue;

            Vector2 position = touch.position.ReadValue();

            if (position.x < Screen.width * cameraTouchStartScreenRatio)
                continue;

            int touchId = touch.touchId.ReadValue();

            cameraTouchId = touchId;
            MobileTouchLock.CameraTouchId = touchId;

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

            if (touch.touchId.ReadValue() != cameraTouchId)
                continue;

            if (!IsValidCameraTouch(touch))
                break;

            return touch;
        }

        cameraTouchId = -1;
        MobileTouchLock.CameraTouchId = -1;

        return null;
    }

    private bool IsValidCameraTouch(TouchControl touch)
    {
        int touchId = touch.touchId.ReadValue();

        if (IsJoystickTouch(touch))
            return false;

        if (blockingUITouchIds.Contains(touchId))
            return false;

        return true;
    }

    private bool TryGetTwoZoomCandidateTouches(out TouchControl touch0, out TouchControl touch1)
    {
        touch0 = null;
        touch1 = null;

        foreach (TouchControl touch in Touchscreen.current.touches)
        {
            if (!touch.press.isPressed)
                continue;

            int touchId = touch.touchId.ReadValue();

            if (blockingUITouchIds.Contains(touchId))
                continue;

            if (!IsTouchInsideZoomArea(touch))
                continue;

            if (touch0 == null)
            {
                touch0 = touch;
                continue;
            }

            touch1 = touch;
            return true;
        }

        return false;
    }

    private bool IsTouchInsideZoomArea(TouchControl touch)
    {
        if (zoomArea == null)
            return true;

        Vector2 position = touch.position.ReadValue();

        return IsScreenPointInsideRect(zoomArea, position);
    }

    private bool IsScreenPointInsideRect(RectTransform rectTransform, Vector2 screenPosition)
    {
        if (rectTransform == null)
            return true;

        Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
        Camera eventCamera = null;

        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
            eventCamera = canvas.worldCamera;

        return RectTransformUtility.RectangleContainsScreenPoint(
            rectTransform,
            screenPosition,
            eventCamera
        );
    }

    private bool IsScreenPointOnBlockingUI(Vector2 screenPosition)
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        uiRaycastResults.Clear();
        EventSystem.current.RaycastAll(eventData, uiRaycastResults);

        for (int i = 0; i < uiRaycastResults.Count; i++)
        {
            GameObject hitObject = uiRaycastResults[i].gameObject;
            int layerMask = 1 << hitObject.layer;

            if ((joystickUILayers.value & layerMask) != 0)
                return false;

            if ((blockingUILayers.value & layerMask) != 0)
                return true;
        }

        return false;
    }

    private TouchControl GetTouchById(int touchId)
    {
        if (touchId < 0 || Touchscreen.current == null)
            return null;

        foreach (TouchControl touch in Touchscreen.current.touches)
        {
            if (!touch.press.isPressed)
                continue;

            if (touch.touchId.ReadValue() == touchId)
                return touch;
        }

        return null;
    }

    private bool IsJoystickTouch(TouchControl touch)
    {
        if (moveJoystick == null)
            return false;

        if (!moveJoystick.IsDragging)
            return false;

        return touch.touchId.ReadValue() == moveJoystick.ActivePointerId;
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

    private void ApplyPinchZoom(TouchControl touch0, TouchControl touch1)
    {
        Vector2 pos0 = touch0.position.ReadValue();
        Vector2 pos1 = touch1.position.ReadValue();

        Vector2 prev0 = pos0 - touch0.delta.ReadValue();
        Vector2 prev1 = pos1 - touch1.delta.ReadValue();

        float previousDistance = Vector2.Distance(prev0, prev1);
        float currentDistance = Vector2.Distance(pos0, pos1);

        float pinchDelta = currentDistance - previousDistance;

        targetDistance -= pinchDelta * touchZoomSpeed;
        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
    }

    private void ResetTouchState()
    {
        cameraTouchId = -1;

        isPinchZooming = false;
        zoomTouchId0 = -1;
        zoomTouchId1 = -1;

        MobileTouchLock.IsZooming = false;
        MobileTouchLock.CameraTouchId = -1;

        if (moveJoystick != null && moveJoystick.IsDragging)
            moveJoystick.ForceReset();
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
}