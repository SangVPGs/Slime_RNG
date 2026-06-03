using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeNodeView : MonoBehaviour, IPointerClickHandler
{
    [Header("UI")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image iconImage;

    [Header("Lock")]
    [SerializeField] private GameObject lockOverlay;
    [SerializeField] private TMP_Text costText;

    [Header("Visibility")]
    [SerializeField] private CanvasGroup canvasGroup;

    private UpgradeTreeSystem treeSystem;
    private UpgradeNodeData node;

    private bool visible;

    public RectTransform RectTransform => transform as RectTransform;
    public UpgradeNodeData Node => node;

    public void Setup(UpgradeTreeSystem treeSystem, UpgradeNodeData node)
    {
        this.treeSystem = treeSystem;
        this.node = node;

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        Refresh();
    }

    public void SetVisible(bool value)
    {
        visible = value;

        canvasGroup.alpha = value ? 1f : 0f;
        canvasGroup.interactable = value;
        canvasGroup.blocksRaycasts = value;
    }

    public void Refresh()
    {
        if (treeSystem == null || node == null)
            return;

        if (nameText != null)
            nameText.text = string.IsNullOrWhiteSpace(node.DisplayName) ? node.name : node.DisplayName;

        if (iconImage != null)
        {
            iconImage.sprite = node.Icon;
            iconImage.gameObject.SetActive(node.Icon != null);
        }

        if (costText != null)
            costText.text = node.Cost <= 0 ? "Free" : node.Cost.ToString();

        bool unlocked = treeSystem.IsUnlocked(node);

        if (lockOverlay != null)
            lockOverlay.SetActive(!unlocked);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!visible)
            return;

        if (treeSystem == null || node == null)
            return;

        if (!treeSystem.CanUnlock(node))
            return;

        treeSystem.Unlock(node);
    }
}