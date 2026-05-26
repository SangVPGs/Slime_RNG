using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PetInventoryUI : MonoBehaviour
{
    [Header("System")]
    [SerializeField] private InventorySystem inventorySystem;
    [SerializeField] private PartySystem partySystem;

    [Header("UI")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private PetUIItem petItemPrefab;

    [Header("UI Text")]
    [SerializeField] private TMP_Text sortTypeText;
    [SerializeField] private TMP_Text sortDirectionText;
    [SerializeField] private TMP_Text autoEquipText;

    [Header("Auto Equip Btn")]
    [SerializeField] private Image autoEquipButtonImage;
    [SerializeField] private Color autoEquipOnColor = new Color(3,209,3);
    [SerializeField] private Color autoEquipOffColor = new Color(209, 19, 3);

    private bool descending = true;
    private bool sortByRarity = false;
    private List<InventorySystem.PetInventoryEntry> currentEntries = new();

    private void OnEnable()
    {
        if (inventorySystem != null)
            inventorySystem.OnInventoryChanged += ShowPets;
    }

    private void OnDisable()
    {
        if (inventorySystem != null)
            inventorySystem.OnInventoryChanged -= ShowPets;
    }

    private void Start()
    {
        UpdateAutoEquipBtnUI();
        ShowPets();
    }

    public void ToggleSortDirection()
    {
        descending = !descending;
        ApplySort();
    }

    public void ToggleSortType()
    {
        sortByRarity = !sortByRarity;
        ApplySort();
    }

    public void ToggleAutoEquip()
    {
        if (partySystem == null)
            return;

        partySystem.ToggleAutoEquip();

        UpdateAutoEquipBtnUI();
        ShowPets();
    }

    private void UpdateAutoEquipBtnUI()
    {
        if (partySystem == null)
            return;

        bool autoEquip = partySystem.AutoEquip;

        if (autoEquipText != null)
        {
            autoEquipText.text = autoEquip ? "ON" : "OFF";
        }

        if (autoEquipButtonImage != null)
        {
            autoEquipButtonImage.color = autoEquip
                ? autoEquipOnColor
                : autoEquipOffColor;
        }
    }

    public void ShowPets()
    {
        if (inventorySystem == null || inventorySystem.Data == null)
            return;

        currentEntries = inventorySystem.Data.Pets
            .Where(entry =>
                entry != null &&
                entry.petData != null &&
                !entry.isInParty)
            .ToList();

        ApplySort();
    }

    private void ApplySort()
    {
        if (sortByRarity)
        {
            currentEntries = descending
                ? currentEntries
                    .OrderByDescending(entry => entry.petData.rarity)
                    .ToList()
                : currentEntries
                    .OrderBy(entry => entry.petData.rarity)
                    .ToList();

            if (sortTypeText != null)
                sortTypeText.text = "Rarity";
        }
        else
        {
            currentEntries = descending
                ? currentEntries
                    .OrderByDescending(entry => entry.petData.combatPower)
                    .ToList()
                : currentEntries
                    .OrderBy(entry => entry.petData.combatPower)
                    .ToList();

            if (sortTypeText != null)
                sortTypeText.text = "CP";
        }

        if (sortDirectionText != null)
            sortDirectionText.text = descending ? "DESC" : "ASC";

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (contentParent == null || petItemPrefab == null)
            return;

        ClearOldItems();

        foreach (InventorySystem.PetInventoryEntry entry in currentEntries)
        {
            if (entry == null || entry.petData == null)
                continue;

            PetUIItem item = Instantiate(petItemPrefab, contentParent);
            item.SetupInventory(entry.petData, OnPetClicked, !partySystem.AutoEquip);
        }
    }

    private void OnPetClicked(PetUnitData petData)
    {
        if (petData == null || partySystem == null || inventorySystem == null)
            return;

        bool addedToParty = partySystem.AddPet(petData);

        if (!addedToParty)
            return;

        inventorySystem.SetPetInParty(petData, true);
    }

    private void ClearOldItems()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }
}