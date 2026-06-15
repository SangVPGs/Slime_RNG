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

    [Header("Special Reward")]
    [SerializeField, Range(0f, 1f)] private float specialChance = 0.08f;
    [SerializeField] private List<GachaSpecialWeightConfig> specialWeights = new();

    [Header("Display Special Reward")]
    // [SerializeField, Range(0f, 1f)] private float displayBonusChance = 0.06f;
    [SerializeField, Range(0f, 1f)] private float displayCloverChance = 0.06f;

    private int cloverCount;

    public IReadOnlyList<PetUnitData> Pets => petDatabase != null ? petDatabase.Pets : null;

    public static GachaSystem Instance { get; private set; }

    public float CurrentLuck => GetCurrentLuck();
    public int CloverCount => cloverCount;
    public float CloverLuckMultiplier => GetCloverLuckMultiplier();
    public float CurrentFinalLuck => GetCurrentLuck() * GetCloverLuckMultiplier();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void ResetClover()
    {
        cloverCount = 0;
    }

    public GachaReward RollReward(int columnIndex, int maxColumns)
    {
        if (!HasValidDatabase())
            return null;

        if (columnIndex <= 0)
            ResetClover();

        bool isLastColumn = columnIndex >= maxColumns - 1;
        bool allowClover = !isLastColumn;

        if (Random.value < specialChance)
        {
            GachaReward specialReward = RollSpecialReward(allowClover);

            if (specialReward != null)
            {
                if (specialReward.IsClover)
                    AddClover();

                return specialReward;
            }
        }

        PetUnitData pet = RollPet(GetCloverLuckMultiplier());

        if (pet == null)
            return null;

        return GachaReward.Pet(pet);
    }

    public GachaReward GetRandomDisplayReward(int columnIndex, int maxColumns)
    {
        bool isLastColumn = columnIndex >= maxColumns - 1;
        bool allowClover = !isLastColumn;

        float value = Random.value;

        // Bonus tạm khóa.
        // if (value < displayBonusChance)
        //     return GachaReward.Bonus();

        if (allowClover && value < displayCloverChance)
            return GachaReward.Clover();

        return GachaReward.Pet(GetRandomDisplayPet());
    }

    public PetUnitData RollPet()
    {
        return RollPet(1f);
    }

    private PetUnitData RollPet(float luckMultiplier)
    {
        if (!HasValidDatabase())
            return null;

        PetRarity rarity = RollRarityFromWeights(rarityWeights, luckMultiplier);
        return RollPetByRarity(rarity);
    }

    public PetUnitData GetRandomDisplayPet()
    {
        if (!HasValidDatabase())
            return null;

        PetRarity rarity = RollRarityFromWeights(displayRarityWeights, 1f);
        PetUnitData pet = RollPetByRarity(rarity);

        if (pet != null)
            return pet;

        return GetRandomPetFromDatabase();
    }

    private GachaReward RollSpecialReward(bool allowClover)
    {
        if (specialWeights == null || specialWeights.Count == 0)
            return null;

        float totalWeight = 0f;

        foreach (GachaSpecialWeightConfig config in specialWeights)
        {
            if (!IsValidSpecialConfig(config, allowClover))
                continue;

            totalWeight += Mathf.Max(0f, config.weight);
        }

        if (totalWeight <= 0f)
            return null;

        float value = Random.Range(0f, totalWeight);

        foreach (GachaSpecialWeightConfig config in specialWeights)
        {
            if (!IsValidSpecialConfig(config, allowClover))
                continue;

            float weight = Mathf.Max(0f, config.weight);

            if (value < weight)
                return CreateSpecialReward(config.rewardType);

            value -= weight;
        }

        return null;
    }

    private bool IsValidSpecialConfig(GachaSpecialWeightConfig config, bool allowClover)
    {
        if (config == null)
            return false;

        if (config.rewardType == GachaRewardType.Pet)
            return false;

        // Bonus tạm khóa.
        if (config.rewardType == GachaRewardType.Bonus)
            return false;

        if (!allowClover && config.rewardType == GachaRewardType.Clover)
            return false;

        return config.weight > 0f;
    }

    private GachaReward CreateSpecialReward(GachaRewardType rewardType)
    {
        return rewardType switch
        {
            // GachaRewardType.Bonus => GachaReward.Bonus(),
            GachaRewardType.Clover => GachaReward.Clover(),
            _ => null
        };
    }

    private void AddClover()
    {
        cloverCount++;
    }

    private float GetCloverLuckMultiplier()
    {
        if (cloverCount <= 0)
            return 1f;

        return Mathf.Pow(2f, cloverCount);
    }

    private PetRarity RollRarityFromWeights(List<RarityWeightConfig> weights, float luckMultiplier)
    {
        if (weights == null || weights.Count == 0)
        {
            Debug.LogWarning("Rarity weight config is empty.");
            return default;
        }

        float luck = GetCurrentLuck() * Mathf.Max(1f, luckMultiplier);
        float totalWeight = 0f;

        foreach (RarityWeightConfig config in weights)
        {
            if (config == null)
                continue;

            totalWeight += GetAdjustedWeight(config, luck);
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

            float weight = GetAdjustedWeight(config, luck);

            if (weight <= 0f)
                continue;

            if (value < weight)
                return config.rarity;

            value -= weight;
        }

        for (int i = weights.Count - 1; i >= 0; i--)
        {
            if (weights[i] != null)
                return weights[i].rarity;
        }

        return default;
    }

    private float GetCurrentLuck()
    {
        if (PlayerStatContext.Instance == null)
            return 1f;

        return PlayerStatContext.Instance.GetFinalStat(UpgradeStatType.Luck, 1f);
    }

    public string GetCurrentRateText()
    {
        if (rarityWeights == null || rarityWeights.Count == 0)
            return "Rate: N/A";

        float baseLuck = GetCurrentLuck();
        float cloverMultiplier = GetCloverLuckMultiplier();
        float finalLuck = baseLuck * cloverMultiplier;

        float totalWeight = 0f;

        foreach (RarityWeightConfig config in rarityWeights)
        {
            if (config == null)
                continue;

            totalWeight += GetAdjustedWeight(config, finalLuck);
        }

        if (totalWeight <= 0f)
            return "Rate: N/A";

        System.Text.StringBuilder builder = new();

        builder.AppendLine($"Luck: {baseLuck:0.###}");
        builder.AppendLine($"Clover: {cloverCount}");
        builder.AppendLine($"Clover Luck: x{cloverMultiplier:0.##}");
        builder.AppendLine($"Final Luck: {finalLuck:0.###}");

        foreach (RarityWeightConfig config in rarityWeights)
        {
            if (config == null)
                continue;

            float weight = GetAdjustedWeight(config, finalLuck);
            float percent = weight / totalWeight * 100f;

            builder.AppendLine($"{config.rarity}: {percent:0.###}%");
        }

        builder.AppendLine($"Special: {specialChance * 100f:0.##}%");

        return builder.ToString();
    }

    private float GetAdjustedWeight(RarityWeightConfig config, float luck)
    {
        int rarityIndex = Mathf.Max(0, (int)config.rarity);
        return config.baseWeight * Mathf.Pow(luck, rarityIndex);
    }

    private PetUnitData RollPetByRarity(PetRarity rarity)
    {
        List<PetUnitData> candidates = GetPetsByRarity(rarity);

        if (candidates.Count == 0)
        {
            Debug.LogWarning($"No pets found for rarity: {rarity}");
            return null;
        }

        return candidates[Random.Range(0, candidates.Count)];
    }

    private List<PetUnitData> GetPetsByRarity(PetRarity rarity)
    {
        List<PetUnitData> result = new();

        if (!HasValidDatabase())
            return result;

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

        return petDatabase.Pets[Random.Range(0, petDatabase.Pets.Count)];
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