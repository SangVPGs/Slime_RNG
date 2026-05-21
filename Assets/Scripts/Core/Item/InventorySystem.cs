using System;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public event Action OnInventoryChanged;

    [SerializeField] private InventoryData inventoryData;

    public InventoryData Data => inventoryData;

    public void AddPet(PetUnitData pet, int amount = 1)
    {
        if (inventoryData == null)
        {
            Debug.LogError("Missing InventoryData.");
            return;
        }

        inventoryData.AddPet(pet, amount);

        OnInventoryChanged?.Invoke();
    }

    public int GetAmount(PetUnitData pet)
    {
        if (inventoryData == null)
            return 0;

        return inventoryData.GetAmount(pet);
    }
}