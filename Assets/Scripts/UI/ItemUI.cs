using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private Button button;

    private InventorySystem inventorySystem;
    private InventoryUI inventoryUI;
    private InventorySystem.ItemInventoryEntry entry;

    public void Setup(
        InventorySystem inventory,
        InventoryUI ui,
        InventorySystem.ItemInventoryEntry itemEntry)
    {
        inventorySystem = inventory;
        inventoryUI = ui;
        entry = itemEntry;

        Refresh();

        if (button != null)
        {
            button.onClick.RemoveListener(OnClicked);
            button.onClick.AddListener(OnClicked);
        }
    }

    private void Refresh()
    {
        if (entry == null || entry.itemData == null)
            return;

        ItemData itemData = entry.itemData;

        if (icon != null)
        {
            icon.sprite = itemData.Icon;
            icon.enabled = itemData.Icon != null;
        }

        if (nameText != null)
            nameText.text = itemData.ItemName;

        if (amountText != null)
            amountText.text = $"x{entry.amount}";
    }

    private void OnClicked()
    {
        if (entry == null || entry.itemData == null)
            return;

        if (inventoryUI == null)
            return;

        inventoryUI.SelectItem(entry);
    }
}