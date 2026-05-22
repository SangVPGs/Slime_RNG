using UnityEngine;
using UnityEngine.EventSystems;

public class DynamicJoystick : Joystick
{
    public float MoveThreshold
    {
        get => moveThreshold;
        set => moveThreshold = Mathf.Abs(value);
    }

    [SerializeField] private float moveThreshold = 1f;

    private Vector2 startPosition;

    protected override void Start()
    {
        MoveThreshold = moveThreshold;

        base.Start();

        startPosition = background.anchoredPosition;
        background.gameObject.SetActive(true);
    }

    protected override void OnJoystickPressed(PointerEventData eventData)
    {
        background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
        background.gameObject.SetActive(true);
    }

    protected override void OnJoystickReset(PointerEventData eventData)
    {
        background.anchoredPosition = startPosition;
        background.gameObject.SetActive(true);
    }

    protected override void HandleInput(float magnitude, Vector2 normalised, Vector2 radius, Camera cam)
    {
        if (magnitude > moveThreshold)
        {
            Vector2 difference = normalised * (magnitude - moveThreshold) * radius;
            background.anchoredPosition += difference;
        }

        base.HandleInput(magnitude, normalised, radius, cam);
    }
}