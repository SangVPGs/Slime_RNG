using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    private const string SaveKey = "Inventory_Data";

    public event Action OnInventoryChanged;

    [Header("Database")]
    [SerializeField] private PetDatabase petDatabase;
    [SerializeField] private ItemDatabase itemDatabase;

    [Header("Party")]
    [SerializeField] private PartySystem partySystem;

    [Header("Pet Exp")]
    [SerializeField] private int defaultMaxExp = 100;
    [SerializeField] private float maxExpGrowthMultiplier = 1.25f;

    [Header("Data")]
    [SerializeField] private InventoryData data = new();

    public InventoryData Data => data;

    private void Awake()
    {
        Load();

        data.ResolvePetData(petDatabase, defaultMaxExp);
        data.ResolveItemData(itemDatabase);
    }

    #region Pet

    public bool AddPet(PetUnitData petData)
    {
        if (petData == null)
            return false;

        bool success = data.AddPet(petData, defaultMaxExp);

        if (!success)
            return false;

        Save();

        if (partySystem != null && partySystem.AutoEquip)
            partySystem.AutoEquipFromInventory();
        else
            OnInventoryChanged?.Invoke();

        return true;
    }

    public bool SetPetInParty(PetInventoryEntry entry, bool isInParty)
    {
        if (entry == null)
            return false;

        bool success = data.SetPetInParty(entry, isInParty);

        if (!success)
            return false;

        Save();
        OnInventoryChanged?.Invoke();

        return true;
    }

    public bool SetPetInPartyWithoutNotify(PetInventoryEntry entry, bool isInParty)
    {
        if (entry == null)
            return false;

        return data.SetPetInParty(entry, isInParty);
    }

    public bool SetPetLevel(PetInventoryEntry entry, int level)
    {
        if (entry == null || entry.petData == null)
            return false;

        int newLevel = Mathf.Clamp(level, 1, entry.petData.maxLevel);

        if (entry.level == newLevel)
            return false;

        entry.level = newLevel;
        entry.exp = 0;

        Save();
        OnInventoryChanged?.Invoke();

        return true;
    }

    public bool AddPetExp(PetInventoryEntry entry, int expAmount)
    {
        if (entry == null || entry.petData == null)
            return false;

        if (expAmount <= 0)
            return false;

        if (entry.level >= entry.petData.maxLevel)
            return false;

        data.AddPetExp(
            entry,
            expAmount,
            maxExpGrowthMultiplier
        );

        Save();
        OnInventoryChanged?.Invoke();

        return true;
    }

    public bool IsPetInParty(PetInventoryEntry entry)
    {
        return data.IsPetInParty(entry);
    }

    public void SetAllPetsOutParty()
    {
        data.SetAllPetsOutParty();

        Save();
        OnInventoryChanged?.Invoke();
    }

    #endregion

    #region Item

    public bool AddItem(ItemData itemData, int amount = 1)
    {
        if (itemData == null || amount <= 0)
            return false;

        bool success = data.AddItem(itemData, amount);

        if (!success)
            return false;

        Save();
        OnInventoryChanged?.Invoke();

        return true;
    }

    public bool RemoveItem(ItemData itemData, int amount = 1)
    {
        if (itemData == null || amount <= 0)
            return false;

        bool success = data.RemoveItem(itemData.Id, amount);

        if (!success)
            return false;

        Save();
        OnInventoryChanged?.Invoke();

        return true;
    }

    public bool UseItem(ItemInventoryEntry itemEntry, PetInventoryEntry petEntry)
    {
        if (itemEntry == null || itemEntry.itemData == null)
            return false;

        if (itemEntry.amount <= 0)
            return false;

        ItemData item = itemEntry.itemData;

        switch (item.ItemType)
        {
            case ItemType.Food:
                {
                    if (petEntry == null || petEntry.petData == null)
                        return false;

                    int expAmount = Mathf.RoundToInt(item.Value);

                    if (expAmount <= 0)
                        return false;

                    if (petEntry.level >= petEntry.petData.maxLevel)
                        return false;

                    data.AddPetExp(
                        petEntry,
                        expAmount,
                        maxExpGrowthMultiplier
                    );

                    bool removed = data.RemoveItem(item.Id, 1);

                    if (!removed)
                        return false;

                    Save();
                    OnInventoryChanged?.Invoke();

                    return true;
                }

            case ItemType.BuffStat:
                return false;

            default:
                return false;
        }
    }

    public bool HasItem(ItemData itemData, int amount = 1)
    {
        if (itemData == null)
            return false;

        return data.HasItem(itemData.Id, amount);
    }

    #endregion

    public void SaveAndNotify()
    {
        Save();
        OnInventoryChanged?.Invoke();
    }

    private void Save()
    {
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
            return;

        string json = PlayerPrefs.GetString(SaveKey);

        if (string.IsNullOrEmpty(json))
            return;

        JsonUtility.FromJsonOverwrite(json, data);
    }

    [Serializable]
    public class InventoryData
    {
        [SerializeField] private List<PetInventoryEntry> pets = new();
        [SerializeField] private List<ItemInventoryEntry> items = new();

        public IReadOnlyList<PetInventoryEntry> Pets => pets;
        public IReadOnlyList<ItemInventoryEntry> Items => items;

        #region Pet

        public void ResolvePetData(PetDatabase database, int defaultMaxExp)
        {
            if (database == null)
                return;

            foreach (PetInventoryEntry entry in pets)
            {
                if (entry == null)
                    continue;

                if (string.IsNullOrEmpty(entry.petId))
                    continue;

                entry.petData = database.GetPetById(entry.petId);

                if (entry.petData == null)
                    continue;

                entry.level = Mathf.Clamp(
                    entry.level,
                    1,
                    entry.petData.maxLevel
                );

                entry.exp = Mathf.Max(0, entry.exp);

                if (entry.maxExp <= 0)
                    entry.maxExp = Mathf.Max(1, defaultMaxExp);

                if (entry.level >= entry.petData.maxLevel)
                    entry.exp = 0;
            }
        }

        public bool AddPet(PetUnitData petData, int defaultMaxExp)
        {
            if (petData == null)
                return false;

            if (string.IsNullOrEmpty(petData.Id))
            {
                Debug.LogError($"{petData.name} missing pet Id.");
                return false;
            }

            bool alreadyOwned = pets.Any(entry =>
                entry != null &&
                entry.petId == petData.Id);

            if (alreadyOwned)
                return false;

            pets.Add(new PetInventoryEntry
            {
                petId = petData.Id,
                isInParty = false,
                level = Mathf.Clamp(petData.defaultLevel, 1, petData.maxLevel),
                exp = 0,
                maxExp = Mathf.Max(1, defaultMaxExp),
                petData = petData
            });

            return true;
        }

        public void AddPetExp(
            PetInventoryEntry entry,
            int expAmount,
            float maxExpGrowthMultiplier)
        {
            if (entry == null || entry.petData == null)
                return;

            if (expAmount <= 0)
                return;

            if (entry.level >= entry.petData.maxLevel)
            {
                entry.exp = 0;
                return;
            }

            entry.exp += expAmount;

            while (entry.exp >= entry.maxExp &&
                   entry.level < entry.petData.maxLevel)
            {
                entry.exp -= entry.maxExp;
                entry.level++;

                entry.maxExp = Mathf.RoundToInt(
                    entry.maxExp * maxExpGrowthMultiplier
                );

                if (entry.maxExp < 1)
                    entry.maxExp = 1;
            }

            if (entry.level >= entry.petData.maxLevel)
                entry.exp = 0;
        }

        public bool SetPetInParty(PetInventoryEntry entry, bool isInParty)
        {
            if (entry == null || string.IsNullOrEmpty(entry.petId))
                return false;

            PetInventoryEntry found = GetEntryByPetId(entry.petId);

            if (found == null)
                return false;

            found.isInParty = isInParty;
            return true;
        }

        public bool IsPetInParty(PetInventoryEntry entry)
        {
            if (entry == null || string.IsNullOrEmpty(entry.petId))
                return false;

            PetInventoryEntry found = GetEntryByPetId(entry.petId);

            return found != null && found.isInParty;
        }

        public PetInventoryEntry GetEntryByPetId(string petId)
        {
            if (string.IsNullOrEmpty(petId))
                return null;

            return pets.FirstOrDefault(entry =>
                entry != null &&
                entry.petId == petId);
        }

        public void SetAllPetsOutParty()
        {
            foreach (PetInventoryEntry entry in pets)
            {
                if (entry != null)
                    entry.isInParty = false;
            }
        }

        #endregion

        #region Item

        public void ResolveItemData(ItemDatabase database)
        {
            if (database == null)
                return;

            for (int i = items.Count - 1; i >= 0; i--)
            {
                ItemInventoryEntry entry = items[i];

                if (entry == null || string.IsNullOrEmpty(entry.itemId))
                {
                    items.RemoveAt(i);
                    continue;
                }

                entry.itemData = database.GetItemById(entry.itemId);

                if (entry.itemData == null)
                {
                    Debug.LogWarning($"Missing item data with id: {entry.itemId}");
                    continue;
                }

                entry.amount = Mathf.Clamp(
                    entry.amount,
                    1,
                    entry.itemData.MaxStack
                );
            }
        }

        public bool AddItem(ItemData itemData, int amount)
        {
            if (itemData == null || amount <= 0)
                return false;

            if (string.IsNullOrEmpty(itemData.Id))
            {
                Debug.LogError($"{itemData.name} missing item Id.");
                return false;
            }

            if (itemData.Stackable)
            {
                ItemInventoryEntry existing = GetEntryByItemId(itemData.Id);

                if (existing != null)
                {
                    existing.amount = Mathf.Clamp(
                        existing.amount + amount,
                        1,
                        itemData.MaxStack
                    );

                    existing.itemData = itemData;
                    return true;
                }
            }

            items.Add(new ItemInventoryEntry
            {
                itemId = itemData.Id,
                amount = Mathf.Clamp(amount, 1, itemData.MaxStack),
                itemData = itemData
            });

            return true;
        }

        public bool RemoveItem(string itemId, int amount)
        {
            if (string.IsNullOrEmpty(itemId) || amount <= 0)
                return false;

            ItemInventoryEntry entry = GetEntryByItemId(itemId);

            if (entry == null || entry.amount < amount)
                return false;

            entry.amount -= amount;

            if (entry.amount <= 0)
                items.Remove(entry);

            return true;
        }

        public bool HasItem(string itemId, int amount)
        {
            if (string.IsNullOrEmpty(itemId) || amount <= 0)
                return false;

            ItemInventoryEntry entry = GetEntryByItemId(itemId);

            return entry != null && entry.amount >= amount;
        }

        public ItemInventoryEntry GetEntryByItemId(string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
                return null;

            return items.FirstOrDefault(entry =>
                entry != null &&
                entry.itemId == itemId);
        }

        #endregion

        public void Clear()
        {
            pets.Clear();
            items.Clear();
        }
    }

    [Serializable]
    public class PetInventoryEntry
    {
        public string petId;
        public bool isInParty;
        public int level = 1;

        public int exp = 0;
        public int maxExp = 100;

        [NonSerialized] public PetUnitData petData;
    }

    [Serializable]
    public class ItemInventoryEntry
    {
        public string itemId;
        public int amount = 1;

        [NonSerialized] public ItemData itemData;
    }
}