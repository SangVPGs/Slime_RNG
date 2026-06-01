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

    public ItemData GetRandomItem()
    {
        List<ItemData> validItems = items.FindAll(item => item != null);

        if (validItems.Count == 0)
            return null;

        int index = Random.Range(0, validItems.Count);
        return validItems[index];
    }
}