using UnityEngine;

public class ItemInventoryUI : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private InventorySystem inventorySystem;

    [Header("Inventory UI")]
    [SerializeField] private InventoryUI inventoryUI;

    [Header("UI")]
    [SerializeField] private Transform contentRoot;
    [SerializeField] private ItemUI itemUIPrefab;

    private void OnEnable()
    {
        if (inventorySystem != null)
            inventorySystem.OnInventoryChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (inventorySystem != null)
            inventorySystem.OnInventoryChanged -= Refresh;
    }

    private void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (inventorySystem == null || contentRoot == null || itemUIPrefab == null)
            return;

        Clear();

        foreach (InventorySystem.ItemInventoryEntry entry in inventorySystem.Data.Items)
        {
            if (entry == null || entry.itemData == null)
                continue;

            ItemUI ui = Instantiate(itemUIPrefab, contentRoot);
            ui.Setup(inventorySystem, inventoryUI, entry);
        }
    }

    private void Clear()
    {
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
            Destroy(contentRoot.GetChild(i).gameObject);
    }
}