using UnityEngine;

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
    public bool IsWaitingRespawn => waitingRespawn;

    private float nextScanTime;

    public SlimeUnitData SlimeData => data as SlimeUnitData;

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
        {
            return;
        }

        base.Init(unitData);

        currentPetTarget = null;
        nextScanTime = 0f;
    }

    public void Respawn(Vector3 position)
    {
        waitingRespawn = false;
        respawnTimer = 0f;

        transform.position = position;

        Revive();
        gameObject.SetActive(true);

        currentPetTarget = null;
        nextScanTime = 0f;
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
        {
            currentPetTarget = null;
        }

        PetUnit foundPet = FindFirstAlivePetInDetectRange();

        if (foundPet != null)
        {
            currentPetTarget = foundPet;
        }
    }

    private PetUnit FindFirstAlivePetInDetectRange()
    {
        float range = Mathf.Max(detectRange, data.atkRange);

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            range,
            petLayer
        );

        foreach (Collider hit in hits)
        {
            PetUnit pet = hit.GetComponent<PetUnit>();

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
            state = UnitState.Attacking;
            Attack(currentPetTarget);
            return;
        }

        state = UnitState.Moving;

        Vector3 direction =
            currentPetTarget.transform.position - transform.position;

        direction.y = 0f;

        Move(direction);
    }

    private void MoveToPlayer()
    {
        if (player == null)
        {
            state = UnitState.Idle;
            return;
        }

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= stopDistanceFromPlayer * stopDistanceFromPlayer)
        {
            state = UnitState.Idle;
            return;
        }

        state = UnitState.Moving;
        Move(direction);
    }

    public override bool Attack(Unit target)
    {
        bool attacked = base.Attack(target);

        if (attacked)
        {
            // Animation
        }

        return attacked;
    }

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
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

        respawnTimer -= deltaTime;

        if (respawnTimer > 0f)
            return;

        Respawn(spawner.GetRespawnPosition());
    }

    public override void Die()
    {
        base.Die();

        currentPetTarget = null;
        gameObject.SetActive(false);

        if (spawner != null)
        {
            spawner.RequestRespawn(this);
        }
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