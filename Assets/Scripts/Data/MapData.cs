using UnityEngine;

[CreateAssetMenu(menuName = "Game/Map Data")]
public class MapData : ScriptableObject
{
    public int level;
    public GameObject mapPrefab;
    public int unlockCost = 100;
}