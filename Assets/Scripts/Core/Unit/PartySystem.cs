using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PartySystem : MonoBehaviour
{
    private const string SaveKey = "Party_Data";
    private const string AutoEquipKey = "Party_AutoEquip";

    public event Action OnPartyChanged;

    private InventorySystem inventorySystem => InventorySystem.Instance;

    [Header("Data")]
    [SerializeField] private PartyData data = new();

    private bool autoEquip = true;

    public PartyData Data => data;
    public bool AutoEquip => autoEquip;

    public static PartySystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Load();

        autoEquip = PlayerPrefs.GetInt(AutoEquipKey, 1) == 1;
    }

    private void Start()
    {
        RebuildPartyEntries();

        if (autoEquip)
            AutoEquipFromInventory();
        else
            OnPartyChanged?.Invoke();
    }

    public bool AddPet(InventorySystem.PetInventoryEntry entry)
    {
        if (entry == null || entry.petData == null)
            return false;

        bool success = data.AddPet(entry);

        if (!success)
            return false;

        inventorySystem?.SetPetInPartyWithoutNotify(entry, true);

        Save();
        inventorySystem?.SaveAndNotify();
        OnPartyChanged?.Invoke();

        return true;
    }

    public bool RemovePet(InventorySystem.PetInventoryEntry entry)
    {
        if (entry == null || entry.petData == null)
            return false;

        bool success = data.RemovePet(entry);

        if (!success)
            return false;

        inventorySystem?.SetPetInPartyWithoutNotify(entry, false);

        Save();
        inventorySystem?.SaveAndNotify();
        OnPartyChanged?.Invoke();

        return true;
    }

    public void ClearParty()
    {
        foreach (InventorySystem.PetInventoryEntry entry in data.Pets)
        {
            if (entry != null)
                inventorySystem?.SetPetInPartyWithoutNotify(entry, false);
        }

        data.ClearParty();

        Save();
        inventorySystem?.SaveAndNotify();
        OnPartyChanged?.Invoke();
    }

    public void ToggleAutoEquip()
    {
        autoEquip = !autoEquip;

        PlayerPrefs.SetInt(AutoEquipKey, autoEquip ? 1 : 0);
        PlayerPrefs.Save();

        if (autoEquip)
            AutoEquipFromInventory();
        else
            OnPartyChanged?.Invoke();
    }

    public void AutoEquipFromInventory()
    {
        if (inventorySystem == null || inventorySystem.Data == null)
            return;

        RebuildPartyEntries();

        List<InventorySystem.PetInventoryEntry> availablePets = inventorySystem.Data.Pets
            .Where(entry =>
                entry != null &&
                entry.petData != null &&
                !entry.isInParty)
            .OrderByDescending(entry => PetUnit.CalculateCombatPower(entry.petData, entry.level))
            .ToList();

        foreach (InventorySystem.PetInventoryEntry entry in availablePets)
        {
            if (entry == null || entry.petData == null)
                continue;

            if (!data.IsFull)
            {
                if (!data.AddPet(entry))
                    continue;

                inventorySystem.SetPetInPartyWithoutNotify(entry, true);
                continue;
            }

            InventorySystem.PetInventoryEntry weakestPet = GetWeakestPartyPet();

            if (weakestPet == null)
                continue;

            long newPetPower = PetUnit.CalculateCombatPower(entry.petData, entry.level);
            long weakestPower = PetUnit.CalculateCombatPower(weakestPet.petData, weakestPet.level);

            if (newPetPower <= weakestPower)
                continue;

            if (!data.RemovePet(weakestPet))
                continue;

            inventorySystem.SetPetInPartyWithoutNotify(weakestPet, false);

            if (data.AddPet(entry))
                inventorySystem.SetPetInPartyWithoutNotify(entry, true);
        }

        Save();
        inventorySystem.SaveAndNotify();
        OnPartyChanged?.Invoke();
    }

    public void RebuildPartyEntries()
    {
        if (inventorySystem == null || inventorySystem.Data == null)
            return;

        data.ClearRuntimeOnly();

        inventorySystem.Data.SetAllPetsOutParty();

        foreach (string petId in data.PetIds)
        {
            InventorySystem.PetInventoryEntry entry =
                inventorySystem.Data.GetEntryByPetId(petId);

            if (entry == null || entry.petData == null)
            {
                Debug.LogWarning($"Party pet not found in inventory: {petId}");
                continue;
            }

            data.AddRuntimeEntry(entry);
            entry.isInParty = true;
        }

        inventorySystem.SaveAndNotify();
    }

    private InventorySystem.PetInventoryEntry GetWeakestPartyPet()
    {
        InventorySystem.PetInventoryEntry weakestPet = null;

        foreach (InventorySystem.PetInventoryEntry entry in data.Pets)
        {
            if (entry == null || entry.petData == null)
                continue;

            if (weakestPet == null || 
                PetUnit.CalculateCombatPower(entry.petData, entry.level) 
                < PetUnit.CalculateCombatPower(weakestPet.petData, weakestPet.level))
            {
                weakestPet = entry;
            }
        }

        return weakestPet;
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
        {
            data.ClearParty();
            return;
        }

        string json = PlayerPrefs.GetString(SaveKey);

        if (string.IsNullOrEmpty(json))
        {
            data.ClearParty();
            return;
        }

        JsonUtility.FromJsonOverwrite(json, data);
    }

    [Serializable]
    public class PartyData
    {
        [SerializeField] private int maxPartySize = 4;
        [SerializeField] private List<string> petIds = new();

        [NonSerialized] private List<InventorySystem.PetInventoryEntry> pets = new();

        public int MaxPartySize => maxPartySize;
        public IReadOnlyList<string> PetIds => petIds;
        public IReadOnlyList<InventorySystem.PetInventoryEntry> Pets => pets;

        public bool IsFull => petIds.Count >= maxPartySize;

        public bool AddPet(InventorySystem.PetInventoryEntry entry)
        {
            if (entry == null || entry.petData == null)
                return false;

            string petId = entry.petId;

            if (string.IsNullOrEmpty(petId))
                return false;

            if (IsFull)
                return false;

            if (petIds.Contains(petId))
                return false;

            petIds.Add(petId);

            AddRuntimeEntry(entry);

            return true;
        }

        public bool RemovePet(InventorySystem.PetInventoryEntry entry)
        {
            if (entry == null || string.IsNullOrEmpty(entry.petId))
                return false;

            pets.Remove(entry);
            return petIds.Remove(entry.petId);
        }

        public void AddRuntimeEntry(InventorySystem.PetInventoryEntry entry)
        {
            if (entry == null || entry.petData == null)
                return;

            if (!pets.Contains(entry))
                pets.Add(entry);
        }

        public void ClearRuntimeOnly()
        {
            pets.Clear();
        }

        public void ClearParty()
        {
            petIds.Clear();
            pets.Clear();
        }
    }
}