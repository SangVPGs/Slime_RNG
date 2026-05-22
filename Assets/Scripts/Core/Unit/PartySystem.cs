using System;
using System.Collections.Generic;
using UnityEngine;

public class PartySystem : MonoBehaviour
{
    private const string SaveKey = "Party_Data";

    public event Action OnPartyChanged;

    [Header("Database")]
    [SerializeField] private PetDatabase petDatabase;

    [Header("Data")]
    [SerializeField] private PartyData data = new();

    public PartyData Data => data;

    private void Awake()
    {
        data.SetDatabase(petDatabase);
        Load();
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