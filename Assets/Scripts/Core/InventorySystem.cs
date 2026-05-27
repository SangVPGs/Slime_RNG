using System;
using System.Collections.Generic;
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
        data.SetDatabase(petDatabase);
        Load();
    }

    public bool AddPet(PetUnitData pet)
    {
        if (pet == null)
            return false;

        bool success = data.AddPet(pet);

        if (!success)
            return false;

        Save();

        if (partySystem != null && partySystem.AutoEquip)
        {
            partySystem.AutoEquipFromInventory();
        }
        else
        {
            OnInventoryChanged?.Invoke();
        }

        return true;
    }

    public bool SetPetInParty(PetUnitData pet, bool isInParty)
    {
        if (pet == null)
            return false;

        bool success = data.SetPetInParty(pet, isInParty);

        if (!success)
            return false;

        Save();
        OnInventoryChanged?.Invoke();

        return true;
    }

    public bool SetPetInPartyWithoutNotify(PetUnitData pet, bool isInParty)
    {
        if (pet == null)
            return false;

        return data.SetPetInParty(pet, isInParty);
    }

    public void SaveAndNotify()
    {
        Save();
        OnInventoryChanged?.Invoke();
    }

    public bool IsPetInParty(PetUnitData pet)
    {
        return data.IsPetInParty(pet);
    }

    public void SetAllPetsOutParty()
    {
        data.SetAllPetsOutParty();

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

        data.SetDatabase(petDatabase);
    }

    [Serializable]
    public class InventoryData
    {
        [SerializeField] private List<PetInventoryEntry> pets = new();

        [NonSerialized] private PetDatabase petDatabase;

        public IReadOnlyList<PetInventoryEntry> Pets
        {
            get
            {
                List<PetInventoryEntry> result = new();

                if (petDatabase == null)
                    return result;

                foreach (PetInventoryEntry entry in pets)
                {
                    if (entry == null)
                        continue;

                    PetUnitData petData = petDatabase.GetPetById(entry.petId);

                    if (petData == null)
                        continue;

                    result.Add(new PetInventoryEntry
                    {
                        petId = entry.petId,
                        petData = petData,
                        isInParty = entry.isInParty
                    });
                }

                return result;
            }
        }

        public void SetDatabase(PetDatabase database)
        {
            petDatabase = database;
        }

        public bool AddPet(PetUnitData pet)
        {
            if (pet == null)
                return false;

            if (string.IsNullOrEmpty(pet.Id))
            {
                Debug.LogError($"{pet.name} missing pet Id.");
                return false;
            }

            foreach (PetInventoryEntry entry in pets)
            {
                if (entry.petId == pet.Id)
                    return false;
            }

            pets.Add(new PetInventoryEntry
            {
                petId = pet.Id,
                isInParty = false
            });

            return true;
        }

        public bool SetPetInParty(PetUnitData pet, bool isInParty)
        {
            if (pet == null || string.IsNullOrEmpty(pet.Id))
                return false;

            foreach (PetInventoryEntry entry in pets)
            {
                if (entry.petId != pet.Id)
                    continue;

                entry.isInParty = isInParty;
                return true;
            }

            return false;
        }

        public bool IsPetInParty(PetUnitData pet)
        {
            if (pet == null || string.IsNullOrEmpty(pet.Id))
                return false;

            foreach (PetInventoryEntry entry in pets)
            {
                if (entry.petId == pet.Id)
                    return entry.isInParty;
            }

            return false;
        }

        public void SetAllPetsOutParty()
        {
            foreach (PetInventoryEntry entry in pets)
            {
                if (entry == null)
                    continue;

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

        [NonSerialized] public PetUnitData petData;
    }
}