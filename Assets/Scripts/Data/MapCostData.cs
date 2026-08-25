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
        public double cost;
        public double goldDrop;

        [Range(0f, 1f)]
        public float poonChance = 0.3f;
        public long poonDrop;
    }

    [SerializeField] private double defaultCost = 100;
    [SerializeField] private float overflowMultiplier = 2.5f;
    [SerializeField] private List<CostEntry> costs = new();

    public double GetCost(int level)
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
            return (double)Math.Max(0, exactEntry.cost);

        if (highestEntry == null)
            return defaultCost;

        if (level > highestEntry.level)
            return (double)Math.Round(highestEntry.cost * overflowMultiplier);

        return defaultCost;
    }
}