using UnityEngine;

public class SlimeUnit : Unit
{
    public SlimeUnitData SlimeData => data as SlimeUnitData;

    public override void Init(UnitData unitData)
    {
        if (unitData is not SlimeUnitData)
        {
            return;
        }

        base.Init(unitData);
    }

    public void Spawn(Vector3 position)
    {
        transform.position = position;
        gameObject.SetActive(true);

        currentHp = data.hp;
        isDead = false;
    }
}