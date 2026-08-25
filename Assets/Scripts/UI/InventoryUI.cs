using System;
using UnityEngine;
using UnityEngine.UI;

public enum InventoryTab
{
    Pet,
    Item
}

public class InventoryUI : MonoBehaviour
{
    public event Action<InventoryTab> OnTabChanged;
    public event Action<InventorySystem.ItemInventoryEntry> OnSelectedItemChanged;

    [Header("Buttons")]
    [SerializeField] private Button petTabButton;
    [SerializeField] private Button itemTabButton;

    [Header("Pages")]
    [SerializeField] private GameObject petInventoryUI;
    [SerializeField] private GameObject itemInventoryUI;

    public InventoryTab CurrentTab { get; private set; }

    public InventorySystem.ItemInventoryEntry SelectedItem
    {
        get;
        private set;
    }

    private void Awake()
    {
        if (petTabButton != null)
        {
            petTabButton.onClick.RemoveListener(ShowPetTab);
            petTabButton.onClick.AddListener(ShowPetTab);
        }

        if (itemTabButton != null)
        {
            itemTabButton.onClick.RemoveListener(ShowItemTab);
            itemTabButton.onClick.AddListener(ShowItemTab);
        }
    }

    private void OnEnable()
    {
        ShowPetTab();
    }

    public void ShowPetTab()
    {
        CurrentTab = InventoryTab.Pet;

        SelectedItem = null;

        if (petInventoryUI != null)
            petInventoryUI.SetActive(true);

        if (itemInventoryUI != null)
            itemInventoryUI.SetActive(false);

        OnTabChanged?.Invoke(CurrentTab);
        OnSelectedItemChanged?.Invoke(null);
    }

    public void ShowItemTab()
    {
        CurrentTab = InventoryTab.Item;

        SelectedItem = null;

        if (petInventoryUI != null)
            petInventoryUI.SetActive(false);

        if (itemInventoryUI != null)
            itemInventoryUI.SetActive(true);

        OnTabChanged?.Invoke(CurrentTab);
        OnSelectedItemChanged?.Invoke(null);
    }

    public void SelectItem(
        InventorySystem.ItemInventoryEntry itemEntry)
    {
        SelectedItem = itemEntry;

        OnSelectedItemChanged?.Invoke(itemEntry);
    }

    public bool HasSelectedFood()
    {
        return SelectedItem != null &&
               SelectedItem.itemData != null &&
               SelectedItem.itemData.ItemType == ItemType.Food;
    }
}