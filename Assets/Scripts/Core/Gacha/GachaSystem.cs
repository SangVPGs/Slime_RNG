using System.Collections.Generic;
using UnityEngine;

public class GachaSystem : MonoBehaviour
{
    [Header("Database")]
    [SerializeField] private PetDatabase petDatabase;

    [Header("Real Roll Rarity Weights")]
    [SerializeField] private List<RarityWeightConfig> rarityWeights = new();

    [Header("Display Roll Rarity Weights")]
    [SerializeField] private List<RarityWeightConfig> displayRarityWeights = new();

    public IReadOnlyList<PetUnitData> Pets => petDatabase != null ? petDatabase.Pets : null;

    public PetUnitData RollPet()
    {
        if (!HasValidDatabase())
            return null;

        PetRarity rarity = RollRarityFromWeights(rarityWeights);
        return RollPetByRarity(rarity);
    }

    public PetUnitData GetRandomDisplayPet()
    {
        if (!HasValidDatabase())
            return null;

        PetRarity rarity = RollRarityFromWeights(displayRarityWeights);

        PetUnitData pet = RollPetByRarity(rarity);

        if (pet != null)
            return pet;

        return GetRandomPetFromDatabase();
    }

    private PetRarity RollRarityFromWeights(List<RarityWeightConfig> weights)
    {
        if (weights == null || weights.Count == 0)
        {
            Debug.LogWarning("Rarity weight config is empty.");
            return default;
        }

        float totalWeight = 0f;

        foreach (RarityWeightConfig config in weights)
        {
            if (config == null)
                continue;

            totalWeight += config.FinalWeight;
        }

        if (totalWeight <= 0f)
        {
            Debug.LogError("Total rarity weight must be greater than 0.");
            return default;
        }

        float value = Random.Range(0f, totalWeight);

        foreach (RarityWeightConfig config in weights)
        {
            if (config == null)
                continue;

            float weight = config.FinalWeight;

            if (weight <= 0f)
                continue;

            if (value < weight)
                return config.rarity;

            value -= weight;
        }

        return weights[weights.Count - 1].rarity;
    }

    private PetUnitData RollPetByRarity(PetRarity rarity)
    {
        List<PetUnitData> candidates = GetPetsByRarity(rarity);

        if (candidates.Count == 0)
        {
            Debug.LogWarning($"No pets found for rarity: {rarity}");
            return null;
        }

        return candidates[
            Random.Range(0, candidates.Count)
        ];
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

    private PetUnitData GetRandomPetFromDatabase()
    {
        if (!HasValidDatabase())
            return null;

        return petDatabase.Pets[
            Random.Range(0, petDatabase.Pets.Count)
        ];
    }

    private bool HasValidDatabase()
    {
        if (petDatabase == null || petDatabase.Pets == null || petDatabase.Pets.Count == 0)
        {
            Debug.LogError("PetDatabase is empty.");
            return false;
        }

        return true;
    }
}