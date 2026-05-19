using UnityEngine;

public enum UnitState
{
    Idle,
    Moving,
    Attacking,
    Dead
}

public abstract class Unit : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] protected UnitData data;

    public int MaxHp => data != null ? data.hp : 0;
    public float HpPercent => MaxHp > 0 ? (float)currentHp / MaxHp : 0f;

    [Header("Health Bar")]
    [SerializeField] private UnitHealthBar healthBarPrefab;
    private UnitHealthBar healthBarInstance;

    [Header("Runtime")]
    [SerializeField] protected int currentHp;
    [SerializeField] protected UnitState state = UnitState.Idle;

    protected float nextAttackTime;

    public UnitData Data => data;
    public int CurrentHp => currentHp;
    public UnitState State => state;
    public bool IsDead => state == UnitState.Dead;

    protected Rigidbody rb;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public virtual void Init(UnitData unitData)
    {
        if (unitData == null)
        {
            return;
        }

        data = unitData;
        currentHp = data.hp;
        state = UnitState.Idle;
        nextAttackTime = 0f;

        gameObject.name = data.unitName;
        CreateHealthBar();
    }
    private void CreateHealthBar()
    {
        if (healthBarPrefab == null)
            return;

        if (healthBarInstance != null)
            return;

        healthBarInstance = Instantiate(healthBarPrefab, transform);

        healthBarInstance.transform.localPosition = Vector3.up * 0.5f;
        healthBarInstance.transform.localRotation = Quaternion.identity;
        healthBarInstance.transform.localScale = Vector3.one;
    }

    public virtual void Move(Vector3 direction)
    {
        if (!CanAct())
            return;

        if (state == UnitState.Attacking)
            return;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            state = UnitState.Idle;
            return;
        }

        direction.Normalize();

        state = UnitState.Moving;

        if (rb != null)
        {
            Vector3 nextPosition =
                rb.position + direction * data.speed * Time.fixedDeltaTime;

            rb.MovePosition(nextPosition);
        }
        else
        {
            transform.position += direction * data.speed * Time.deltaTime;
        }

        RotateTo(direction);
    }

    public virtual bool Attack(Unit target)
    {
        if (!CanAct())
            return false;

        if (target == null || target.IsDead)
            return false;

        if (!IsTargetInAttackRange(target))
            return false;

        if (Time.time < nextAttackTime)
            return false;

        state = UnitState.Attacking;
        nextAttackTime = Time.time + data.atkSpeed;

        RotateTo(target.transform.position - transform.position);

        target.TakeDamage(data.atk);

        state = UnitState.Idle;

        return true;
    }

    public virtual void TakeDamage(int damage)
    {
        if (IsDead)
            return;

        int finalDamage = Mathf.Max(1, damage);
        currentHp -= finalDamage;

        if (currentHp <= 0)
        {
            currentHp = 0;
            Die();
        }
    }

    public virtual void Die()
    {
        if (IsDead)
            return;

        state = UnitState.Dead;
    }

    protected bool CanAct()
    {
        return data != null && !IsDead;
    }

    protected bool IsTargetInAttackRange(Unit target)
    {
        Vector3 offset = target.transform.position - transform.position;
        offset.y = 0f;

        float sqrDistance = offset.sqrMagnitude;
        float sqrRange = data.atkRange * data.atkRange;

        return sqrDistance <= sqrRange;
    }

    protected void RotateTo(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        transform.rotation = Quaternion.LookRotation(direction);
    }

    public virtual void Revive()
    {
        if (data == null)
            return;

        currentHp = data.hp;
        state = UnitState.Idle;
        nextAttackTime = 0f;
    }
}