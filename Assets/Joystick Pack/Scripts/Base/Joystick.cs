using UnityEngine;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public float Horizontal => IsControlLocked ? 0f : (snapX ? SnapFloat(input.x, AxisOptions.Horizontal) : input.x);
    public float Vertical => IsControlLocked ? 0f : (snapY ? SnapFloat(input.y, AxisOptions.Vertical) : input.y);
    public Vector2 Direction => IsControlLocked ? Vector2.zero : new Vector2(Horizontal, Vertical);

    public bool IsDragging => activePointerId >= 0;
    public bool IsControlLocked => MobileTouchLock.IsZooming;
    public int ActivePointerId => activePointerId;

    [Header("Value")]
    [SerializeField] private float handleRange = 1f;
    [SerializeField] private float deadZone = 0f;
    [SerializeField] private AxisOptions axisOptions = AxisOptions.Both;
    [SerializeField] private bool snapX = false;
    [SerializeField] private bool snapY = false;

    [Header("Components")]
    [SerializeField] protected RectTransform background;
    [SerializeField] private RectTransform handle;

    [Header("Touch Areas")]
    [SerializeField] private RectTransform pressArea;
    [SerializeField] private RectTransform limitArea;

    private RectTransform baseRect;
    private Canvas canvas;
    private Camera cam;

    protected Vector2 input;

    private int activePointerId = -1;

    protected virtual void Start()
    {
        handleRange = Mathf.Abs(handleRange);
        deadZone = Mathf.Abs(deadZone);

        baseRect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
            Debug.LogError($"{name}: Joystick must be inside a Canvas.");

        if (background == null)
            Debug.LogError($"{name}: Background is missing.");

        if (handle == null)
            Debug.LogError($"{name}: Handle is missing.");

        Vector2 center = new Vector2(0.5f, 0.5f);

        if (background != null)
            background.pivot = center;

        if (handle != null)
        {
            handle.anchorMin = center;
            handle.anchorMax = center;
            handle.pivot = center;
            handle.anchoredPosition = Vector2.zero;
        }
    }

    protected virtual void OnDisable()
    {
        ForceReset();
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (MobileTouchLock.IsZooming)
            return;

        // Nếu camera đang rotate, joystick không được cướp touch.
        if (MobileTouchLock.HasCamera)
            return;

        if (!IsInsidePressArea(eventData))
            return;

        activePointerId = eventData.pointerId;
        MobileTouchLock.JoystickTouchId = eventData.pointerId;

        OnJoystickPressed(eventData);
        OnDrag(eventData);
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (MobileTouchLock.IsZooming)
        {
            ResetJoystick(eventData);
            return;
        }

        if (eventData.pointerId != activePointerId)
            return;

        if (!IsInsideLimitArea(eventData))
        {
            ResetJoystick(eventData);
            return;
        }

        UpdateCamera();

        if (background == null || handle == null || canvas == null)
            return;

        Vector2 center = RectTransformUtility.WorldToScreenPoint(cam, background.position);
        Vector2 radius = background.rect.size / 2f;

        input = (eventData.position - center) / (radius * canvas.scaleFactor);

        FormatInput();
        HandleInput(input.magnitude, input.normalized, radius, cam);

        handle.anchoredPosition = input * radius * handleRange;
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId)
            return;

        ResetJoystick(eventData);
    }

    public void ForceReset()
    {
        if (MobileTouchLock.JoystickTouchId == activePointerId)
            MobileTouchLock.JoystickTouchId = -1;

        activePointerId = -1;
        input = Vector2.zero;

        if (handle != null)
            handle.anchoredPosition = Vector2.zero;
    }

    protected virtual void OnJoystickPressed(PointerEventData eventData)
    {
    }

    protected virtual void OnJoystickReset(PointerEventData eventData)
    {
    }

    protected virtual void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam)
    {
        if (magnitude <= deadZone)
        {
            input = Vector2.zero;
            return;
        }

        if (magnitude > 1f)
            input = normalised;
    }

    protected void ResetJoystick(PointerEventData eventData)
    {
        ForceReset();
        OnJoystickReset(eventData);
    }

    protected Vector2 ScreenPointToAnchoredPosition(Vector2 screenPosition)
    {
        UpdateCamera();

        if (baseRect == null || background == null)
            return Vector2.zero;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            baseRect,
            screenPosition,
            cam,
            out Vector2 localPoint))
        {
            Vector2 pivotOffset = baseRect.pivot * baseRect.rect.size;

            return localPoint
                   - (background.anchorMax * baseRect.rect.size)
                   + pivotOffset;
        }

        return Vector2.zero;
    }

    private bool IsInsidePressArea(PointerEventData eventData)
    {
        RectTransform area = pressArea != null
            ? pressArea
            : transform as RectTransform;

        return IsInsideArea(area, eventData);
    }

    private bool IsInsideLimitArea(PointerEventData eventData)
    {
        RectTransform area = limitArea != null
            ? limitArea
            : pressArea;

        if (area == null)
            area = transform as RectTransform;

        return IsInsideArea(area, eventData);
    }

    private bool IsInsideArea(RectTransform area, PointerEventData eventData)
    {
        if (area == null)
            return true;

        UpdateCamera();

        return RectTransformUtility.RectangleContainsScreenPoint(
            area,
            eventData.position,
            cam
        );
    }

    private void UpdateCamera()
    {
        cam = null;

        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
            cam = canvas.worldCamera;
    }

    private void FormatInput()
    {
        if (axisOptions == AxisOptions.Horizontal)
            input = new Vector2(input.x, 0f);
        else if (axisOptions == AxisOptions.Vertical)
            input = new Vector2(0f, input.y);
    }

    private float SnapFloat(float value, AxisOptions snapAxis)
    {
        if (value == 0f)
            return value;

        if (axisOptions == AxisOptions.Both)
        {
            float angle = Vector2.Angle(input, Vector2.up);

            if (snapAxis == AxisOptions.Horizontal)
            {
                if (angle < 22.5f || angle > 157.5f)
                    return 0f;

                return value > 0f ? 1f : -1f;
            }

            if (snapAxis == AxisOptions.Vertical)
            {
                if (angle > 67.5f && angle < 112.5f)
                    return 0f;

                return value > 0f ? 1f : -1f;
            }

            return value;
        }

        return value > 0f ? 1f : -1f;
    }
}

public enum AxisOptions
{
    Both,
    Horizontal,
    Vertical
}