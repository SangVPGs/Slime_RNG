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

    [Header("State Visual")]
    [SerializeField] private GameObject unlockedMark;
    [SerializeField] private GameObject canUnlockMark;

    [Header("Visibility")]
    [SerializeField] private CanvasGroup canvasGroup;

    private UpgradeTreeSystem treeSystem;
    private UpgradeTreeView treeView;
    private UpgradeNodeData node;

    private bool visible;

    public RectTransform RectTransform => transform as RectTransform;
    public UpgradeNodeData Node => node;

    public void Setup(
        UpgradeTreeSystem treeSystem,
        UpgradeTreeView treeView,
        UpgradeNodeData node)
    {
        this.treeSystem = treeSystem;
        this.treeView = treeView;
        this.node = node;

        ResolveCanvasGroup();

        Refresh();
    }

    public void SetVisible(bool value)
    {
        visible = value;

        ResolveCanvasGroup();

        if (canvasGroup == null)
            return;

        canvasGroup.alpha = value ? 1f : 0f;
        canvasGroup.interactable = value;
        canvasGroup.blocksRaycasts = value;
    }

    public void Refresh()
    {
        if (treeSystem == null || node == null)
            return;

        RefreshInfo();
        RefreshState();
    }

    private void RefreshInfo()
    {
        if (nameText != null)
        {
            nameText.text = string.IsNullOrWhiteSpace(node.DisplayName)
                ? node.name
                : node.DisplayName;
        }

        if (iconImage != null)
        {
            iconImage.sprite = node.Icon;
            iconImage.gameObject.SetActive(node.Icon != null);
        }

        if (costText != null)
        {
            costText.text = node.Cost <= 0
                ? "Free"
                : NumberFormatter.Format(node.Cost);
        }
    }

    private void RefreshState()
    {
        bool unlocked = treeSystem.IsUnlocked(node);
        bool canUnlock = treeSystem.CanUnlock(node);

        if (lockOverlay != null)
            lockOverlay.SetActive(!unlocked);

        if (unlockedMark != null)
            unlockedMark.SetActive(unlocked);

        if (canUnlockMark != null)
            canUnlockMark.SetActive(!unlocked && canUnlock);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (treeView == null || node == null)
            return;

        treeView.SelectNode(node);
    }

    private void ResolveCanvasGroup()
    {
        if (canvasGroup != null)
            return;

        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }
}