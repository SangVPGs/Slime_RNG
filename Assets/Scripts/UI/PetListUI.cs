using System.Collections.Generic;
using UnityEngine;

public class PetListUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private PetDatabase database;

    [Header("UI")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private PetUIItem petItemPrefab;

    private List<PetUnitData> allPet = new();

    private void Start()
    {
        SortPet();
    }

    private void ShowPets()
    {
        ClearOldItems();

        foreach (PetUnitData pet in allPet)
        {
            PetUIItem item = Instantiate(petItemPrefab, contentParent);
            item.SetupIndex(pet);
        }
    }

    private void SortPet()
    {
        allPet = new List<PetUnitData>(database.Pets);

        allPet.Sort((a, b) =>
        {
            return a.rarity.CompareTo(b.rarity);
        });

        ShowPets();
    }

    private void ClearOldItems()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }
}