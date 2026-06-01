using UnityEditor;
using UnityEngine;

public enum ItemType
{
    Food,
    BuffStat
}

[CreateAssetMenu(menuName = "Game/Item")]
public class ItemData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string id;
    [SerializeField] private string itemName;
    [TextArea]
    [SerializeField] private string description;

    [Header("Visual")]
    [SerializeField] private Sprite icon;
    [SerializeField] private GameObject prefab;

    [Header("Stack")]
    [SerializeField] private bool stackable = true;
    [SerializeField] private int maxStack = 99;

    [Header("Effect")]
    [SerializeField] private ItemType itemType;

    [SerializeField] private UpgradeStatType statType;
    [SerializeField] private StatModifierType statModifier;
    [SerializeField] private float value;
    [SerializeField] private float duration;

    public string Id => id;
    public string ItemName => itemName;
    public string Description => description;

    public Sprite Icon => icon;
    public GameObject Prefab => prefab;

    public bool Stackable => stackable;
    public int MaxStack => maxStack;

    public ItemType ItemType => itemType;
    public UpgradeStatType StatType => statType;
    public StatModifierType StatModifier => statModifier;
    public float Value => value;
    public float Duration => duration;

    public bool IsFood => itemType == ItemType.Food;
    public bool IsBuffStat => itemType == ItemType.BuffStat;

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
        string[] guids = AssetDatabase.FindAssets("t:ItemData");

        int count = guids.Length;

        return $"I{count:000}";
    }

#endif
}