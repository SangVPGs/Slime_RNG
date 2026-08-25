using System;
using UnityEngine;

public enum GachaRewardType
{
    Pet,
    Bonus,
    Clover
}

[Serializable]
public class RarityWeightConfig
{
    public PetRarity rarity;

    [Min(0f)]
    public float baseWeight = 1f;
}

[Serializable]
public class GachaSpecialWeightConfig
{
    public GachaRewardType rewardType;

    [Min(0f)]
    public float weight = 1f;
}

[Serializable]
public class GachaReward
{
    public GachaRewardType rewardType;
    public PetUnitData pet;

    public bool IsPet => rewardType == GachaRewardType.Pet && pet != null;
    public bool IsBonus => rewardType == GachaRewardType.Bonus;
    public bool IsClover => rewardType == GachaRewardType.Clover;
    public bool CanChain => IsBonus || IsClover;

    public static GachaReward Pet(PetUnitData pet)
    {
        return new GachaReward
        {
            rewardType = GachaRewardType.Pet,
            pet = pet
        };
    }

    public static GachaReward Bonus()
    {
        return new GachaReward
        {
            rewardType = GachaRewardType.Bonus,
            pet = null
        };
    }

    public static GachaReward Clover()
    {
        return new GachaReward
        {
            rewardType = GachaRewardType.Clover,
            pet = null
        };
    }
}