using System.Collections.Generic;
using UnityEngine;

public readonly struct SlimeSpawnContext
{
    public readonly int MapLevel;
    public readonly IReadOnlyList<SlimeUnitData> Enemies;

    public SlimeSpawnContext(int mapLevel, IReadOnlyList<SlimeUnitData> enemies)
    {
        MapLevel = mapLevel;
        Enemies = enemies;
    }
}

public class SlimeSpawner : MonoBehaviour
{
    public static SlimeSpawner Instance { get; private set; }

    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Slime")]
    [SerializeField] private SlimeUnit slimePrefab;
    [SerializeField] private int poolSize = 5;

    [Header("Spawn Area")]
    [SerializeField] private Vector2 areaSize = new Vector2(50f, 50f);
    [SerializeField] private float spawnY = 0f;
    [SerializeField] private float edgeInset = 5f;

    [Header("Out Of Range By Z")]
    [SerializeField] private float maxZDistanceFromPlayer = 80f;
    [SerializeField] private float outOfRangeCheckInterval = 1f;
    [SerializeField] private float outOfRangeRespawnCooldown = 3f;

    [Header("World Min Spawn Z Limit")]
    [SerializeField] private bool limitFirstMapStart = true;
    [SerializeField] private float firstMapStartZ = -50f;

    [Header("Spawn Timing")]
    [SerializeField] private float initialSpawnDelay = 1f;
    [SerializeField] private float spawnInterval = 2f;

    [Header("Death Respawn")]
    [SerializeField] private float deathRespawnDelay = 3f;

    private readonly List<SlimeUnit> slimePool = new();
    private readonly Dictionary<SlimeUnit, float> nextOutOfRangeRespawnTime = new();
    private readonly List<SlimeUnitData> currentEnemies = new();

    private SlimeSpawnContext currentContext;
    private float spawnTimer;
    private float outOfRangeTimer;
    private bool canStartSpawning;

    private void Awake()
    {
        SetupSingleton();
        FindPlayerIfNeeded();
        CreatePool();
    }

    private void Start()
    {
        spawnTimer = initialSpawnDelay;
        outOfRangeTimer = outOfRangeCheckInterval;
        canStartSpawning = false;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        FindPlayerIfNeeded();
        FollowPlayerZ();

        HandleDeathRespawn();
        HandleOutOfRangeRespawn();
        HandleSpawn();
    }

    private void SetupSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetContext(SlimeSpawnContext context)
    {
        currentContext = context;
        currentEnemies.Clear();

        if (context.Enemies != null)
        {
            foreach (SlimeUnitData enemy in context.Enemies)
            {
                if (enemy != null)
                    currentEnemies.Add(enemy);
            }
        }

        canStartSpawning = currentEnemies.Count > 0;

        if (canStartSpawning)
            spawnTimer = initialSpawnDelay;
    }

    private void FindPlayerIfNeeded()
    {
        if (player != null)
            return;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;
    }

    private void CreatePool()
    {
        if (slimePrefab == null)
        {
            Debug.LogError("Slime prefab missing.");
            return;
        }

        for (int i = 0; i < poolSize; i++)
        {
            SlimeUnit slime = Instantiate(
                slimePrefab,
                transform.position,
                Quaternion.identity
            );

            slime.SetSpawner(this);
            slime.gameObject.SetActive(false);

            slimePool.Add(slime);
            nextOutOfRangeRespawnTime[slime] = 0f;
        }
    }

    private void HandleSpawn()
    {
        if (!canStartSpawning)
            return;

        spawnTimer -= Time.deltaTime;

        if (spawnTimer > 0f)
            return;

        spawnTimer = spawnInterval;

        SlimeUnit slime = GetInactiveSlime();

        if (slime == null)
            return;

        RespawnSlime(slime);
    }

    private void HandleDeathRespawn()
    {
        foreach (SlimeUnit slime in slimePool)
        {
            if (slime == null)
                continue;

            if (!slime.TickRespawn(Time.deltaTime))
                continue;

            RespawnSlime(slime);
        }
    }

