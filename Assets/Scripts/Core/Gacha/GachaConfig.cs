using System;
using UnityEngine;

[Serializable]
public class RarityWeightConfig
{
    public PetRarity rarity;

    [Min(0f)]
    public float baseWeight = 1f;
}

[Serializable]
public class GachaDisplayPetConfig
{
    public PetUnitData pet;

    [Min(0)]
    public int weight = 100;
}