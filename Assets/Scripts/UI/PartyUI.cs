using TMPro;
using UnityEngine;

public class PartyUI : MonoBehaviour
{
    private PartySystem partySystem;
    private InventorySystem inventorySystem;

    [Header("Inventory UI")]
    [SerializeField] private InventoryUI inventoryUI;

    [Header("UI")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private PetInventoryUIItem petItemPrefab;
    [SerializeField] private TMP_Text partyCountText;

    private void OnEnable()
    {
        ResolveSystems();

        if (partySystem != null)
            partySystem.OnPartyChanged += ShowParty;

        if (inventorySystem != null)
            inventorySystem.OnInventoryChanged += ShowParty;

        if (inventoryUI != null)
        {
            inventoryUI.OnTabChanged += OnInventoryTabChanged;
            inventoryUI.OnSelectedItemChanged += OnSelectedItemChanged;
        }

        if (partySystem != null)
            partySystem.RebuildPartyEntries();

        ShowParty();
    }

    private void OnDisable()
    {
        if (partySystem != null)
            partySystem.OnPartyChanged -= ShowParty;

        if (inventorySystem != null)
            inventorySystem.OnInventoryChanged -= ShowParty;

        if (inventoryUI != null)
        {
            inventoryUI.OnTabChanged -= OnInventoryTabChanged;
            inventoryUI.OnSelectedItemChanged -= OnSelectedItemChanged;
        }
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

    public void ShowParty()
    {
        ResolveSystems();

        if (partySystem == null || partySystem.Data == null)
            return;

        UpdatePartyCountText();

        ClearOldItems();

        foreach (InventorySystem.PetInventoryEntry entry in partySystem.Data.Pets)
        {
            if (entry == null || entry.petData == null)
                continue;

            PetInventoryUIItem item = Instantiate(petItemPrefab, contentParent);

            SetupPetUIItem(item, entry);
        }
    }

    private void SetupPetUIItem(
        PetInventoryUIItem item,
        InventorySystem.PetInventoryEntry entry)
    {
        if (item == null || entry == null)
            return;

        if (inventoryUI == null || inventoryUI.CurrentTab == InventoryTab.Pet)
        {
            item.SetupParty(
                entry,
                OnPartyPetClicked,
                partySystem != null && !partySystem.AutoEquip
            );

            return;
        }

        if (inventoryUI.CurrentTab == InventoryTab.Item)
        {
            if (inventoryUI.HasSelectedFood())
            {
                item.SetupUseItem(
                    entry,
                    OnUseFoodClicked,
                    true
                );
            }
            else
            {
                item.SetupParty(
                    entry,
                    null,
                    false
                );
            }
        }
    }

    private void UpdatePartyCountText()
    {
        if (partyCountText == null ||
            partySystem == null ||
            partySystem.Data == null)
        {
            return;
        }

        int currentCount = partySystem.Data.Pets.Count;
        int maxCount = partySystem.Data.MaxPartySize;

        partyCountText.text = $"{currentCount}/{maxCount}";
    }

    private void OnPartyPetClicked(InventorySystem.PetInventoryEntry entry)
    {
        if (entry == null || partySystem == null)
            return;

        if (partySystem.AutoEquip)
            return;

        partySystem.RemovePet(entry);
    }

    private void OnUseFoodClicked(InventorySystem.PetInventoryEntry petEntry)
    {
        if (inventorySystem == null || inventoryUI == null)
            return;

        InventorySystem.ItemInventoryEntry selectedItem = inventoryUI.SelectedItem;

        if (selectedItem == null || selectedItem.itemData == null)
            return;

        if (selectedItem.itemData.ItemType != ItemType.Food)
            return;

        inventorySystem.UseItem(selectedItem, petEntry);
    }

    private void OnInventoryTabChanged(InventoryTab tab)
    {
        ShowParty();
    }

    private void OnSelectedItemChanged(InventorySystem.ItemInventoryEntry itemEntry)
    {
        ShowParty();
    }

    private void ClearOldItems()
    {
        if (contentParent == null)
            return;

        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);
    }
}