using UnityEngine;
using UnityEngine.AI;

public class SlimeUnit : Unit
{
    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Combat Target")]
    [SerializeField] private PetUnit currentPetTarget;

    [Header("Pet Detection")]
    [SerializeField] private LayerMask petLayer;
    [SerializeField] private float scanInterval = 0.25f;
    [SerializeField] private float detectRange = 3f;

    [Header("Movement")]
    [SerializeField] private float stopDistanceFromPlayer = 2f;

    [Header("Gold Drop")]
    [SerializeField] private long goldDrop;

    private SlimeSpawner spawner;

    private bool waitingRespawn;
    private float respawnTimer;
    private float nextScanTime;

    private NavMeshAgent[] navMeshAgents;
    private NavMeshObstacle[] navMeshObstacles;
    private Animator[] animators;

    public bool IsWaitingRespawn => waitingRespawn;
    public SlimeUnitData SlimeData => data as SlimeUnitData;
    private EnemyStatData currentEnemyStat;

    protected override void Awake()
    {
        base.Awake();

        CacheUnusedComponents();
        DisableUnusedNavigation();
        DisableRootMotion();
    }

    public override void Init(UnitData unitData)
    {
        if (unitData is not SlimeUnitData)
            return;

        base.Init(unitData);

        ResetSlimeRuntime();
    }

    public override void Init(UnitData unitData, string instanceId)
    {
        if (unitData is not SlimeUnitData)
            return;

        base.Init(unitData, instanceId);

        ResetSlimeRuntime();
    }

    protected override void RecalculateStats()
    {
        base.RecalculateStats();

        if (currentEnemyStat == null)
            return;

        maxHp = currentEnemyStat.Hp;
        atk = currentEnemyStat.Atk;
    }

    private void FixedUpdate()
    {
        if (!CanAct())
            return;

        FindPlayerIfNeeded();
        ScanPetByInterval();

        if (HasValidPetTarget())
        {
            HandlePetTarget();
            return;
        }

        MoveToPlayer();
    }

    private void ResetSlimeRuntime()
    {
        currentPetTarget = null;
        nextScanTime = 0f;
        waitingRespawn = false;
        respawnTimer = 0f;

        DisableUnusedNavigation();
        DisableRootMotion();
    }

    private void CacheUnusedComponents()
    {
        navMeshAgents = GetComponentsInChildren<NavMeshAgent>(true);
        navMeshObstacles = GetComponentsInChildren<NavMeshObstacle>(true);
        animators = GetComponentsInChildren<Animator>(true);
    }

    private void DisableUnusedNavigation()
    {
        if (navMeshAgents != null)
        {
            foreach (NavMeshAgent agent in navMeshAgents)
            {
                if (agent != null)
                    agent.enabled = false;
            }
        }

        if (navMeshObstacles != null)
        {
            foreach (NavMeshObstacle obstacle in navMeshObstacles)
            {
                if (obstacle != null)
                    obstacle.enabled = false;
            }
        }
    }

    private void DisableRootMotion()
    {
        if (animators == null)
            return;

        foreach (Animator animator in animators)
        {
            if (animator != null)
                animator.applyRootMotion = false;
        }
    }

    public void Spawn(SlimeUnitData slimeData, EnemyStatData enemyStats, Vector3 position)
    {
        if (slimeData == null)
            return;

        waitingRespawn = false;
        respawnTimer = 0f;

        Physics.SyncTransforms();

        if (visualRoot != null)
        {
            visualRoot.localPosition = Vector3.zero;
            visualRoot.localRotation = Quaternion.identity;
            visualRoot.localScale = Vector3.one;
        }

        currentEnemyStat = enemyStats;

        gameObject.SetActive(true);

        Init(slimeData);

        currentHp = maxHp;

        DisableUnusedNavigation();
        DisableRootMotion();

        transform.position = position;

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = position;
        }

        currentPetTarget = null;
        nextScanTime = 0f;

        Revive();
    }

    private void FindPlayerIfNeeded()
    {
        if (player != null)
            return;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;
    }

    private void ScanPetByInterval()
    {
        if (Time.time < nextScanTime)
            return;

        nextScanTime = Time.time + scanInterval;

        if (currentPetTarget != null && currentPetTarget.IsDead)
            currentPetTarget = null;

        PetUnit foundPet = FindFirstAlivePetInDetectRange();

        if (foundPet != null)
            currentPetTarget = foundPet;
    }

    private PetUnit FindFirstAlivePetInDetectRange()
    {
        float range = Mathf.Max(detectRange, atkRange);

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            range,
            petLayer
        );

        foreach (Collider hit in hits)
        {
            PetUnit pet = hit.GetComponentInParent<PetUnit>();

            if (pet == null)
                continue;

            if (pet.IsDead)
                continue;

            return pet;
        }

        return null;
    }

    private bool HasValidPetTarget()
    {
        return currentPetTarget != null && !currentPetTarget.IsDead;
    }

    private void HandlePetTarget()
    {
        if (IsTargetInAttackRange(currentPetTarget))
        {
            Attack(currentPetTarget);
            return;
        }

        Vector3 direction = currentPetTarget.transform.position - transform.position;
        direction.y = 0f;

        Move(direction);
    }

    private void MoveToPlayer()
    {
        if (player == null)
        {
            SetState(UnitState.Idle);
            return;
        }

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= stopDistanceFromPlayer * stopDistanceFromPlayer)
        {
            SetState(UnitState.Idle);
            return;
        }

        Move(direction);
    }

    public override bool Attack(Unit target)
    {
        return base.Attack(target);
    }

    public override void TakeDamage(long damage)
    {
        base.TakeDamage(damage);
    }

    public override void Die()
    {
        if (IsDead)
            return;

        base.Die();

        if (GameManager.Instance != null)
            GameManager.Instance.AddGold(currentEnemyStat.goldDrop);
    }

    public override bool LevelUp()
    {
        bool success = base.LevelUp();

        if (!success)
            return false;

        return true;
    }

    public void OnDeathAnimationFinished()
    {
        if (!IsDead)
            return;

        if (ItemDropSystem.Instance != null)
            ItemDropSystem.Instance.DropRandomItem(transform.position);

        gameObject.SetActive(false);

        if (spawner != null)
            spawner.RequestRespawn(this);
    }

    public void SetSpawner(SlimeSpawner slimeSpawner)
    {
        spawner = slimeSpawner;
    }

    public void StartRespawn(float delay)
    {
        waitingRespawn = true;
        respawnTimer = delay;
    }

    public bool TickRespawn(float deltaTime)
    {
        if (!waitingRespawn)
            return false;

        if (gameObject.activeSelf)
            return false;

        respawnTimer -= deltaTime;

        return respawnTimer <= 0f;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, atkRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistanceFromPlayer);
    }
#endif
}