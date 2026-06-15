using System;
using UnityEngine;

public class PetUnit : Unit
{
    public PetUnitData PetData => data as PetUnitData;

    [Header("Pet Heal")]
    [SerializeField] private int heal;
    [SerializeField] private float healInterval = 1f;

    public int Heal => heal;

    private float nextHealTime;

    protected override bool UseSavedLevel => false;

    public static long CalculateCombatPower(PetUnitData petData, int level)
    {
        if (petData == null)
            return 0;

        int safeLevel = Mathf.Max(1, level);
        int levelOffset = safeLevel - 1;

        double hp = Mathf.RoundToInt(
            petData.baseHp *
            Mathf.Pow(petData.hpGrowthMultiplier, levelOffset)
        );

        double atk = Mathf.RoundToInt(
            petData.baseAtk *
            Mathf.Pow(petData.atkGrowthMultiplier, levelOffset)
        );

        double heal = Mathf.RoundToInt(
            petData.baseHeal *
            Mathf.Pow(petData.healGrowthMultiplier, levelOffset)
        );

        return CalculateCombatPower(atk, hp, heal);
    }

    public static long CalculateCombatPower(double atk, double hp, double heal)
    {
        double combatPower = atk * 1.5d + hp * 0.5d + heal * 1d;

        if (combatPower >= long.MaxValue)
            return long.MaxValue;

        return (long)Math.Round(combatPower);
    }

    public override void Init(UnitData unitData)
    {
        if (unitData is not PetUnitData)
            return;

        base.Init(unitData);

        InitDefaultProgress();
        RecalculateStats();
        currentHp = maxHp;

        nextHealTime = Time.time + healInterval;
    }

    public override void Init(UnitData unitData, string instanceId)
    {
        if (unitData is not PetUnitData)
            return;

        base.Init(unitData, instanceId);

        InitDefaultProgress();
        RecalculateStats();
        currentHp = maxHp;

        nextHealTime = Time.time + healInterval;
    }

    public void InitFromInventoryEntry(
        InventorySystem.PetInventoryEntry entry)
    {
        if (entry == null || entry.petData == null)
            return;

        Init(entry.petData);
        ApplyInventoryProgress(entry);
    }

    public void ApplyInventoryProgress(InventorySystem.PetInventoryEntry entry)
    {
        if (entry == null || entry.petData == null)
            return;

        data = entry.petData;

        double hpPercent = maxHp > 0
            ? Mathf.Clamp01((float)((double)currentHp / maxHp))
            : 1d;

        currentLevel = Mathf.Clamp(
            entry.level,
            1,
            entry.petData.maxLevel
        );

        RecalculateStats();

        currentHp = Math.Round(maxHp * hpPercent);

        if (currentHp <= 0 && !IsDead)
            currentHp = maxHp;

        nextHealTime = Time.time + healInterval;
    }

    protected override void RecalculateStats()
    {
        base.RecalculateStats();

        if (PetData == null)
            return;

        int levelOffset = Mathf.Max(0, currentLevel - 1);

        maxHp = Mathf.RoundToInt(
            PetData.baseHp *
            Mathf.Pow(PetData.hpGrowthMultiplier, levelOffset)
        );

        atk = Mathf.RoundToInt(
            PetData.baseAtk *
            Mathf.Pow(PetData.atkGrowthMultiplier, levelOffset)
        );

        heal = Mathf.RoundToInt(
            PetData.baseHeal *
            Mathf.Pow(PetData.healGrowthMultiplier, levelOffset)
        );
    }

    private void Update()
    {
        HealTick();
    }

    public void MoveTo(Vector3 targetPosition, float stopDistance)
    {
        if (!CanAct())
            return;

        if (state == UnitState.Attacking)
            return;

        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= stopDistance * stopDistance)
        {
            SetState(UnitState.Idle);
            return;
        }

        Move(direction);
    }

    public override bool LevelUp()
    {
        bool success = base.LevelUp();

        if (!success)
            return false;

        RecalculateStats();

        currentHp = maxHp;
        nextHealTime = Time.time + healInterval;

        return true;
    }

    private void InitDefaultProgress()
    {
        if (PetData == null)
            return;

        currentLevel = Mathf.Clamp(
            PetData.defaultLevel,
            1,
            PetData.maxLevel
        );
    }

    private void HealTick()
    {
        if (!CanAct())
            return;

        if (Heal <= 0)
            return;

        if (currentHp >= MaxHp)
            return;

        if (Time.time < nextHealTime)
            return;

        nextHealTime = Time.time + healInterval;

        currentHp += Heal;

        if (currentHp > MaxHp)
            currentHp = MaxHp;
    }
}