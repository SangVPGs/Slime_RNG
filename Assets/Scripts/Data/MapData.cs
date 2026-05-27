using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Map")]
public class MapData : ScriptableObject
{
    public int level;
    public GameObject mapPrefab;
    public int unlockCost = 100;

    public List<SlimeUnitData> enemies;
}