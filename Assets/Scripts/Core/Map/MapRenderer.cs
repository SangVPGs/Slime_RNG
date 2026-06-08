using System.Collections.Generic;
using UnityEngine;

public class MapRenderer : MonoBehaviour
{
    [Header("Map")]
    [SerializeField] private MapChunk mapTemplate;
    [SerializeField] private MapDatabase mapDatabase;
    [SerializeField] private Transform mapParent;

    [Header("Cost")]
    [SerializeField] private MapCostData mapCostData;

    [Header("Player In Scene")]
    [SerializeField] private Transform player;

    [Header("Endless")]
    [SerializeField] private int extraMapCountWhenAtHighest = 3;

    private readonly List<MapChunk> spawnedMaps = new();

    private void Awake()
    {
        if (!CanRender())
            return;

        int savedLevel = MapProgressSave.LoadCurrentMapLevel(1);

        RenderMapsToLevel(savedLevel);
        SpawnExtraMapsIfCurrentLevelIsHighest(savedLevel);
        MovePlayerToCheckpoint(savedLevel);
    }

    private bool CanRender()
    {
        if (mapTemplate == null)
        {
            return false;
        }

        if (mapDatabase == null || mapDatabase.Maps == null || mapDatabase.Maps.Count == 0)
        {
            return false;
        }

        return true;
    }

    private void RenderMapsToLevel(int targetLevel)
    {
        int highestDatabaseLevel = GetHighestDatabaseLevel();
        int finalLevel = Mathf.Max(targetLevel, highestDatabaseLevel);

        for (int level = 1; level <= finalLevel; level++)
        {
            SpawnMap(level);
        }
    }

    private void SpawnExtraMapsIfCurrentLevelIsHighest(int currentLevel)
    {
        int highestSpawnedLevel = GetHighestSpawnedLevel();

        if (currentLevel != highestSpawnedLevel)
            return;

        for (int i = 0; i < extraMapCountWhenAtHighest; i++)
        {
            int nextLevel = GetHighestSpawnedLevel() + 1;
            SpawnMap(nextLevel);
        }
    }

    private void SpawnMap(int runtimeLevel)
    {
        MapData data = GetMapDataByRuntimeLevel(runtimeLevel);

        if (data == null)
        {
            return;
        }

        long unlockCost = mapCostData != null ? mapCostData.GetCost(runtimeLevel) : 0;

        MapChunk chunk = Instantiate(mapTemplate, mapParent);

        chunk.Initialize(data, runtimeLevel, unlockCost);

        if (spawnedMaps.Count > 0)
            AlignToPrevious(chunk, spawnedMaps[^1]);

        spawnedMaps.Add(chunk);
    }

    private MapData GetMapDataByRuntimeLevel(int runtimeLevel)
    {
        foreach (MapData data in mapDatabase.Maps)
        {
            if (data != null && data.level == runtimeLevel)
                return data;
        }

        int index = (runtimeLevel - 1) % mapDatabase.Maps.Count;
        return mapDatabase.Maps[index];
    }

    private void AlignToPrevious(MapChunk current, MapChunk previous)
    {
        if (current.StartPoint == null || previous.EndPoint == null)
            return;

        Vector3 offset = current.StartPoint.position - current.transform.position;
        current.transform.position = previous.EndPoint.position - offset;
    }

    private void MovePlayerToCheckpoint(int savedLevel)
    {
        if (player == null)
            return;

        MapChunk targetMap = FindMapByLevel(savedLevel);

        if (targetMap == null)
        {
            targetMap = FindMapByLevel(1);
        }

        if (targetMap == null || targetMap.CheckPoint == null)
            return;

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
            if (chunk != null && chunk.Level == level)
                return chunk;
        }

        return null;
    }

    private int GetHighestDatabaseLevel()
    {
        int highest = 0;

        foreach (MapData data in mapDatabase.Maps)
        {
            if (data != null && data.level > highest)
                highest = data.level;
        }

        return highest;
    }

    private int GetHighestSpawnedLevel()
    {
        int highest = 0;

        foreach (MapChunk chunk in spawnedMaps)
        {
            if (chunk != null && chunk.Level > highest)
                highest = chunk.Level;
        }

        return highest;
    }
}