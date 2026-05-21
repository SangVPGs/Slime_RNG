using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Database/Map Database")]
public class MapDatabase : ScriptableObject
{
    [SerializeField] private List<MapData> maps = new();

    public IReadOnlyList<MapData> Maps => maps;
}