using UnityEngine;

public class UpgradeLineView : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;

    public UpgradeNodeData FromNode { get; private set; }
    public UpgradeNodeData ToNode { get; private set; }

    private void Reset()
    {
        rectTransform = transform as RectTransform;
    }

    public void Setup(UpgradeNodeData fromNode, UpgradeNodeData toNode)
    {
        FromNode = fromNode;
        ToNode = toNode;
    }

    public void Draw(Vector2 from, Vector2 to, float thickness)
    {
        if (rectTransform == null)
            rectTransform = transform as RectTransform;

        Vector2 direction = to - from;
        float distance = direction.magnitude;

        rectTransform.anchoredPosition = from + direction * 0.5f;
        rectTransform.sizeDelta = new Vector2(distance, thickness);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}