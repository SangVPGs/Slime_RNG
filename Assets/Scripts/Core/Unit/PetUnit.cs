using UnityEngine;

public class PetUnit : Unit
{
    public PetUnitData PetData => data as PetUnitData;

    private const float HpGrowthMultiplier = 1.12f;
    private const float AtkGrowthMultiplier = 1.08f;
    private const float HealGrowthMultiplier = 1.05f;

    [Header("Pet Heal")]
    [SerializeField] private int heal;
    [SerializeField] private float healInterval = 1f;

    public int Heal => heal;

    public int CombatPower => CalculateCombatPower(
        Atk,
        MaxHp,
        Heal
    );

    private float nextHealTime;

    public static int CalculateCombatPower(PetUnitData petData, int level)
    {
        if (petData == null)
            return 0;

        int safeLevel = Mathf.Max(1, level);
        int levelOffset = safeLevel - 1;

        int hp = Mathf.RoundToInt(petData.baseHp * Mathf.Pow(HpGrowthMultiplier, levelOffset));
        int atk = Mathf.RoundToInt(petData.baseAtk * Mathf.Pow(AtkGrowthMultiplier, levelOffset));
        int heal = Mathf.RoundToInt(petData.baseHeal * Mathf.Pow(HealGrowthMultiplier, levelOffset));

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

        nextHealTime = Time.time + healInterval;
    }

    public override void Init(UnitData unitData, string instanceId)
    {
        if (unitData is not PetUnitData)
            return;

        base.Init(unitData, instanceId);

        nextHealTime = Time.time + healInterval;
    }

    protected override void RecalculateStats()
    {
        base.RecalculateStats();

        if (PetData == null)
            return;

        int levelOffset = Mathf.Max(0, currentLevel - 1);

        maxHp = Mathf.RoundToInt(PetData.baseHp * Mathf.Pow(HpGrowthMultiplier, levelOffset));
        atk = Mathf.RoundToInt(PetData.baseAtk * Mathf.Pow(AtkGrowthMultiplier, levelOffset));
        heal = Mathf.RoundToInt(PetData.baseHeal * Mathf.Pow(HealGrowthMultiplier, levelOffset));
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

        nextHealTime = Time.time + healInterval;

        return true;
    }

    private void HealTick()
    {
        if (!CanAct())
            return;

        if (heal <= 0)
            return;

        if (currentHp >= maxHp)
            return;

        if (Time.time < nextHealTime)
            return;

        nextHealTime = Time.time + healInterval;

        currentHp += heal;

        if (currentHp > maxHp)
            currentHp = maxHp;
    }
}