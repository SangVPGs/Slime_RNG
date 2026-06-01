using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ItemPickup : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private ItemData itemData;
    [SerializeField] private int amount = 1;

    [Header("Visual")]
    [SerializeField] private Transform modelRoot;

    private GameObject currentModel;

    public ItemData ItemData => itemData;
    public int Amount => amount;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        RefreshVisual();
    }

    public void SetData(ItemData data, int itemAmount = 1)
    {
        itemData = data;
        amount = Mathf.Max(1, itemAmount);

        RefreshVisual();
    }

    public bool Pickup(InventorySystem inventory)
    {
        if (inventory == null || itemData == null)
            return false;

        bool success = inventory.AddItem(itemData, amount);

        if (!success)
            return false;

        Destroy(gameObject);
        return true;
    }

    private void RefreshVisual()
    {
        ClearCurrentModel();

        if (itemData == null || itemData.Prefab == null)
            return;

        Transform parent = modelRoot != null ? modelRoot : transform;

        currentModel = Instantiate(itemData.Prefab, parent);
        currentModel.transform.localPosition = Vector3.zero;
        currentModel.transform.localRotation = Quaternion.identity;
        currentModel.transform.localScale = Vector3.one;
    }

    private void ClearCurrentModel()
    {
        if (currentModel == null)
            return;

        Destroy(currentModel);
        currentModel = null;
    }
}