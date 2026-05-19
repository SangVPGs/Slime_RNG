using UnityEngine;

public class PetUnit : Unit
{
    public PetUnitData PetData => data as PetUnitData;

    public PetRarity Rarity => PetData != null ? PetData.rarity : PetRarity.Common;
    public Sprite Icon => PetData != null ? PetData.icon : null;

    public override void Init(UnitData unitData)
    {
        if (unitData is not PetUnitData)
        {
            return;
        }

        base.Init(unitData);
    }

    public void MoveTo(Vector3 targetPosition, float stopDistance = 0.2f)
    {
        if (IsDead || data == null)
            return;

        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        if (direction.magnitude <= stopDistance)
            return;

        direction.Normalize();

        transform.position += direction * data.speed * Time.deltaTime;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}