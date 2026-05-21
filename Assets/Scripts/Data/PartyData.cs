using System.Collections.Generic;
using UnityEngine;

// Sai kiến trúc - sửa sau

[CreateAssetMenu(menuName = "Game/Party")]
public class PartyData : ScriptableObject
{
    [SerializeField] private int maxPartySize = 4;
    [SerializeField] private List<PetUnitData> pets = new();

    public int MaxPartySize => maxPartySize;
    public IReadOnlyList<PetUnitData> Pets => pets;
    public bool IsFull => pets.Count >= maxPartySize;

    public bool AddPet(PetUnitData petData)
    {
        if (petData == null)
            return false;

        if (IsFull)
            return false;

        if (pets.Contains(petData))
            return false;

        pets.Add(petData);
        return true;
    }

    public bool RemovePet(PetUnitData petData)
    {
        return pets.Remove(petData);
    }

    public void ClearParty()
    {
        pets.Clear();
    }
}