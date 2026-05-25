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
    public int heal;
    public PetRarity rarity;
    public Sprite icon;

    private float CombatPower => (atk * 5 + hp * 2 + speed * 10 + atkSpeed * 10 + heal * 7);
    public int combatPower => Mathf.RoundToInt(CombatPower);

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