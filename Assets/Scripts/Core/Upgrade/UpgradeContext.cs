using System.Collections.Generic;

public class UpgradeContext
{
    private readonly HashSet<string> unlockedItems = new();

    public StatContext Stats { get; } = new();

    public IReadOnlyCollection<string> UnlockedItems => unlockedItems;

    public bool IsItemUnlocked(string itemId)
    {
        return !string.IsNullOrWhiteSpace(itemId) && unlockedItems.Contains(itemId);
    }

    public void UnlockItem(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return;

        unlockedItems.Add(itemId);
    }

    public void Clear()
    {
        unlockedItems.Clear();
        Stats.Clear();
    }
}