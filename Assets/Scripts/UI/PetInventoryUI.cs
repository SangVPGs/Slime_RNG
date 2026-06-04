using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PetInventoryUI : MonoBehaviour
{
    private InventorySystem inventorySystem;
    private PartySystem partySystem;

    [Header("UI")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private PetInventoryUIItem petItemPrefab;

    [Header("UI Text")]
    [SerializeField] private TMP_Text sortTypeText;
    [SerializeField] private TMP_Text sortDirectionText;
    [SerializeField] private TMP_Text autoEquipText;

    [Header("Auto Equip Btn")]
    [SerializeField] private Image autoEquipButtonImage;
    [SerializeField] private Color autoEquipOnColor = Color.green;
    [SerializeField] private Color autoEquipOffColor = Color.red;

    private bool descending = true;
    private bool sortByRarity = false;

    private List<InventorySystem.PetInventoryEntry> currentEntries = new();

    private void OnEnable()
    {
        ResolveSystems();

        if (inventorySystem != null)
            inventorySystem.OnInventoryChanged += Refresh;

        if (partySystem != null)
            partySystem.OnPartyChanged += Refresh;
    }

    private void OnDisable()
    {
        if (inventorySystem != null)
            inventorySystem.OnInventoryChanged -= Refresh;

        if (partySystem != null)
            partySystem.OnPartyChanged -= Refresh;
    }

    private void Start()
    {
        Refresh();
    }

    private void ResolveSystems()
    {
        if (partySystem == null)
        {
            partySystem = PartySystem.Instance;

            if (partySystem == null)
                partySystem = FindFirstObjectByType<PartySystem>();
        }

        if (inventorySystem == null)
        {
            inventorySystem = InventorySystem.Instance;

            if (inventorySystem == null)
                inventorySystem = FindFirstObjectByType<InventorySystem>();
        }
    }

    public void ToggleSortDirection()
    {
        descending = !descending;
        Refresh();
    }

    public void ToggleSortType()
    {
        sortByRarity = !sortByRarity;
        Refresh();
    }

    public void ToggleAutoEquip()
    {
        if (partySystem == null)
            return;

        partySystem.ToggleAutoEquip();
        Refresh();
    }

    private void Refresh()
    {
        UpdateAutoEquipBtnUI();
        BuildEntries();
        ApplySort();
        RefreshUI();
    }

    private void UpdateAutoEquipBtnUI()
    {
        if (partySystem == null)
            return;

        bool autoEquip = partySystem.AutoEquip;

        if (autoEquipText != null)
            autoEquipText.text = autoEquip ? "ON" : "OFF";

        if (autoEquipButtonImage != null)
            autoEquipButtonImage.color = autoEquip
                ? autoEquipOnColor
                : autoEquipOffColor;
    }

    private void BuildEntries()
    {
        currentEntries.Clear();

        if (inventorySystem == null || inventorySystem.Data == null)
            return;

        currentEntries = inventorySystem.Data.Pets
            .Where(entry =>
                entry != null &&
                entry.petData != null &&
                !entry.isInParty)
            .ToList();
    }

    private void ApplySort()
    {
        if (sortByRarity)
        {
            currentEntries = descending
                ? currentEntries.OrderByDescending(entry => entry.petData.rarity).ToList()
                : currentEntries.OrderBy(entry => entry.petData.rarity).ToList();

            if (sortTypeText != null)
                sortTypeText.text = "Rarity";
        }
        else
        {
            currentEntries = descending
                ? currentEntries
                    .OrderByDescending(entry => PetUnit.CalculateCombatPower(entry.petData, entry.level))
                    .ToList()
                : currentEntries
                    .OrderBy(entry => PetUnit.CalculateCombatPower(entry.petData, entry.level))
                    .ToList();

            if (sortTypeText != null)
                sortTypeText.text = "CP";
        }

        if (sortDirectionText != null)
            sortDirectionText.text = descending ? "DESC" : "ASC";
    }

    private void RefreshUI()
    {
        if (contentParent == null || petItemPrefab == null)
            return;

        ClearOldItems();

        bool canManualEquip = partySystem != null && !partySystem.AutoEquip;

        foreach (InventorySystem.PetInventoryEntry entry in currentEntries)
        {
            if (entry == null || entry.petData == null)
                continue;

            PetInventoryUIItem item = Instantiate(petItemPrefab, contentParent);

            item.SetupPetInventory(
                entry,
                OnPetClicked,
                canManualEquip
            );
        }
    }

    private void OnPetClicked(InventorySystem.PetInventoryEntry entry)
    {
        if (entry == null || partySystem == null)
            return;

        if (partySystem.AutoEquip)
            return;

        partySystem.AddPet(entry);
    }

    private void ClearOldItems()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }
}