using System.Collections;
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

    [Header("Visual")]
    [SerializeField] protected Transform visualRoot;
    protected GameObject visualInstance;

    [Header("Animation")]
    [SerializeField] protected Animator animator;
    [SerializeField] protected float attackAnimationDuration = 0.5f;

    private static readonly int IdleStateHash = Animator.StringToHash("Idle");
    private static readonly int MoveStateHash = Animator.StringToHash("Move");
    private static readonly int AttackStateHash = Animator.StringToHash("Attack");
    private static readonly int DeadStateHash = Animator.StringToHash("Die");

    private Coroutine attackRoutine;
    private Coroutine reviveRoutine;

    [Header("Health Bar")]
    [SerializeField] private UnitHealthBar healthBarPrefab;
    [SerializeField] protected float reviveHealDuration = 3f;
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

        if (animator == null)
            animator = GetComponent<Animator>();

        if (visualRoot == null)
        {
            Transform foundVisual = transform.Find("Visual");

            if (foundVisual != null)
                visualRoot = foundVisual;
        }
    }

    public virtual void Init(UnitData unitData)
    {
        if (unitData == null)
            return;

        data = unitData;
        currentHp = data.hp;
        nextAttackTime = 0f;

        gameObject.name = data.unitName;

        CreateVisualModel();
        CreateHealthBar();

        SetState(UnitState.Idle, true);
    }

    protected virtual void CreateVisualModel()
    {
        if (visualRoot == null)
        {
            Debug.LogError($"{name} missing Visual root.");
            return;
        }

        ClearVisualModel();

        if (data.model == null)
        {
            Debug.LogWarning($"{name} has no model in UnitData.");
            return;
        }

        visualInstance = Instantiate(data.model, visualRoot);
        visualInstance.transform.localPosition = Vector3.zero;
        visualInstance.transform.localRotation = Quaternion.identity;
    }

    protected virtual void ClearVisualModel()
    {
        if (visualRoot == null)
            return;

        for (int i = visualRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(visualRoot.GetChild(i).gameObject);
        }

        visualInstance = null;
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
            SetState(UnitState.Idle);
            return;
        }

        direction.Normalize();

        SetState(UnitState.Moving);

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

        if (state == UnitState.Attacking)
            return false;

        if (target == null || target.IsDead)
            return false;

        if (!IsTargetInAttackRange(target))
            return false;

        if (Time.time < nextAttackTime)
            return false;

        nextAttackTime = Time.time + (1f / data.atkSpeed);

        RotateTo(target.transform.position - transform.position);
        SetState(UnitState.Attacking, true);

        target.TakeDamage(data.atk);

        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        attackRoutine = StartCoroutine(EndAttackAfterDelay());

        return true;
    }

    private IEnumerator EndAttackAfterDelay()
    {
        yield return new WaitForSeconds(attackAnimationDuration);

        if (!IsDead)
            SetState(UnitState.Idle);

        attackRoutine = null;
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

        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        SetState(UnitState.Dead, true);
    }

    public virtual void Revive()
    {
        if (data == null)
            return;

        if (reviveRoutine != null)
            StopCoroutine(reviveRoutine);

        reviveRoutine = StartCoroutine(ReviveRoutine());
    }

    private IEnumerator ReviveRoutine()
    {
        nextAttackTime = 0f;

        currentHp = 0;

        float elapsed = 0f;

        while (elapsed < reviveHealDuration)
        {
            elapsed += Time.deltaTime;

            float percent = Mathf.Clamp01(elapsed / reviveHealDuration);

            currentHp = Mathf.RoundToInt(data.hp * percent);

            yield return null;
        }

        currentHp = data.hp;

        reviveRoutine = null;

        SetState(UnitState.Idle, true);
    }

    protected void SetState(UnitState newState, bool forceRefreshAnimation = false)
    {
        if (!forceRefreshAnimation && state == newState)
            return;

        state = newState;
        UpdateAnimationByState();
    }

    protected virtual void UpdateAnimationByState()
    {
        if (animator == null)
            return;

        switch (state)
        {
            case UnitState.Idle:
                animator.CrossFade(IdleStateHash, 0.1f);
                break;

            case UnitState.Moving:
                animator.CrossFade(MoveStateHash, 0.1f);
                break;

            case UnitState.Attacking:
                animator.CrossFade(AttackStateHash, 0.05f);
                break;

            case UnitState.Dead:
                animator.CrossFade(DeadStateHash, 0.05f);
                break;
        }
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
}