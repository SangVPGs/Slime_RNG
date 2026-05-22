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

    [Header("Respawn Check Area")]
    [SerializeField] private Vector2 respawnCheckAreaSize = new Vector2(80f, 80f);
    [SerializeField] private float outOfRangeCheckInterval = 0.5f;

    [Header("First Map Limit")]
    [SerializeField] private bool limitFirstMapStart = true;
    [SerializeField] private float firstMapStartZ = -50f;

    [Header("Spawn Timing")]
    [SerializeField] private float initialSpawnDelay = 1f;
    [SerializeField] private float spawnInterval = 2f;

    [Header("Respawn")]
    [SerializeField] private float respawnDelay = 3f;

    private readonly List<SlimeUnit> slimePool = new();

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
        canStartSpawning = true;
    }

    private void Update()
    {
        FollowPlayerZ();
        HandleRespawn();
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
            SlimeUnit slime = Instantiate(
                slimePrefab,
                transform.position,
                Quaternion.identity
            );

            slime.Init(slimeData);
            slime.SetSpawner(this);

            slime.gameObject.SetActive(false);

            slimePool.Add(slime);
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

        Vector3 spawnPosition = GetRandomEdgePosition();
        slime.Respawn(spawnPosition);
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

            if (IsInsideRespawnCheckArea(slime.transform.position))
                continue;

            slime.Respawn(GetRandomEdgePosition());
        }
    }

    private bool IsInsideRespawnCheckArea(Vector3 position)
    {
        Vector3 center = transform.position;

        float halfWidth = respawnCheckAreaSize.x * 0.5f;
        float halfHeight = respawnCheckAreaSize.y * 0.5f;

        float minX = center.x - halfWidth;
        float maxX = center.x + halfWidth;

        float minZ = center.z - halfHeight;
        float maxZ = center.z + halfHeight;

        return position.x >= minX &&
               position.x <= maxX &&
               position.z >= minZ &&
               position.z <= maxZ;
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

    private void HandleRespawn()
    {
        foreach (SlimeUnit slime in slimePool)
        {
            if (slime == null)
                continue;

            slime.TickRespawn(Time.deltaTime);
        }
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
        slime.StartRespawn(respawnDelay);
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
        Vector3 spawnCenter = new Vector3(transform.position.x, spawnY, transform.position.z);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(
            spawnCenter,
            new Vector3(areaSize.x, 0.1f, areaSize.y)
        );

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(
            spawnCenter,
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