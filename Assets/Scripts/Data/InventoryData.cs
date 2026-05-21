using System;
using System.Collections.Generic;
using UnityEngine;

// Sai kiến trúc - sửa sau

[CreateAssetMenu(menuName = "Game/Database/Inventory Data")]
public class InventoryData : ScriptableObject
{
    [SerializeField] private List<PetInventoryEntry> pets = new();

    public IReadOnlyList<PetInventoryEntry> Pets => pets;

    public void AddPet(PetUnitData pet, int amount = 1)
    {
        if (pet == null)
            return;

        foreach (PetInventoryEntry entry in pets)
        {
            if (entry.petData == pet)
            {
                entry.amount += amount;
                return;
            }
        }

        pets.Add(new PetInventoryEntry
        {
            petData = pet,
            amount = amount
        });
    }

    public bool RemovePet(PetUnitData pet, int amount = 1)
    {
        if (pet == null)
            return false;

        foreach (PetInventoryEntry entry in pets)
        {
            if (entry.petData != pet)
                continue;

            if (entry.amount < amount)
                return false;

            entry.amount -= amount;

            if (entry.amount <= 0)
            {
                pets.Remove(entry);
            }

            return true;
        }

        return false;
    }

    public int GetAmount(PetUnitData pet)
    {
        if (pet == null)
            return 0;

        foreach (PetInventoryEntry entry in pets)
        {
            if (entry.petData == pet)
            {
                return entry.amount;
            }
        }

        return 0;
    }

    public bool HasPet(PetUnitData pet)
    {
        return GetAmount(pet) > 0;
    }
}

[Serializable]
public class PetInventoryEntry
{
    public PetUnitData petData;
    public int amount;
}