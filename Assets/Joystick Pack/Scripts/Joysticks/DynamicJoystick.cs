using UnityEngine;
using UnityEngine.EventSystems;

public class DynamicJoystick : Joystick
{
    private Vector2 startPosition;

    protected override void Start()
    {
        base.Start();

        if (background != null)
        {
            startPosition = background.anchoredPosition;
            background.gameObject.SetActive(true);
        }
    }

    protected override void OnJoystickPressed(PointerEventData eventData)
    {
        if (background == null)
            return;

        background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
        background.gameObject.SetActive(true);
    }

    protected override void OnJoystickReset(PointerEventData eventData)
    {
        if (background == null)
            return;

        background.anchoredPosition = startPosition;
        background.gameObject.SetActive(true);
    }
}