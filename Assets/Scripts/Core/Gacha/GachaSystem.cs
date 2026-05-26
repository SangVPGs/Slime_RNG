using System.Collections.Generic;
using UnityEngine;

public class GachaSystem : MonoBehaviour
{
    [Header("Database")]
    [SerializeField] private PetDatabase petDatabase;

    [Header("Rates")]
    [SerializeField] private float commonRate = 70f;
    [SerializeField] private float uncommonRate = 20f;
    [SerializeField] private float rareRate = 7f;
    [SerializeField] private float epicRate = 2f;
    [SerializeField] private float legendaryRate = 1f;

    public IReadOnlyList<PetUnitData> Pets => petDatabase.Pets;

    public PetUnitData RollPet()
    {
        if (petDatabase == null || petDatabase.Pets.Count == 0)
        {
            Debug.LogError("PetDatabase is empty.");
            return null;
        }

        PetRarity rarity = RollRarity();
        List<PetUnitData> candidates = GetPetsByRarity(rarity);

        if (candidates.Count == 0)
        {
            Debug.LogWarning($"No pets found for rarity: {rarity}");
            return null;
        }

        return candidates[Random.Range(0, candidates.Count)];
    }

    public PetUnitData GetRandomDisplayPet()
    {
        if (petDatabase == null || petDatabase.Pets.Count == 0)
            return null;

        return petDatabase.Pets[Random.Range(0, petDatabase.Pets.Count)];
    }

    private PetRarity RollRarity()
    {
        float totalRate = commonRate + uncommonRate + rareRate + epicRate + legendaryRate;
        float value = Random.Range(0f, totalRate);

        if (value <= commonRate)
            return PetRarity.Common;

        value -= commonRate;

        if (value <= uncommonRate)
            return PetRarity.Uncommon;

        value -= uncommonRate;

        if (value <= rareRate)
            return PetRarity.Rare;

        value -= rareRate;

        if (value <= epicRate)
            return PetRarity.Epic;

        return PetRarity.Legendary;
    }

    private List<PetUnitData> GetPetsByRarity(PetRarity rarity)
    {
        List<PetUnitData> result = new();

        foreach (PetUnitData pet in petDatabase.Pets)
        {
            if (pet != null && pet.rarity == rarity)
                result.Add(pet);
        }

        return result;
    }
}