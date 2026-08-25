using System.Collections.Generic;
using UnityEngine;

public class PetListUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private PetDatabase database;
    private InventorySystem inventorySystem => InventorySystem.Instance;

    [Header("UI")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private PetListUIItem petItemPrefab;

    [Header("Detail UI")]
    [SerializeField] private PetDetailUI petDetailUI;

    private List<PetUnitData> allPets = new();

    private void OnEnable()
    {
        if (inventorySystem != null)
            inventorySystem.OnInventoryChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (inventorySystem != null)
            inventorySystem.OnInventoryChanged -= Refresh;
    }

    private void Refresh()
    {
        SortPets();
        ShowPets();
    }

    private void SortPets()
    {
        if (database == null)
            return;

        allPets = new List<PetUnitData>(database.Pets);

        allPets.Sort((a, b) =>
        {
            return a.rarity.CompareTo(b.rarity);
        });
    }

    private void ShowPets()
    {
        ClearOldItems();

        foreach (PetUnitData pet in allPets)
        {
            PetListUIItem item = Instantiate(petItemPrefab, contentParent);

            bool isOwned = inventorySystem != null && inventorySystem.HasPet(pet);

            item.Setup(pet, isOwned, ShowPetDetail);
        }
    }

    private void ShowPetDetail(PetUnitData petData, bool isOwned)
    {
        if (petDetailUI == null)
            return;

        petDetailUI.Show(petData, isOwned);
    }

    private void ClearOldItems()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }
}