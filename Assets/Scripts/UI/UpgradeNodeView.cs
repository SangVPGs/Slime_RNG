using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeNodeView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image lockOverlay;
    [SerializeField] private Image unlockedOverlay;
    [SerializeField] private Image availableOverlay;

    private UpgradeTreeSystem treeSystem;
    private UpgradeNodeData node;

    public RectTransform RectTransform => transform as RectTransform;
    public UpgradeNodeData Node => node;

    public void Setup(UpgradeTreeSystem treeSystem, UpgradeNodeData node)
    {
        this.treeSystem = treeSystem;
        this.node = node;

        SetupButton();
        Refresh();
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnClick);
    }

    private void SetupButton()
    {
        if (button == null)
        {
            Debug.LogWarning($"UpgradeNodeView '{name}' is missing button.");
            return;
        }

        button.onClick.RemoveListener(OnClick);
        button.onClick.AddListener(OnClick);
    }

    public void Refresh()
    {
        if (node == null || treeSystem == null)
            return;

        if (nameText != null)
            nameText.text = string.IsNullOrWhiteSpace(node.DisplayName) ? node.name : node.DisplayName;

        if (costText != null)
            costText.text = node.Cost.ToString();

        if (iconImage != null)
        {
            iconImage.sprite = node.Icon;
            iconImage.gameObject.SetActive(node.Icon != null);
        }

        bool unlocked = treeSystem.IsUnlocked(node);
        bool canUnlock = treeSystem.CanUnlock(node);

        if (button != null)
            button.interactable = canUnlock;

        if (lockOverlay != null)
            lockOverlay.gameObject.SetActive(!unlocked && !canUnlock);

        if (unlockedOverlay != null)
            unlockedOverlay.gameObject.SetActive(unlocked);

        if (availableOverlay != null)
            availableOverlay.gameObject.SetActive(!unlocked && canUnlock);
    }

    private void OnClick()
    {
        if (treeSystem == null || node == null)
            return;

        treeSystem.Unlock(node);
    }
}