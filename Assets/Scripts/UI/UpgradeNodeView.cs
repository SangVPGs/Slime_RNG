using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeNodeView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image iconImage;

    [Header("Lock")]
    [SerializeField] private GameObject lockOverlay;
    [SerializeField] private TMP_Text costText;

    [Header("Button")]
    [SerializeField] private Button nodeButton;

    private UpgradeTreeSystem treeSystem;
    private UpgradeNodeData node;

    public RectTransform RectTransform => transform as RectTransform;
    public UpgradeNodeData Node => node;

    public void Setup(UpgradeTreeSystem treeSystem, UpgradeNodeData node)
    {
        if (this.treeSystem != null)
            this.treeSystem.OnTreeChanged -= Refresh;

        this.treeSystem = treeSystem;
        this.node = node;

        if (this.treeSystem != null)
            this.treeSystem.OnTreeChanged += Refresh;

        SetupButton();
        Refresh();
    }

    private void OnDestroy()
    {
        if (nodeButton != null)
            nodeButton.onClick.RemoveListener(OnNodeClicked);

        if (treeSystem != null)
            treeSystem.OnTreeChanged -= Refresh;
    }

    private void SetupButton()
    {
        if (nodeButton == null)
            nodeButton = GetComponent<Button>();

        if (nodeButton == null)
        {
            Debug.LogWarning($"UpgradeNodeView '{name}' is missing Button.");
            return;
        }

        nodeButton.onClick.RemoveListener(OnNodeClicked);
        nodeButton.onClick.AddListener(OnNodeClicked);
    }

    public void Refresh()
    {
        if (node == null || treeSystem == null)
            return;

        RefreshContent();
        RefreshState();
    }

    private void RefreshContent()
    {
        if (nameText != null)
            nameText.text = string.IsNullOrWhiteSpace(node.DisplayName) ? node.name : node.DisplayName;

        if (iconImage != null)
        {
            iconImage.sprite = node.Icon;
            iconImage.gameObject.SetActive(node.Icon != null);
        }

        if (costText != null)
            costText.text = node.Cost <= 0 ? "Free" : node.Cost.ToString();
    }

    private void RefreshState()
    {
        bool unlocked = treeSystem.IsUnlocked(node);
        bool canUnlock = treeSystem.CanUnlock(node);

        if (lockOverlay != null)
            lockOverlay.SetActive(!unlocked);

        if (nodeButton != null)
            nodeButton.interactable = !unlocked && canUnlock;
    }

    private void OnNodeClicked()
    {
        if (treeSystem == null || node == null)
            return;

        if (!treeSystem.CanUnlock(node))
            return;

        Debug.Log("Clicked");

        treeSystem.Unlock(node);
    }
}