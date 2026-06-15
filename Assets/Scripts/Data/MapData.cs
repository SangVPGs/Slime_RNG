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
    public double hp = 1000;
    public double atk = 100;
    public double goldDrop = 10;

    public float poonChance = 0.3f;
    public double poonDrop = 1;

    public double Hp => (double)Math.Max(1, hp);
    public double Atk => (double)Math.Max(1, atk);
    public double GoldDrop => (double)Math.Max(0, goldDrop);
    public double PoonDrop => (double)Math.Max(0, poonDrop);
    public float PoonChance => Mathf.Clamp01(poonChance);
}