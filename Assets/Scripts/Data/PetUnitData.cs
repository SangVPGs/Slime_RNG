using UnityEditor;
using UnityEngine;

public enum PetRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

[CreateAssetMenu(menuName = "Game/Unit/Pet")]
public class PetUnitData : UnitData
{
    [Header("Pet Info")]
    public long baseHeal;
    public PetRarity rarity;
    public Sprite icon;

    [Header("Pet Growth Scale")]
    public float hpGrowthMultiplier = 1.12f;
    public float atkGrowthMultiplier = 1.08f;
    public float healGrowthMultiplier = 1.05f;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(id))
        {
            id = GeneratePetId();
            EditorUtility.SetDirty(this);
        }
    }

    private string GeneratePetId()
    {
        string[] guids = AssetDatabase.FindAssets("t:PetUnitData");

        int count = guids.Length;

        return $"P{count:000}";
    }

#endif
}