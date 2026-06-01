using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum UpgradeEffectType
{
    UnlockItem,
    ChangeStat
}

public enum UpgradeStatType
{
    None,
    Luck,
    Exp,
    Gold,
}

[CreateAssetMenu(menuName = "Game/Upgrade Node")]
public class UpgradeNodeData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField, HideInInspector] private string id;

    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;

    [Header("Tree")]
    [SerializeField] private UpgradeNodeData parent;

    [Header("Cost")]
    [SerializeField, Min(0)] private int cost;

    [Header("Effect")]
    [SerializeField] private UpgradeEffectType effectType;

    [SerializeField] private string targetId;

    [SerializeField] private UpgradeStatType statType;
    [SerializeField] private StatModifierType statModifierType;
    [SerializeField] private float value;

    public string Id => id;
    public string DisplayName => displayName;
    public Sprite Icon => icon;

    public UpgradeNodeData Parent => parent;
    public string ParentId => parent != null ? parent.Id : string.Empty;
    public bool IsRoot => parent == null;

    public int Cost => cost;

    public UpgradeEffectType EffectType => effectType;

    public string TargetId => targetId;

    public UpgradeStatType StatType => statType;
    public StatModifierType StatModifierType => statModifierType;
    public float Value => value;

    public void Apply(UpgradeContext context)
    {
        if (context == null)
            return;

        switch (effectType)
        {
            case UpgradeEffectType.UnlockItem:
                context.UnlockItem(targetId);
                break;

            case UpgradeEffectType.ChangeStat:
                context.Stats.AddStat(statType, statModifierType, value);
                break;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ValidateParent();
        ValidateId();
        ValidateEffectFields();
    }

    private void ValidateParent()
    {
        if (parent == this)
        {
            parent = null;
            Debug.LogError($"Upgrade node '{name}' cannot be its own parent.");
            EditorUtility.SetDirty(this);
            return;
        }

        if (HasCircularParent())
        {
            parent = null;
            Debug.LogError($"Upgrade node '{name}' has circular parent reference. Parent has been cleared.");
            EditorUtility.SetDirty(this);
        }
    }

    private void ValidateId()
    {
        if (!string.IsNullOrWhiteSpace(id))
            return;

        id = GenerateStableId();
        EditorUtility.SetDirty(this);
    }

    private void ValidateEffectFields()
    {
        switch (effectType)
        {
            case UpgradeEffectType.UnlockItem:
                statType = UpgradeStatType.None;
                statModifierType = StatModifierType.Flat;
                value = 0f;
                break;

            case UpgradeEffectType.ChangeStat:
                targetId = string.Empty;

                if (statType == UpgradeStatType.None)
                    statType = UpgradeStatType.Luck;

                break;
        }
    }

    private bool HasCircularParent()
    {
        UpgradeNodeData current = parent;

        while (current != null)
        {
            if (current == this)
                return true;

            current = current.Parent;
        }

        return false;
    }

    private string GenerateStableId()
    {
        string path = AssetDatabase.GetAssetPath(this);
        string guid = AssetDatabase.AssetPathToGUID(path);

        if (string.IsNullOrWhiteSpace(guid))
            return System.Guid.NewGuid().ToString("N");

        return $"UN_{guid[..8].ToUpper()}";
    }
#endif
}