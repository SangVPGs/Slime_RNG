using UnityEngine;

public class PetUnit : Unit
{
    public PetUnitData PetData => data as PetUnitData;

    private float healInterval = 1f;
    private float nextHealTime;

    public override void Init(UnitData unitData)
    {
        if (unitData is not PetUnitData)
        {
            return;
        }

        base.Init(unitData);
        nextHealTime = Time.time + healInterval;
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
            state = UnitState.Idle;
            return;
        }

        Move(direction);
    }

    public override bool Attack(Unit target)
    {
        bool attacked = base.Attack(target);

        if (attacked)
        {
            // Pet attack animation
        }

        return attacked;
    }

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);

        // Pet hit animation
    }

    private void HealTick()
    {
        if (!CanAct())
            return;

        if (PetData.heal <= 0)
            return;

        if (currentHp >= data.hp)
            return;

        if (Time.time < nextHealTime)
            return;

        nextHealTime = Time.time + healInterval;

        currentHp += PetData.heal;

        if (currentHp > data.hp)
            currentHp = data.hp;
    }

    public override void Die()
    {
        base.Die();
        Debug.Log($"{data.unitName} died");
    }
}