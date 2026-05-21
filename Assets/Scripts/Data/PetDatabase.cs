using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Database/Pet Database")]
public class PetDatabase : ScriptableObject
{
    [SerializeField] private List<PetUnitData> pets = new();

    public IReadOnlyList<PetUnitData> Pets => pets;
}