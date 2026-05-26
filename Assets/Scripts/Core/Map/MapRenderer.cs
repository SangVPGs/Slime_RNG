using System.Collections.Generic;
using UnityEngine;

public class MapRenderer : MonoBehaviour
{
    [Header("Map")]
    [SerializeField] private MapChunk mapTemplate;
    [SerializeField] private MapDatabase mapDatabase;
    [SerializeField] private Transform mapParent;

    [Header("Player In Scene")]
    [SerializeField] private Transform player;

    private readonly List<MapChunk> spawnedMaps = new();

    private void Awake()
    {
        RenderMaps();
        MovePlayerToSavedCheckpoint();
    }

    private void RenderMaps()
    {
        if (mapTemplate == null || mapDatabase == null)
            return;

        foreach (MapData data in mapDatabase.Maps)
            SpawnMap(data);
    }

    private void SpawnMap(MapData data)
    {
        if (data == null)
            return;

        MapChunk chunk = Instantiate(
            mapTemplate,
            Vector3.zero,
            Quaternion.identity,
            mapParent
        );

        chunk.Initialize(data);

        if (spawnedMaps.Count >= 1)
            AlignToPrevious(chunk, spawnedMaps[^1]);

        spawnedMaps.Add(chunk);
    }

    private void AlignToPrevious(MapChunk current, MapChunk previous)
    {
        Vector3 offset = current.StartPoint.position - current.transform.position;
        current.transform.position = previous.EndPoint.position - offset;
    }

    private void MovePlayerToSavedCheckpoint()
    {
        if (player == null)
        {
            return;
        }

        int savedLevel = MapProgressSave.LoadCurrentMapLevel(0);

        MapChunk targetMap = FindMapByLevel(savedLevel);

        if (targetMap == null)
            targetMap = FindMapByLevel(0);

        if (targetMap == null || targetMap.CheckPoint == null)
        {
            return;
        }

        CharacterController controller = player.GetComponent<CharacterController>();

        if (controller != null)
            controller.enabled = false;

        player.SetPositionAndRotation(
            targetMap.CheckPoint.position,
            targetMap.CheckPoint.rotation
        );

        if (controller != null)
            controller.enabled = true;
    }

    private MapChunk FindMapByLevel(int level)
    {
        foreach (MapChunk chunk in spawnedMaps)
        {
            if (chunk.Level == level)
                return chunk;
        }

        return null;
    }
}