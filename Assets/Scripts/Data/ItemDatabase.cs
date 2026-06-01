using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Database/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<ItemData> items = new();

    public IReadOnlyList<ItemData> Items => items;

    public ItemData GetItemById(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        foreach (ItemData item in items)
        {
            if (item == null)
                continue;

            if (item.Id == id)
                return item;
        }

        return null;
    }

    public ItemData GetRandomUnlockedFood(UpgradeContext upgradeContext)
    {
        List<ItemData> validItems = new();

        foreach (ItemData item in items)
        {
            if (item == null)
                continue;

            if (item.ItemType != ItemType.Food)
                continue;

            if (item.DropWeight <= 0)
                continue;

            if (!item.RequireUnlock)
            {
                validItems.Add(item);
                continue;
            }

            if (upgradeContext != null &&
                upgradeContext.IsItemUnlocked(item.Id))
            {
                validItems.Add(item);
            }
        }

        return GetRandomByWeight(validItems);
    }

    private ItemData GetRandomByWeight(List<ItemData> validItems)
    {
        if (validItems == null || validItems.Count == 0)
            return null;

        int totalWeight = 0;

        foreach (ItemData item in validItems)
        {
            if (item == null)
                continue;

            totalWeight += item.DropWeight;
        }

        if (totalWeight <= 0)
            return null;

        int roll = Random.Range(0, totalWeight);
        int current = 0;

        foreach (ItemData item in validItems)
        {
            if (item == null)
                continue;

            current += item.DropWeight;

            if (roll < current)
                return item;
        }

        return validItems[validItems.Count - 1];
    }
}