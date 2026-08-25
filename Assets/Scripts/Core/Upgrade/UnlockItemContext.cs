using System.Collections.Generic;
using UnityEngine;

public class UnlockItemContext : MonoBehaviour
{
    public static UnlockItemContext Instance { get; private set; }

    private readonly HashSet<string> unlockedItemIds = new();

    public IReadOnlyCollection<string> UnlockedItemIds => unlockedItemIds;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public bool IsItemUnlocked(string itemId)
    {
        return !string.IsNullOrWhiteSpace(itemId) && unlockedItemIds.Contains(itemId);
    }

    public void UnlockItem(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return;

        unlockedItemIds.Add(itemId);
    }

    public void Clear()
    {
        unlockedItemIds.Clear();
    }
}