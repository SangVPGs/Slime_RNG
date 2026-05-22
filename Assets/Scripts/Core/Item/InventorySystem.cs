using System;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    private const string SaveKey = "Inventory_Data";

    public event Action OnInventoryChanged;

    [Header("Database")]
    [SerializeField] private PetDatabase petDatabase;

    [Header("Data")]
    [SerializeField] private InventoryData data = new();

    public InventoryData Data => data;

    private void Awake()
    {
        data.SetDatabase(petDatabase);
        Load();
    }

    public void AddPet(PetUnitData pet, int amount = 1)
    {
        if (pet == null)
            return;

        bool success = data.AddPet(pet, amount);

        if (!success)
            return;

        Save();

        OnInventoryChanged?.Invoke();
    }

    public bool RemovePet(PetUnitData pet, int amount = 1)
    {
        if (pet == null)
            return false;

        bool success = data.RemovePet(pet, amount);

        if (!success)
            return false;

        Save();

        OnInventoryChanged?.Invoke();

        return true;
    }

    public int GetAmount(PetUnitData pet)
    {
        return data.GetAmount(pet);
    }

    public bool HasPet(PetUnitData pet)
    {
        return data.HasPet(pet);
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
                        amount = entry.amount
                    });
                }

                return result;
            }
        }

        public void SetDatabase(PetDatabase database)
        {
            petDatabase = database;
        }

        public bool AddPet(PetUnitData pet, int amount = 1)
        {
            if (pet == null)
                return false;

            if (string.IsNullOrEmpty(pet.Id))
            {
                Debug.LogError($"{pet.name} missing petId.");
                return false;
            }

            if (amount <= 0)
                return false;

            foreach (PetInventoryEntry entry in pets)
            {
                if (entry.petId == pet.Id)
                {
                    entry.amount += amount;
                    return true;
                }
            }

            pets.Add(new PetInventoryEntry
            {
                petId = pet.Id,
                amount = amount
            });

            return true;
        }

        public bool RemovePet(PetUnitData pet, int amount = 1)
        {
            if (pet == null)
                return false;

            if (string.IsNullOrEmpty(pet.Id))
                return false;

            if (amount <= 0)
                return false;

            for (int i = 0; i < pets.Count; i++)
            {
                PetInventoryEntry entry = pets[i];

                if (entry.petId != pet.Id)
                    continue;

                if (entry.amount < amount)
                    return false;

                entry.amount -= amount;

                if (entry.amount <= 0)
                    pets.RemoveAt(i);

                return true;
            }

            return false;
        }

        public int GetAmount(PetUnitData pet)
        {
            if (pet == null || string.IsNullOrEmpty(pet.Id))
                return 0;

            foreach (PetInventoryEntry entry in pets)
            {
                if (entry.petId == pet.Id)
                    return entry.amount;
            }

            return 0;
        }

        public bool HasPet(PetUnitData pet)
        {
            return GetAmount(pet) > 0;
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

        [NonSerialized] public PetUnitData petData;

        public int amount;
    }
}