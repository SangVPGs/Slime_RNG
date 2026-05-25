using System.Collections.Generic;
using UnityEngine;

public class SlimeSpawner : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Slime")]
    [SerializeField] private SlimeUnitData slimeData;
    [SerializeField] private SlimeUnit slimePrefab;
    [SerializeField] private int poolSize = 5;

    [Header("Spawn Area")]
    [SerializeField] private Vector2 areaSize = new Vector2(50f, 50f);
    [SerializeField] private float spawnY = 0f;

    [Header("Out Of Range Check")]
    [SerializeField] private Vector2 respawnCheckAreaSize = new Vector2(100f, 100f);
    [SerializeField] private float outOfRangeCheckInterval = 0.75f;
    [SerializeField] private float outOfRangeRespawnCooldown = 2f;

    [Header("First Map Limit")]
    [SerializeField] private bool limitFirstMapStart = true;
    [SerializeField] private float firstMapStartZ = -50f;

    [Header("Spawn Timing")]
    [SerializeField] private float initialSpawnDelay = 1f;
    [SerializeField] private float spawnInterval = 2f;

    [Header("Death Respawn")]
    [SerializeField] private float deathRespawnDelay = 3f;

    private readonly List<SlimeUnit> slimePool = new();
    private readonly Dictionary<SlimeUnit, float> nextOutOfRangeRespawnTime = new();

    private float spawnTimer;
    private float outOfRangeCheckTimer;
    private bool canStartSpawning;

    private void Awake()
    {
        FindPlayerIfNeeded();
        CreatePool();
    }

    private void Start()
    {
        spawnTimer = initialSpawnDelay;
        outOfRangeCheckTimer = outOfRangeCheckInterval;
        canStartSpawning = true;
    }

    private void Update()
    {
        FollowPlayerZ();

        HandleDeathRespawn();
        HandleOutOfRangeRespawn();
        HandleSpawn();
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
        if (slimeData == null || slimePrefab == null)
        {
            Debug.LogWarning("SlimeData hoặc SlimePrefab chưa được gán.");
            return;
        }

        for (int i = 0; i < poolSize; i++)
        {
            SlimeUnit slime = Instantiate(
                slimePrefab,
                transform.position,
                Quaternion.identity
            );

            slime.Init(slimeData);
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

        slime.Respawn(GetRandomEdgePosition());
        nextOutOfRangeRespawnTime[slime] = Time.time + outOfRangeRespawnCooldown;
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

    private void HandleDeathRespawn()
    {
        foreach (SlimeUnit slime in slimePool)
        {
            if (slime == null)
                continue;

            slime.TickRespawn(Time.deltaTime);
        }
    }

    private void HandleOutOfRangeRespawn()
    {
        outOfRangeCheckTimer -= Time.deltaTime;

        if (outOfRangeCheckTimer > 0f)
            return;

        outOfRangeCheckTimer = outOfRangeCheckInterval;

        foreach (SlimeUnit slime in slimePool)
        {
            if (slime == null)
                continue;

            if (!slime.gameObject.activeSelf)
                continue;

            if (slime.IsDead)
                continue;

            if (slime.IsWaitingRespawn)
                continue;

            if (Time.time < nextOutOfRangeRespawnTime[slime])
                continue;

            if (IsInsideRespawnCheckArea(slime.transform.position))
                continue;

            slime.Respawn(GetRandomEdgePosition());
            nextOutOfRangeRespawnTime[slime] = Time.time + outOfRangeRespawnCooldown;
        }
    }

    private bool IsInsideRespawnCheckArea(Vector3 position)
    {
        Vector3 center = transform.position;

        float halfWidth = respawnCheckAreaSize.x * 0.5f;
        float halfHeight = respawnCheckAreaSize.y * 0.5f;

        return position.x >= center.x - halfWidth &&
               position.x <= center.x + halfWidth &&
               position.z >= center.z - halfHeight &&
               position.z <= center.z + halfHeight;
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

    public Vector3 GetRespawnPosition()
    {
        return GetRandomEdgePosition();
    }

    private Vector3 GetRandomEdgePosition()
    {
        float halfWidth = areaSize.x * 0.5f;
        float halfHeight = areaSize.y * 0.5f;

        int edge = Random.Range(0, 4);

        float x = 0f;
        float z = 0f;

        switch (edge)
        {
            case 0: // Top
                x = Random.Range(-halfWidth, halfWidth);
                z = halfHeight;
                break;

            case 1: // Bottom
                x = Random.Range(-halfWidth, halfWidth);
                z = -halfHeight;
                break;

            case 2: // Left
                x = -halfWidth;
                z = Random.Range(-halfHeight, halfHeight);
                break;

            case 3: // Right
                x = halfWidth;
                z = Random.Range(-halfHeight, halfHeight);
                break;
        }

        float finalX = transform.position.x + x;
        float finalZ = transform.position.z + z;

        if (limitFirstMapStart)
            finalZ = Mathf.Max(finalZ, firstMapStartZ);

        return new Vector3(finalX, spawnY, finalZ);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 center = new Vector3(transform.position.x, spawnY, transform.position.z);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(
            center,
            new Vector3(areaSize.x, 0.1f, areaSize.y)
        );

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(
            center,
            new Vector3(respawnCheckAreaSize.x, 0.15f, respawnCheckAreaSize.y)
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
                respawnCheckAreaSize.x,
                0.1f,
                0.2f
            );

            Gizmos.DrawWireCube(startLineCenter, startLineSize);
        }
    }
#endif
}