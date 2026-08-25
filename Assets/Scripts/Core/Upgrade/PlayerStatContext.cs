using System.Collections;
using UnityEngine;

public class PlayerStatContext : MonoBehaviour
{
    public static PlayerStatContext Instance { get; private set; }

    private const string RebirthLuckKey = "Rebirth_Luck";

    public StatContext UpgradeStats { get; } = new();
    public StatContext BuffStats { get; } = new();
    public StatContext RebirthStats { get; } = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        LoadRebirthStats();
    }

    public float GetFinalStat(UpgradeStatType statType, float baseValue = 0f)
    {
        float value = baseValue;

        value = UpgradeStats.GetFinalValue(statType, value);
        value = BuffStats.GetFinalValue(statType, value);
        value = RebirthStats.GetFinalValue(statType, value);

        return value;
    }

    public void AddTemporaryBuff(
        UpgradeStatType statType,
        StatModifierType modifierType,
        float value,
        float duration)
    {
        if (statType == UpgradeStatType.None)
            return;

        if (value <= 0f || duration <= 0f)
            return;

        StartCoroutine(TemporaryBuffRoutine(
            statType,
            modifierType,
            value,
            duration
        ));
    }

    private IEnumerator TemporaryBuffRoutine(
        UpgradeStatType statType,
        StatModifierType modifierType,
        float value,
        float duration)
    {
        BuffStats.AddStat(statType, modifierType, value);

        yield return new WaitForSeconds(duration);

        BuffStats.RemoveStat(statType, modifierType, value);
    }

    public void MultiplyRebirthLuck(float multiplier)
    {
        if (multiplier <= 0f)
            return;

        float currentLuck = PlayerPrefs.GetFloat(RebirthLuckKey, 1f);
        currentLuck *= multiplier;

        PlayerPrefs.SetFloat(RebirthLuckKey, currentLuck);
        PlayerPrefs.Save();

        RebuildRebirthLuck(currentLuck);
    }

    private void LoadRebirthStats()
    {
        float rebirthLuck = PlayerPrefs.GetFloat(RebirthLuckKey, 1f);
        RebuildRebirthLuck(rebirthLuck);
    }

    private void RebuildRebirthLuck(float rebirthLuck)
    {
        RebirthStats.Clear();

        if (rebirthLuck > 1f)
        {
            RebirthStats.AddStat(
                UpgradeStatType.Luck,
                StatModifierType.Flat,
                rebirthLuck - 1f
            );
        }
    }

    public void ClearTemporaryBuffs()
    {
        BuffStats.Clear();
    }

    public void ClearUpgradeStats()
    {
        UpgradeStats.Clear();
    }
}