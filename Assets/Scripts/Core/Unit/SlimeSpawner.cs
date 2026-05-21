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

    [Header("Spawn Timing")]
    [SerializeField] private float initialSpawnDelay = 1f;
    [SerializeField] private float spawnInterval = 2f;

    [Header("Respawn")]
    [SerializeField] private float respawnDelay = 3f;

    private readonly List<SlimeUnit> slimePool = new();

    private float spawnTimer;
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

        return new Vector3(
            transform.position.x + x,
            spawnY,
            transform.position.z + z
        );
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Vector3 center = new Vector3(
            transform.position.x,
            spawnY,
            transform.position.z
        );

        Vector3 size = new Vector3(
            areaSize.x,
            0.1f,
            areaSize.y
        );

        Gizmos.DrawWireCube(center, size);
    }
#endif
}