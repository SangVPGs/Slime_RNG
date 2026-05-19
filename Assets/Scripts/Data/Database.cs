using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Database/Pet Database")]
public class Database : ScriptableObject
{
    [SerializeField] private List<PetUnitData> pets = new();
    [SerializeField] private List<SlimeUnitData> slimes = new();

    public IReadOnlyList<PetUnitData> Pets => pets;
    public IReadOnlyList<SlimeUnitData> Slimes => slimes;
}