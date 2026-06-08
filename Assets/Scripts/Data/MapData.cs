using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Map")]
public class MapData : ScriptableObject
{
    public int level;
    public GameObject mapPrefab;

    [Header("Enemies")]
    public List<SlimeUnitData> enemies;
    public EnemyStatData enemyStats;
}

[Serializable]
public class EnemyStatData
{
    public long hp = 1000;
    public long atk = 100;
    public long goldDrop = 10;

    public long Hp => (long)Mathf.Max(1, hp);
    public long Atk => (long)Mathf.Max(1, atk);
    public long GoldDrop => (long)Mathf.Max(0, goldDrop);
}