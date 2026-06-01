using UnityEngine;

public class PartyUI : MonoBehaviour
{
    [Header("System")]
    [SerializeField] private PartySystem partySystem;
    [SerializeField] private InventorySystem inventorySystem;

    [Header("Inventory UI")]
    [SerializeField] private InventoryUI inventoryUI;

    [Header("UI")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private PetUIItem petItemPrefab;

    private void OnEnable()
    {
        if (partySystem != null)
            partySystem.OnPartyChanged += ShowParty;

        if (inventorySystem != null)
            inventorySystem.OnInventoryChanged += ShowParty;

        if (inventoryUI != null)
        {
            inventoryUI.OnTabChanged += OnInventoryTabChanged;
            inventoryUI.OnSelectedItemChanged += OnSelectedItemChanged;
        }

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

    private void Start()
    {
        ShowParty();
    }

    public void ShowParty()
    {
        if (partySystem == null || partySystem.Data == null)
            return;

        ClearOldItems();

        foreach (InventorySystem.PetInventoryEntry entry in partySystem.Data.Pets)
        {
            if (entry == null || entry.petData == null)
                continue;

            PetUIItem item = Instantiate(petItemPrefab, contentParent);

            SetupPetUIItem(item, entry);
        }
    }

    private void SetupPetUIItem(
        PetUIItem item,
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