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

    private SlimeSpawner spawner;

    private bool waitingRespawn;
    private float respawnTimer;
    private float nextScanTime;

    private NavMeshAgent[] navMeshAgents;
    private NavMeshObstacle[] navMeshObstacles;
    private Animator[] animators;

    public bool IsWaitingRespawn => waitingRespawn;
    public SlimeUnitData SlimeData => data as SlimeUnitData;

    protected override void Awake()
    {
        base.Awake();

        CacheUnusedComponents();
        DisableUnusedNavigation();
        DisableRootMotion();
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

    public override void Init(UnitData unitData)
    {
        if (unitData is not SlimeUnitData)
            return;

        base.Init(unitData);

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

    public void Respawn(Vector3 position)
    {
        waitingRespawn = false;
        respawnTimer = 0f;

        DisableUnusedNavigation();
        DisableRootMotion();

        transform.position = position;

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = position;
        }

        Physics.SyncTransforms();

        if (visualRoot != null)
        {
            visualRoot.localPosition = Vector3.zero;
            visualRoot.localRotation = Quaternion.identity;
            visualRoot.localScale = Vector3.one;
        }

        currentPetTarget = null;
        nextScanTime = 0f;

        gameObject.SetActive(true);

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
        if (data == null)
            return null;

        float range = Mathf.Max(detectRange, data.atkRange);

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

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
    }

    public override void Die()
    {
        if (IsDead)
            return;

        base.Die();

        if (GameManager.Instance != null && SlimeData != null)
            GameManager.Instance.AddGold(SlimeData.goldDrop);
    }

    public void OnDeathAnimationFinished()
    {
        if (!IsDead)
            return;

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
        if (waitingRespawn)
            return;

        waitingRespawn = true;
        respawnTimer = delay;
    }

    public void TickRespawn(float deltaTime)
    {
        if (!waitingRespawn)
            return;

        if (gameObject.activeSelf)
            return;

        if (spawner == null)
            return;

        respawnTimer -= deltaTime;

        if (respawnTimer > 0f)
            return;

        Respawn(spawner.GetRespawnPosition());
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (data == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, data.atkRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistanceFromPlayer);
    }
#endif
}