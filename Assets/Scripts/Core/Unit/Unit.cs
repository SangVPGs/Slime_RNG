using System;
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
    private const string LevelKeyPrefix = "UnitLevel_";

    [Header("Data")]
    [SerializeField] protected UnitData data;

    [Header("Instance Save")]
    [SerializeField] protected string unitInstanceId;

    [Header("Level")]
    [SerializeField] protected int currentLevel = 1;

    [Header("Runtime Stats")]
    [SerializeField] protected double maxHp;
    [SerializeField] protected double atk;
    [SerializeField] protected float atkRange;
    [SerializeField] protected float atkSpeed;
    [SerializeField] protected float speed;

    [Header("Runtime")]
    [SerializeField] protected double currentHp;
    [SerializeField] protected UnitState state = UnitState.Idle;

    [Header("Visual")]
    [SerializeField] protected Transform visualRoot;
    protected GameObject visualInstance;

    [Header("Animation")]
    [SerializeField] protected Animator animator;
    [SerializeField] protected float attackAnimationDuration = 0.5f;

    [Header("Health Bar")]
    [SerializeField] private UnitHealthBar healthBarPrefab;
    [SerializeField] protected float reviveHealDuration = 3f;

    private UnitHealthBar healthBarInstance;

    private static readonly int IdleStateHash = Animator.StringToHash("Idle");
    private static readonly int MoveStateHash = Animator.StringToHash("Move");
    private static readonly int AttackStateHash = Animator.StringToHash("Attack");
    private static readonly int DeadStateHash = Animator.StringToHash("Die");

    private Coroutine attackRoutine;
    private Coroutine reviveRoutine;

    protected Rigidbody rb;
    protected float nextAttackTime;

    public UnitData Data => data;

    public string UnitInstanceId => unitInstanceId;

    public int CurrentLevel => currentLevel;

    public double MaxHp => maxHp;
    public double CurrentHp => currentHp;
    public double Atk => atk;
    public float AtkRange => atkRange;

    public bool IsDead => state == UnitState.Dead;

    protected virtual string LevelSaveKey => $"{LevelKeyPrefix}{unitInstanceId}";

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
        Init(unitData, null);
    }

    protected virtual bool UseSavedLevel => true;

    public virtual void Init(UnitData unitData, string instanceId)
    {
        if (unitData == null)
            return;

        data = unitData;

        if (!string.IsNullOrEmpty(instanceId))
            unitInstanceId = instanceId;

        EnsureInstanceId();

        if (UseSavedLevel)
            LoadLevel();
        else
            currentLevel = Mathf.Clamp(data.defaultLevel, 1, data.maxLevel);

        RecalculateStats();

        currentHp = maxHp;
        nextAttackTime = 0f;

        gameObject.name = data.unitName;

        CreateVisualModel();
        CreateHealthBar();

        SetState(UnitState.Idle, true);
    }

    protected virtual void EnsureInstanceId()
    {
        if (!string.IsNullOrEmpty(unitInstanceId))
            return;

        unitInstanceId = Guid.NewGuid().ToString();
    }

    public virtual bool LevelUp()
    {
        if (data == null)
            return false;

        if (currentLevel >= data.maxLevel)
            return false;

        currentLevel++;

        RecalculateStats();
        currentHp = maxHp;

        return true;
    }

    public virtual bool SetLevel(int level)
    {
        if (data == null)
            return false;

        int newLevel = Mathf.Clamp(level, 1, data.maxLevel);

        if (currentLevel == newLevel)
            return false;

        currentLevel = newLevel;

        RecalculateStats();
        currentHp = maxHp;

        return true;
    }

    protected virtual void LoadLevel()
    {
        if (data == null)
            return;

        EnsureInstanceId();

        currentLevel = PlayerPrefs.GetInt(LevelSaveKey, data.defaultLevel);
        currentLevel = Mathf.Clamp(currentLevel, 1, data.maxLevel);
    }

    protected virtual void SaveLevel()
    {
        if (data == null)
            return;

        EnsureInstanceId();

        PlayerPrefs.SetInt(LevelSaveKey, currentLevel);
        PlayerPrefs.Save();
    }

    public virtual void ClearSavedLevel()
    {
        EnsureInstanceId();

        PlayerPrefs.DeleteKey(LevelSaveKey);
        PlayerPrefs.Save();

        if (data != null)
        {
            currentLevel = data.defaultLevel;
            RecalculateStats();
            currentHp = maxHp;
        }
    }

    protected virtual void RecalculateStats()
    {
        if (data == null)
            return;

        maxHp = data.baseHp;
        atk = data.baseAtk;

        atkRange = data.baseAtkRange;
        atkSpeed = data.baseAtkSpeed;
        speed = data.baseSpeed;
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
            Vector3 nextPosition = rb.position + direction * speed * Time.fixedDeltaTime;
            rb.MovePosition(nextPosition);
        }
        else
        {
            transform.position += direction * speed * Time.deltaTime;
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

        float safeAtkSpeed = Mathf.Max(0.01f, atkSpeed);
        nextAttackTime = Time.time + 1f / safeAtkSpeed;

        RotateTo(target.transform.position - transform.position);
        SetState(UnitState.Attacking, true);

        target.TakeDamage(atk);

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

    public virtual void TakeDamage(double damage)
    {
        if (IsDead)
            return;

        double finalDamage = Math.Max(1, damage);
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

        if (reviveRoutine != null)
        {
            StopCoroutine(reviveRoutine);
            reviveRoutine = null;
        }

        SetState(UnitState.Dead, true);
    }

    public virtual void Revive()
    {
        if (data == null)
            return;

        RecalculateStats();

        if (reviveRoutine != null)
            StopCoroutine(reviveRoutine);

        reviveRoutine = StartCoroutine(ReviveRoutine());
    }

    private IEnumerator ReviveRoutine()
    {
        nextAttackTime = 0f;
        currentHp = 0;

        double elapsed = 0f;

        while (elapsed < reviveHealDuration)
        {
            elapsed += Time.deltaTime;

            double percent = Mathf.Clamp01((float)(double)elapsed / reviveHealDuration);
            currentHp = Math.Round(maxHp * percent);

            yield return null;
        }

        currentHp = maxHp;
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

    protected virtual bool CanAct()
    {
        return data != null && !IsDead;
    }

    protected virtual bool IsTargetInAttackRange(Unit target)
    {
        Vector3 offset = target.transform.position - transform.position;
        offset.y = 0f;

        float sqrDistance = offset.sqrMagnitude;
        float sqrRange = atkRange * atkRange;

        return sqrDistance <= sqrRange;
    }

    protected virtual void RotateTo(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        transform.rotation = Quaternion.LookRotation(direction);
    }
}