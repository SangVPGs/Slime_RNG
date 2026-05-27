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

    [Header("Party")]
    [SerializeField] private PartySystem partySystem;

    [Header("Data")]
    [SerializeField] private InventoryData data = new();

    public InventoryData Data => data;

    private void Awake()
    {
        Load();
        data.ResolvePetData(petDatabase);
    }

    public bool AddPet(PetUnitData petData)
    {
        if (petData == null)
            return false;

        bool success = data.AddPet(petData);

        if (!success)
            return false;

        Save();

        if (partySystem != null && partySystem.AutoEquip)
            partySystem.AutoEquipFromInventory();
        else
            OnInventoryChanged?.Invoke();

        return true;
    }

    public bool SetPetInParty(InventorySystem.PetInventoryEntry entry, bool isInParty)
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

    public bool SetPetInPartyWithoutNotify(InventorySystem.PetInventoryEntry entry, bool isInParty)
    {
        if (entry == null)
            return false;

        return data.SetPetInParty(entry, isInParty);
    }

    public bool SetPetLevel(InventorySystem.PetInventoryEntry entry, int level)
    {
        if (entry == null || entry.petData == null)
            return false;

        int newLevel = Mathf.Clamp(level, 1, entry.petData.maxLevel);

        if (entry.level == newLevel)
            return false;

        entry.level = newLevel;

        Save();
        OnInventoryChanged?.Invoke();

        return true;
    }

    public bool IsPetInParty(InventorySystem.PetInventoryEntry entry)
    {
        return data.IsPetInParty(entry);
    }

    public void SetAllPetsOutParty()
    {
        data.SetAllPetsOutParty();

        Save();
        OnInventoryChanged?.Invoke();
    }

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

        public IReadOnlyList<PetInventoryEntry> Pets => pets;

        public void ResolvePetData(PetDatabase database)
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

                if (entry.petData != null)
                    entry.level = Mathf.Clamp(entry.level, 1, entry.petData.maxLevel);
            }
        }

        public bool AddPet(PetUnitData petData)
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
                level = petData.defaultLevel,
                petData = petData
            });

            return true;
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

        public void Clear()
        {
            pets.Clear();
        }
    }

    [Serializable]
    public class PetInventoryEntry
    {
        public string petId;
        public bool isInParty;
        public int level = 1;

        [NonSerialized] public PetUnitData petData;
    }
}