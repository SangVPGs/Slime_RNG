using System;
using UnityEngine;

[Serializable]
public class RarityWeightConfig
{
    public PetRarity rarity;

    [Min(0f)]
    public float baseWeight = 1f;

    [Min(0f)]
    public float multiplier = 1f;

    public float FinalWeight => baseWeight * multiplier;
}

[Serializable]
public class GachaDisplayPetConfig
{
    public PetUnitData pet;

    [Min(0)]
    public int weight = 100;
}