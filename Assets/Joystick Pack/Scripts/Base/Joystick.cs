using UnityEngine;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public float Horizontal => snapX ? SnapFloat(input.x, AxisOptions.Horizontal) : input.x;
    public float Vertical => snapY ? SnapFloat(input.y, AxisOptions.Vertical) : input.y;
    public Vector2 Direction => new Vector2(Horizontal, Vertical);

    public float HandleRange
    {
        get => handleRange;
        set => handleRange = Mathf.Abs(value);
    }

    public float DeadZone
    {
        get => deadZone;
        set => deadZone = Mathf.Abs(value);
    }

    public AxisOptions AxisOptions
    {
        get => axisOptions;
        set => axisOptions = value;
    }

    public bool SnapX
    {
        get => snapX;
        set => snapX = value;
    }

    public bool SnapY
    {
        get => snapY;
        set => snapY = value;
    }

    [Header("Value")]
    [SerializeField] private float handleRange = 1f;
    [SerializeField] private float deadZone = 0f;
    [SerializeField] private AxisOptions axisOptions = AxisOptions.Both;
    [SerializeField] private bool snapX = false;
    [SerializeField] private bool snapY = false;

    [Header("Components")]
    [SerializeField] protected RectTransform background = null;
    [SerializeField] private RectTransform handle = null;

    [Header("Touch Areas")]
    [Tooltip("Vùng được phép bắt đầu chạm. Nếu để trống sẽ dùng RectTransform của chính joystick.")]
    [SerializeField] private RectTransform pressArea = null;

    [Tooltip("Vùng được phép kéo tiếp. Nếu kéo ra ngoài vùng này joystick sẽ reset. Nếu để trống sẽ dùng Press Area.")]
    [SerializeField] private RectTransform limitArea = null;

    private RectTransform baseRect = null;

    protected Canvas canvas;
    protected Camera cam;

    protected Vector2 input = Vector2.zero;

    private bool isDragging;

    protected virtual void Start()
    {
        HandleRange = handleRange;
        DeadZone = deadZone;

        baseRect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
            Debug.LogError("The Joystick is not placed inside a canvas");

        if (background == null)
            Debug.LogError("Joystick background is missing.");

        if (handle == null)
            Debug.LogError("Joystick handle is missing.");

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

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (!IsInsidePressArea(eventData))
            return;

        isDragging = true;

        OnJoystickPressed(eventData);

        OnDrag(eventData);
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        if (!IsInsideLimitArea(eventData))
        {
            ResetJoystick(eventData);
            return;
        }

        UpdateCamera();

        Vector2 position = RectTransformUtility.WorldToScreenPoint(cam, background.position);
        Vector2 radius = background.sizeDelta / 2f;

        input = (eventData.position - position) / (radius * canvas.scaleFactor);

        FormatInput();
        HandleInput(input.magnitude, input.normalized, radius, cam);

        handle.anchoredPosition = input * radius * handleRange;
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        ResetJoystick(eventData);
    }

    protected virtual void OnJoystickPressed(PointerEventData eventData)
    {

    }

    protected virtual void OnJoystickReset(PointerEventData eventData)
    {

    }

    protected virtual void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam)
    {
        if (magnitude > deadZone)
        {
            if (magnitude > 1f)
                input = normalised;
        }
        else
        {
            input = Vector2.zero;
        }
    }

    protected void ResetJoystick(PointerEventData eventData)
    {
        isDragging = false;

        input = Vector2.zero;

        if (handle != null)
            handle.anchoredPosition = Vector2.zero;

        OnJoystickReset(eventData);
    }

    protected bool IsInsidePressArea(PointerEventData eventData)
    {
        RectTransform area = pressArea;

        if (area == null)
            area = transform as RectTransform;

        return IsInsideArea(area, eventData);
    }

    protected bool IsInsideLimitArea(PointerEventData eventData)
    {
        RectTransform area = limitArea;

        if (area == null)
            area = pressArea;

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

        if (value > 0f)
            return 1f;

        if (value < 0f)
            return -1f;

        return 0f;
    }

    protected Vector2 ScreenPointToAnchoredPosition(Vector2 screenPosition)
    {
        UpdateCamera();

        Vector2 localPoint = Vector2.zero;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            baseRect,
            screenPosition,
            cam,
            out localPoint))
        {
            Vector2 pivotOffset = baseRect.pivot * baseRect.sizeDelta;

            return localPoint
                   - (background.anchorMax * baseRect.sizeDelta)
                   + pivotOffset;
        }

        return Vector2.zero;
    }
}

public enum AxisOptions
{
    Both,
    Horizontal,
    Vertical
}