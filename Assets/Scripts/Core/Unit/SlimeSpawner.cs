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

    [Header("Out Of Range By Z")]
    [SerializeField] private float maxZDistanceFromPlayer = 80f;
    [SerializeField] private float outOfRangeCheckInterval = 1f;
    [SerializeField] private float outOfRangeRespawnCooldown = 3f;

    [Header("First Map Limit")]
    [SerializeField] private bool limitFirstMapStart = true;
    [SerializeField] private float firstMapStartZ = -50f;

    [Header("Spawn Timing")]
    [SerializeField] private float initialSpawnDelay = 1f;
    [SerializeField] private float spawnInterval = 2f;

    [Header("Death Respawn")]
    [SerializeField] private float deathRespawnDelay = 3f;

    private readonly List<SlimeUnit> slimePool = new();
    private readonly Dictionary<SlimeUnit, float> nextForceRespawnTime = new();

    private float spawnTimer;
    private float outOfRangeTimer;
    private bool canStartSpawning;

    private void Awake()
    {
        FindPlayerIfNeeded();
        CreatePool();
    }

    private void Start()
    {
        spawnTimer = initialSpawnDelay;
        outOfRangeTimer = outOfRangeCheckInterval;
        canStartSpawning = true;
    }

    private void Update()
    {
        FindPlayerIfNeeded();
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
            return;

        for (int i = 0; i < poolSize; i++)
        {
            SlimeUnit slime = Instantiate(slimePrefab, transform.position, Quaternion.identity);

            slime.Init(slimeData);
            slime.SetSpawner(this);
            slime.gameObject.SetActive(false);

            slimePool.Add(slime);
            nextForceRespawnTime[slime] = 0f;
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
        if (player == null)
            return;

        outOfRangeTimer -= Time.deltaTime;

        if (outOfRangeTimer > 0f)
            return;

        outOfRangeTimer = outOfRangeCheckInterval;

        foreach (SlimeUnit slime in slimePool)
        {
            if (slime == null)
                continue;

            if (!slime.gameObject.activeSelf)
                continue;

            if (slime.IsDead || slime.IsWaitingRespawn)
                continue;

            if (!nextForceRespawnTime.ContainsKey(slime))
                nextForceRespawnTime[slime] = 0f;

            if (Time.time < nextForceRespawnTime[slime])
                continue;

            float zDistance = Mathf.Abs(slime.transform.position.z - player.position.z);

            if (zDistance <= maxZDistanceFromPlayer)
                continue;

            RespawnSlime(slime);
        }
    }

    private void RespawnSlime(SlimeUnit slime)
    {
        slime.Respawn(GetRandomEdgePosition());
        nextForceRespawnTime[slime] = Time.time + outOfRangeRespawnCooldown;
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
            case 0:
                x = Random.Range(-halfWidth, halfWidth);
                z = halfHeight;
                break;

            case 1:
                x = Random.Range(-halfWidth, halfWidth);
                z = -halfHeight;
                break;

            case 2:
                x = -halfWidth;
                z = Random.Range(-halfHeight, halfHeight);
                break;

            case 3:
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
        Gizmos.DrawWireCube(center, new Vector3(areaSize.x, 0.1f, areaSize.y));

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(
            center,
            new Vector3(areaSize.x, 0.15f, maxZDistanceFromPlayer * 2f)
        );
    }
#endif
}