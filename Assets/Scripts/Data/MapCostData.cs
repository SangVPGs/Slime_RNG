using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Database/Map Cost Data")]
public class MapCostData : ScriptableObject
{
    [Serializable]
    private class CostEntry
    {
        public int level;
        public long cost;
        public long goldDrop;
    }

    [SerializeField] private long defaultCost = 100;
    [SerializeField] private float overflowMultiplier = 2.5f;
    [SerializeField] private List<CostEntry> costs = new();

    public long GetCost(int level)
    {
        if (level <= 0)
            return 0;

        CostEntry exactEntry = null;
        CostEntry highestEntry = null;

        foreach (CostEntry entry in costs)
        {
            if (entry == null)
                continue;

            if (entry.level == level)
                exactEntry = entry;

            if (highestEntry == null || entry.level > highestEntry.level)
                highestEntry = entry;
        }

        if (exactEntry != null)
            return Math.Max(0, exactEntry.cost);

        if (highestEntry == null)
            return defaultCost;

        if (level > highestEntry.level)
            return (long)Math.Round(highestEntry.cost * overflowMultiplier);

        return defaultCost;
    }
}