using System.Collections.Generic;

public enum StatModifierType
{
    Flat,
    Percent
}

public class StatContext
{
    private readonly Dictionary<UpgradeStatType, RuntimeStat> stats = new();

    public void AddStat(
        UpgradeStatType statType,
        StatModifierType modifierType,
        float value)
    {
        if (statType == UpgradeStatType.None)
            return;

        RuntimeStat stat = GetOrCreateStat(statType);

        switch (modifierType)
        {
            case StatModifierType.Flat:
                stat.AddFlat(value);
                break;

            case StatModifierType.Percent:
                stat.AddPercent(value / 100f);
                break;
        }
    }

    public RuntimeStat GetStat(UpgradeStatType statType)
    {
        if (statType == UpgradeStatType.None)
            return null;

        stats.TryGetValue(statType, out RuntimeStat stat);
        return stat;
    }

    public float GetFinalValue(UpgradeStatType statType, float baseValue)
    {
        RuntimeStat stat = GetStat(statType);

        if (stat == null)
            return baseValue;

        return stat.Calculate(baseValue);
    }

    public float GetFlat(UpgradeStatType statType)
    {
        RuntimeStat stat = GetStat(statType);
        return stat != null ? stat.Flat : 0f;
    }

    public float GetPercent(UpgradeStatType statType)
    {
        RuntimeStat stat = GetStat(statType);
        return stat != null ? stat.Percent : 0f;
    }

    public void Clear()
    {
        stats.Clear();
    }

    private RuntimeStat GetOrCreateStat(UpgradeStatType statType)
    {
        if (!stats.TryGetValue(statType, out RuntimeStat stat))
        {
            stat = new RuntimeStat();
            stats.Add(statType, stat);
        }

        return stat;
    }
}

[System.Serializable]
public class RuntimeStat
{
    public float Flat { get; private set; }
    public float Percent { get; private set; }

    public void AddFlat(float value)
    {
        Flat += value;
    }

    public void AddPercent(float value)
    {
        Percent += value;
    }

    public float Calculate(float baseValue)
    {
        return (baseValue + Flat) * (1f + Percent);
    }

    public void Clear()
    {
        Flat = 0f;
        Percent = 0f;
    }
}