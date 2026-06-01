using UnityEngine;
using UnityEngine.UI;

public class UpgradeArrowView : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Image image;

    [Header("Shape")]
    [SerializeField, Min(1f)] private float thickness = 12f;
    [SerializeField, Min(0f)] private float nodePadding = 50f;

    [Header("State Colors")]
    [SerializeField] private Color lockedColor = Color.gray;
    [SerializeField] private Color availableColor = Color.yellow;
    [SerializeField] private Color unlockedColor = Color.green;

    private void Awake()
    {
        if (rectTransform == null)
            rectTransform = transform as RectTransform;

        if (image == null)
            image = GetComponent<Image>();
    }

    public void Setup(Vector2 from, Vector2 to)
    {
        if (rectTransform == null)
            rectTransform = transform as RectTransform;

        Vector2 direction = to - from;
        float distance = direction.magnitude;

        if (distance <= 0.01f)
            return;

        Vector2 normal = direction.normalized;

        Vector2 paddedFrom = from + normal * nodePadding;
        Vector2 paddedTo = to - normal * nodePadding;

        Vector2 finalDirection = paddedTo - paddedFrom;
        float finalDistance = Mathf.Max(1f, finalDirection.magnitude);

        rectTransform.anchoredPosition = (paddedFrom + paddedTo) * 0.5f;
        rectTransform.sizeDelta = new Vector2(finalDistance, thickness);

        float angle = Mathf.Atan2(finalDirection.y, finalDirection.x) * Mathf.Rad2Deg;
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    public void RefreshState(bool parentUnlocked, bool childUnlocked, bool childCanUnlock)
    {
        if (image == null)
            return;

        if (childUnlocked)
            image.color = unlockedColor;
        else if (parentUnlocked && childCanUnlock)
            image.color = availableColor;
        else
            image.color = lockedColor;
    }
}