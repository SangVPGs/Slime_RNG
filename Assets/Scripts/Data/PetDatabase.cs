using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Database/Pet Database")]
public class PetDatabase : ScriptableObject
{
    [SerializeField] private List<PetUnitData> pets = new();

    public IReadOnlyList<PetUnitData> Pets => pets;

    public PetUnitData GetPetById(string petId)
    {
        if (string.IsNullOrEmpty(petId))
            return null;

        foreach (PetUnitData pet in pets)
        {
            if (pet != null && pet.Id == petId)
                return pet;
        }

        return null;
    }
}