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

    //ExpGain,
    //GoldGain,

    //UnlockGoldCostReduction,
    //RebirthGoldCostReduction,
}

[CreateAssetMenu(menuName = "Game/Upgrade Node")]
public class UpgradeNodeData : ScriptableObject
{
    [SerializeField, HideInInspector] private string id;

    [Header("Info")]
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;

    [Header("Tree")]
    [SerializeField] private UpgradeNodeData parent;
    [SerializeField] private Vector2 uiPosition;
    [SerializeField, Min(0)] private int cost;

    [Header("Effect")]
    [SerializeField] private UpgradeEffectType effectType;

    [Header("Unlock Item")]
    [SerializeField] private ItemData targetItem;

    [Header("Change Stat")]
    [SerializeField] private UpgradeStatType statType;
    [SerializeField] private StatModifierType statModifierType;
    [SerializeField] private float value;

    public string Id => id;
    public string DisplayName => displayName;
    public Sprite Icon => icon;

    public UpgradeNodeData Parent => parent;
    public string ParentId => parent != null ? parent.Id : string.Empty;
    public bool IsRoot => parent == null;

    public Vector2 UiPosition => uiPosition;
    public int Cost => cost;

    public UpgradeEffectType EffectType => effectType;

    public ItemData TargetItem => targetItem;
    public string TargetId => targetItem != null ? targetItem.Id : string.Empty;

    public UpgradeStatType StatType => statType;
    public StatModifierType StatModifierType => statModifierType;
    public float Value => value;

    public void ApplyStat(PlayerStatContext playerStats)
    {
        if (playerStats == null)
            return;

        if (effectType != UpgradeEffectType.ChangeStat)
            return;

        playerStats.UpgradeStats.AddStat(
            statType,
            statModifierType,
            value
        );
    }

    public void ApplyUnlockItem(UnlockItemContext unlockContext)
    {
        if (unlockContext == null)
            return;

        if (effectType != UpgradeEffectType.UnlockItem)
            return;

        if (targetItem == null)
            return;

        unlockContext.UnlockItem(targetItem.Id);
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
            Debug.LogError(
                $"Upgrade node '{name}' cannot be its own parent.",
                this
            );
            EditorUtility.SetDirty(this);
            return;
        }

        if (HasCircularParent())
        {
            parent = null;
            Debug.LogError(
                $"Upgrade node '{name}' has circular parent reference. Parent has been cleared.",
                this
            );
            EditorUtility.SetDirty(this);
        }
    }

    private void ValidateId()
    {
        if (!string.IsNullOrWhiteSpace(id))
            return;

        id = GenerateId();
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
                targetItem = null;

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

    private string GenerateId()
    {
        string[] guids = AssetDatabase.FindAssets("t:UpgradeNodeData");
        int count = guids.Length;

        return $"UP{count:000}";
    }
#endif
}