    private void HandleOutOfRangeRespawn()
    {
        if (player == null)
            return;

        outOfRangeTimer -= Time.deltaTime;

        if (outOfRangeTimer > 0f)
            return;

        outOfRangeTimer = outOfRangeCheckInterval;

        foreach (SlimeUnit slime in slimePool)
        {
            if (!CanForceRespawn(slime))
                continue;

            float zDistance = Mathf.Abs(slime.transform.position.z - player.position.z);

            if (zDistance <= maxZDistanceFromPlayer)
                continue;

            RespawnSlime(slime);
            nextOutOfRangeRespawnTime[slime] = Time.time + outOfRangeRespawnCooldown;
        }
    }

    private bool CanForceRespawn(SlimeUnit slime)
    {
        if (slime == null)
            return false;

        if (!slime.gameObject.activeSelf)
            return false;

        if (slime.IsDead || slime.IsWaitingRespawn)
            return false;

        if (!nextOutOfRangeRespawnTime.ContainsKey(slime))
            nextOutOfRangeRespawnTime[slime] = 0f;

        return Time.time >= nextOutOfRangeRespawnTime[slime];
    }

    private SlimeUnit GetInactiveSlime()
    {
        foreach (SlimeUnit slime in slimePool)
        {
            if (slime == null)
                continue;

            if (slime.IsWaitingRespawn)
                continue;

            if (!slime.gameObject.activeSelf)
                return slime;
        }

        return null;
    }

    private void RespawnSlime(SlimeUnit slime)
    {
        if (slime == null)
            return;

        SlimeUnitData data = GetRandomCurrentEnemyData();

        if (data == null)
            return;

        slime.Spawn(
            data,
            currentContext.MapLevel,
            GetRandomEdgePosition()
        );
    }

    private SlimeUnitData GetRandomCurrentEnemyData()
    {
        if (currentEnemies.Count == 0)
            return null;

        return currentEnemies[Random.Range(0, currentEnemies.Count)];
    }

    private void FollowPlayerZ()
    {
        if (player == null)
            return;

        Vector3 position = transform.position;
        position.z = player.position.z;
        transform.position = position;
    }

    public void RequestRespawn(SlimeUnit slime)
    {
        if (slime == null)
            return;

        slime.StartRespawn(deathRespawnDelay);
    }

    private Vector3 GetRandomEdgePosition()
    {
        float halfWidth = areaSize.x * 0.5f;
        float halfHeight = areaSize.y * 0.5f;

        float safeHalfWidth = Mathf.Max(0f, halfWidth - edgeInset);
        float safeHalfHeight = Mathf.Max(0f, halfHeight - edgeInset);

        int edge = Random.Range(0, 4);

        float x = 0f;
        float z = 0f;

        switch (edge)
        {
            case 0:
                x = Random.Range(-safeHalfWidth, safeHalfWidth);
                z = safeHalfHeight;
                break;

            case 1:
                x = Random.Range(-safeHalfWidth, safeHalfWidth);
                z = -safeHalfHeight;
                break;

            case 2:
                x = -safeHalfWidth;
                z = Random.Range(-safeHalfHeight, safeHalfHeight);
                break;

            case 3:
                x = safeHalfWidth;
                z = Random.Range(-safeHalfHeight, safeHalfHeight);
                break;
        }

        float finalX = transform.position.x + x;
        float finalZ = transform.position.z + z;

        finalZ = ClampMinSpawnZ(finalZ);

        return new Vector3(finalX, spawnY, finalZ);
    }

    private float ClampMinSpawnZ(float z)
    {
        if (!limitFirstMapStart)
            return z;

        return Mathf.Max(z, firstMapStartZ);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 center = new Vector3(transform.position.x, spawnY, transform.position.z);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, new Vector3(areaSize.x, 0.1f, areaSize.y));

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(
            center,
            new Vector3(areaSize.x, 0.15f, maxZDistanceFromPlayer * 2f)
        );

        if (limitFirstMapStart)
        {
            Gizmos.color = Color.red;

            Vector3 startLineCenter = new Vector3(
                transform.position.x,
                spawnY + 0.05f,
                firstMapStartZ
            );

            Vector3 startLineSize = new Vector3(
                areaSize.x,
                0.1f,
                0.2f
            );

            Gizmos.DrawWireCube(startLineCenter, startLineSize);
        }
    }
#endif
}