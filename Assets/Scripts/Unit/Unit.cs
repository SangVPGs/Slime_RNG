using UnityEngine;

public class Unit : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] protected UnitData data;

    [Header("Runtime")]
    [SerializeField] protected int currentHp;

    protected bool isDead;
    protected float lastAttackTime;

    public UnitData Data => data;

    public string Id => data != null ? data.Id : "";
    public string UnitName => data != null ? data.unitName : "";

    public int CurrentHp => currentHp;
    public bool IsDead => isDead;

    protected virtual void Awake()
    {
        if (data != null)
        {
            Init(data);
        }
    }

    public virtual void Init(UnitData unitData)
    {
        data = unitData;

        currentHp = data.hp;
        isDead = false;

        gameObject.name = data.unitName;
    }
}