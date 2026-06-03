using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<ItemData> items = new();

    public IReadOnlyList<ItemData> Items => items;

    public ItemData GetItemById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        foreach (ItemData item in items)
        {
            if (item != null && item.Id == id)
                return item;
        }

        return null;
    }

    public ItemData GetRandomDropItemByWeight(UnlockItemContext unlockContext)
    {
        int totalWeight = 0;

        foreach (ItemData item in items)
        {
            if (!CanDrop(item, unlockContext))
                continue;

            totalWeight += item.DropWeight;
        }

        if (totalWeight <= 0)
            return null;

        int value = Random.Range(0, totalWeight);

        foreach (ItemData item in items)
        {
            if (!CanDrop(item, unlockContext))
                continue;

            int weight = item.DropWeight;

            if (value < weight)
                return item;

            value -= weight;
        }

        return null;
    }

    private bool CanDrop(ItemData item, UnlockItemContext unlockContext)
    {
        if (item == null)
            return false;

        if (item.DropWeight <= 0)
            return false;

        if (!item.RequireUnlock)
            return true;

        return unlockContext != null && unlockContext.IsItemUnlocked(item.Id);
    }
}