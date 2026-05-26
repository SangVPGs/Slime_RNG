using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PartySystem : MonoBehaviour
{
    private const string SaveKey = "Party_Data";
    private const string AutoEquipKey = "Party_AutoEquip";

    public event Action OnPartyChanged;

    [Header("Database")]
    [SerializeField] private PetDatabase petDatabase;

    [Header("Inventory")]
    [SerializeField] private InventorySystem inventorySystem;

    [Header("Data")]
    [SerializeField] private PartyData data = new();

    private bool autoEquip;

    public PartyData Data => data;
    public bool AutoEquip => autoEquip;

    private void Awake()
    {
        data.SetDatabase(petDatabase);
        Load();

        autoEquip = PlayerPrefs.GetInt(AutoEquipKey, 0) == 1;
    }

    private void Start()
    {
        if (autoEquip)
            AutoEquipFromInventory();
    }

    public bool AddPet(PetUnitData petData)
    {
        bool success = data.AddPet(petData);

        if (!success)
            return false;

        Save();
        OnPartyChanged?.Invoke();

        return true;
    }

    public bool RemovePet(PetUnitData petData)
    {
        bool success = data.RemovePet(petData);

        if (!success)
            return false;

        Save();
        OnPartyChanged?.Invoke();

        return true;
    }

    public void ClearParty()
    {
        data.ClearParty();

        Save();
        OnPartyChanged?.Invoke();
    }

    public void ToggleAutoEquip()
    {
        autoEquip = !autoEquip;

        Debug.Log($"Auto Equip toggled: {autoEquip}");

        PlayerPrefs.SetInt(AutoEquipKey, autoEquip ? 1 : 0);
        PlayerPrefs.Save();

        if (autoEquip)
            AutoEquipFromInventory();
        else
            OnPartyChanged.Invoke();
    }

    public void AutoEquipFromInventory()
    {
        if (inventorySystem == null)
            return;

        if (inventorySystem.Data == null)
            return;

        List<PetUnitData> bestPets = inventorySystem.Data.Pets
            .Where(entry => entry != null && entry.petData != null)
            .Select(entry => entry.petData)
            .OrderByDescending(pet => pet.combatPower)
            .Take(data.MaxPartySize)
            .ToList();

        data.ClearParty();

        foreach (PetUnitData pet in bestPets)
        {
            bool added = data.AddPet(pet);
        }

        inventorySystem.SetAllPetsOutParty();

        foreach (PetUnitData pet in bestPets)
        {
            bool setInParty = inventorySystem.SetPetInParty(pet, true);
        }

        Save();
        OnPartyChanged?.Invoke();
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
    public class PartyData
    {
        [SerializeField] private int maxPartySize = 4;
        [SerializeField] private List<string> petIds = new();

        [NonSerialized] private PetDatabase petDatabase;

        public int MaxPartySize => maxPartySize;
        public IReadOnlyList<string> PetIds => petIds;
        public bool IsFull => petIds.Count >= maxPartySize;

        public IReadOnlyList<PetUnitData> Pets
        {
            get
            {
                List<PetUnitData> result = new();

                if (petDatabase == null)
                    return result;

                foreach (string petId in petIds)
                {
                    PetUnitData pet = petDatabase.GetPetById(petId);

                    if (pet != null)
                        result.Add(pet);
                }

                return result;
            }
        }

        public void SetDatabase(PetDatabase database)
        {
            petDatabase = database;
        }

        public bool AddPet(PetUnitData petData)
        {
            if (petData == null)
                return false;

            if (string.IsNullOrEmpty(petData.Id))
                return false;

            if (IsFull)
                return false;

            if (petIds.Contains(petData.Id))
                return false;

            petIds.Add(petData.Id);
            return true;
        }

        public bool RemovePet(PetUnitData petData)
        {
            if (petData == null || string.IsNullOrEmpty(petData.Id))
                return false;

            return petIds.Remove(petData.Id);
        }

        public void ClearParty()
        {
            petIds.Clear();
        }
    }
}