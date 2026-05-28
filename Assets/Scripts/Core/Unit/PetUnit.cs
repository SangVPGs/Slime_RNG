using UnityEngine;

public class PetUnit : Unit
{
    public PetUnitData PetData => data as PetUnitData;

    [Header("Pet Exp")]
    [SerializeField] private int currentExp;
    [SerializeField] private int maxExp = 100;
    [SerializeField] private float maxExpGrowthMultiplier = 1.25f;

    public int CurrentExp => currentExp;
    public int MaxExp => maxExp;
    public float ExpPercent => maxExp > 0 ? (float)currentExp / maxExp : 0f;

    [Header("Pet Heal")]
    [SerializeField] private int heal;
    [SerializeField] private float healInterval = 1f;

    public int Heal => heal;

    private float nextHealTime;

    public static int CalculateCombatPower(PetUnitData petData, int level)
    {
        if (petData == null)
            return 0;

        int safeLevel = Mathf.Max(1, level);
        int levelOffset = safeLevel - 1;

        int hp = Mathf.RoundToInt(
            petData.baseHp *
            Mathf.Pow(petData.hpGrowthMultiplier, levelOffset)
        );

        int atk = Mathf.RoundToInt(
            petData.baseAtk *
            Mathf.Pow(petData.atkGrowthMultiplier, levelOffset)
        );

        int heal = Mathf.RoundToInt(
            petData.baseHeal *
            Mathf.Pow(petData.healGrowthMultiplier, levelOffset)
        );

        return CalculateCombatPower(atk, hp, heal);
    }

    public static int CalculateCombatPower(int atk, int hp, int heal)
    {
        return Mathf.RoundToInt(
            atk * 1.5f +
            hp * 0.5f +
            heal * 1f
        );
    }

    public override void Init(UnitData unitData)
    {
        if (unitData is not PetUnitData)
            return;

        base.Init(unitData);

        LoadPetProgress();
        RecalculateStats();
        currentHp = maxHp;

        nextHealTime = Time.time + healInterval;
    }

    public override void Init(UnitData unitData, string instanceId)
    {
        if (unitData is not PetUnitData)
            return;

        base.Init(unitData, instanceId);

        LoadPetProgress();
        RecalculateStats();
        currentHp = maxHp;

        nextHealTime = Time.time + healInterval;
    }

    protected override void RecalculateStats()
    {
        base.RecalculateStats();

        if (PetData == null)
            return;

        int levelOffset = Mathf.Max(0, currentLevel - 1);

        maxHp = Mathf.RoundToInt(PetData.baseHp * Mathf.Pow(PetData.hpGrowthMultiplier, levelOffset));
        atk = Mathf.RoundToInt(PetData.baseAtk * Mathf.Pow(PetData.atkGrowthMultiplier, levelOffset));
        heal = Mathf.RoundToInt(PetData.baseHeal * Mathf.Pow(PetData.healGrowthMultiplier, levelOffset));
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

    public void GainExp(int amount)
    {
        if (amount <= 0)
            return;

        if (data == null)
            return;

        if (currentLevel >= data.maxLevel)
            return;

        currentExp += amount;

        if (currentExp >= maxExp)
        {
            LevelUp();
        }

        SavePetProgress();
    }

    public override bool LevelUp()
    {
        bool success = base.LevelUp();

        if (!success)
            return false;

        currentExp = 0;
        maxExp = Mathf.RoundToInt(maxExp * maxExpGrowthMultiplier);

        if (maxExp < 1)
            maxExp = 1;

        nextHealTime = Time.time + healInterval;

        SavePetProgress();

        return true;
    }

    private void LoadPetProgress()
    {
        if (PetData == null)
            return;

        string id = PetData.Id;

        currentLevel = PlayerPrefs.GetInt($"{id}_Level", PetData.defaultLevel);
        currentExp = PlayerPrefs.GetInt($"{id}_Exp", 0);
        maxExp = PlayerPrefs.GetInt($"{id}_MaxExp", 100);

        currentLevel = Mathf.Clamp(currentLevel, 1, PetData.maxLevel);
        currentExp = Mathf.Max(0, currentExp);
        maxExp = Mathf.Max(1, maxExp);
    }

    private void SavePetProgress()
    {
        if (PetData == null)
            return;

        string id = PetData.Id;

        PlayerPrefs.SetInt($"{id}_Level", currentLevel);
        PlayerPrefs.SetInt($"{id}_Exp", currentExp);
        PlayerPrefs.SetInt($"{id}_MaxExp", maxExp);
        PlayerPrefs.Save();
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