using UnityEngine;

public class PetInventoryUI : MonoBehaviour
{
    [Header("System")]
    [SerializeField] private InventorySystem inventorySystem;
    [SerializeField] private PartySystem partySystem;

    [Header("UI")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private PetUIItem petItemPrefab;

    private void OnEnable()
    {
        if (inventorySystem != null)
            inventorySystem.OnInventoryChanged += ShowPets;

        ShowPets();
    }

    private void OnDisable()
    {
        if (inventorySystem != null)
            inventorySystem.OnInventoryChanged -= ShowPets;
    }

    public void ShowPets()
    {
        if (inventorySystem == null)
        {
            Debug.LogError("InventorySystem missing.");
            return;
        }

        if (inventorySystem.Data == null)
        {
            Debug.LogError("InventoryData missing.");
            return;
        }

        if (contentParent == null || petItemPrefab == null)
        {
            Debug.LogError("PetInventoryUI UI reference missing.");
            return;
        }

        ClearOldItems();

        foreach (PetInventoryEntry entry in inventorySystem.Data.Pets)
        {
            if (entry == null || entry.petData == null)
                continue;

            PetUIItem item = Instantiate(petItemPrefab, contentParent);

            item.SetupInventory(entry.petData, OnPetClicked);
        }
    }

    private void OnPetClicked(PetUnitData petData)
    {
        Debug.Log($"Inventory clicked pet: {petData.unitName}");

        if (partySystem == null)
        {
            Debug.LogError("PartySystem missing in PetInventoryUI.");
            return;
        }

        bool success = partySystem.AddPet(petData);

        Debug.Log($"Add pet to party result: {success}");
    }

    private void ClearOldItems()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }
}