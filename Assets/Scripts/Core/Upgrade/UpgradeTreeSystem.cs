using System;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeTreeSystem : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private UpgradeDatabase database;

    private readonly HashSet<string> unlockedNodeIds = new();
    private readonly List<string> unlockedNodeOrder = new();
    private readonly Dictionary<string, UpgradeNodeData> nodeMap = new();

    private const string SaveKey = "UpgradeTree_SaveData";

    public event Action<UpgradeNodeData> OnNodeUnlocked;
    public event Action OnTreeChanged;

    public IReadOnlyCollection<string> UnlockedNodeIds => unlockedNodeIds;
    public IReadOnlyList<string> UnlockedNodeOrder => unlockedNodeOrder;

    public IReadOnlyList<UpgradeNodeData> AllNodes => database != null ? database.Nodes : Array.Empty<UpgradeNodeData>();

    public static UpgradeTreeSystem Instance { get; private set; }

    private PlayerStatContext PlayerStats => PlayerStatContext.Instance;
    private UnlockItemContext UnlockItems => UnlockItemContext.Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        BuildNodeMap();
        Load();
        RebuildUpgradeEffects();
    }

    private void BuildNodeMap()
    {
        nodeMap.Clear();

        if (database == null)
        {
            Debug.LogError("UpgradeTreeSystem: Upgrade database is missing.");
            return;
        }

        foreach (UpgradeNodeData node in database.Nodes)
        {
            if (node == null)
                continue;

            if (string.IsNullOrWhiteSpace(node.Id))
            {
                Debug.LogWarning($"Upgrade node '{node.name}' has empty id.");
                continue;
            }

            if (nodeMap.ContainsKey(node.Id))
            {
                Debug.LogWarning($"Duplicate upgrade node id detected: {node.Id}");
                continue;
            }

            nodeMap.Add(node.Id, node);
        }
    }

    public UpgradeNodeData GetNodeById(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
            return null;

        nodeMap.TryGetValue(nodeId, out UpgradeNodeData node);
        return node;
    }

    public List<UpgradeNodeData> GetChildren(string parentId)
    {
        List<UpgradeNodeData> children = new();

        if (string.IsNullOrWhiteSpace(parentId))
            return children;

        foreach (UpgradeNodeData node in AllNodes)
        {
            if (node == null)
                continue;

            if (node.Parent == null)
                continue;

            if (node.ParentId == parentId)
                children.Add(node);
        }

        return children;
    }

    public List<UpgradeNodeData> GetChildren(UpgradeNodeData parent)
    {
        if (parent == null)
            return new List<UpgradeNodeData>();

        return GetChildren(parent.Id);
    }

    public bool IsUnlocked(string nodeId)
    {
        return !string.IsNullOrWhiteSpace(nodeId) &&
               unlockedNodeIds.Contains(nodeId);
    }

    public bool IsUnlocked(UpgradeNodeData node)
    {
        return node != null && IsUnlocked(node.Id);
    }

    public bool IsItemUnlocked(string itemId)
    {
        return UnlockItems != null &&
               UnlockItems.IsItemUnlocked(itemId);
    }

    public bool CanUnlock(UpgradeNodeData node)
    {
        if (node == null)
            return false;

        if (string.IsNullOrWhiteSpace(node.Id))
            return false;

        if (IsUnlocked(node))
            return false;

        long finalCost = node.Cost;

        if (!CanPay(finalCost))
            return false;

        if (node.IsRoot)
            return true;

        return IsUnlocked(node.ParentId);
    }

    private bool CanPay(long cost)
    {
        if (cost <= 0)
            return true;

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("UpgradeTreeSystem: GameManager.Instance is missing.");
            return false;
        }

        return GameManager.Instance.HasEnoughGold(cost);
    }

    private bool Pay(long cost)
    {
        if (cost <= 0)
            return true;

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("UpgradeTreeSystem: GameManager.Instance is missing.");
            return false;
        }

        return GameManager.Instance.SpendGold(cost);
    }

    public bool Unlock(UpgradeNodeData node)
    {
        if (!CanUnlock(node))
            return false;

        long finalCost = node.Cost;

        if (!Pay(finalCost))
            return false;

        bool added = unlockedNodeIds.Add(node.Id);

        if (!added)
            return false;

        unlockedNodeOrder.Add(node.Id);

        Save();
        RebuildUpgradeEffects();

        Debug.Log($"Upgrade unlocked: {node.DisplayName} (ID: {node.Id})");

        OnNodeUnlocked?.Invoke(node);

        return true;
    }

    public int GetUnlockableNodeCount()
    {
        int count = 0;

        foreach (UpgradeNodeData node in AllNodes)
        {
            if (CanUnlock(node))
                count++;
        }

        return count;
    }

    public bool HasUnlockableNode()
    {
        return GetUnlockableNodeCount() > 0;
    }

    private void RebuildUpgradeEffects()
    {
        if (PlayerStats != null)
            PlayerStats.ClearUpgradeStats();

        if (UnlockItems != null)
            UnlockItems.Clear();

        foreach (string nodeId in unlockedNodeOrder)
        {
            UpgradeNodeData node = GetNodeById(nodeId);

            if (node == null)
                continue;

            switch (node.EffectType)
            {
                case UpgradeEffectType.ChangeStat:
                    node.ApplyStat(PlayerStats);
                    break;

                case UpgradeEffectType.UnlockItem:
                    node.ApplyUnlockItem(UnlockItems);
                    break;
            }
        }

        if (PartySystem.Instance != null)
            PartySystem.Instance.RefreshAfterUpgradeChanged();

        OnTreeChanged?.Invoke();
    }

    public void RefreshUpgradeEffects()
    {
        RebuildUpgradeEffects();
    }

    private void Save()
    {
        UpgradeTreeSaveData saveData = new()
        {
            unlockedIds = new List<string>(unlockedNodeOrder)
        };

        string json = JsonUtility.ToJson(saveData);

        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        unlockedNodeIds.Clear();
        unlockedNodeOrder.Clear();

        if (!PlayerPrefs.HasKey(SaveKey))
            return;

        string json = PlayerPrefs.GetString(SaveKey);

        if (string.IsNullOrWhiteSpace(json))
            return;

        UpgradeTreeSaveData saveData =
            JsonUtility.FromJson<UpgradeTreeSaveData>(json);

        if (saveData == null || saveData.unlockedIds == null)
            return;

        foreach (string nodeId in saveData.unlockedIds)
        {
            if (string.IsNullOrWhiteSpace(nodeId))
                continue;

            if (!nodeMap.ContainsKey(nodeId))
            {
                Debug.LogWarning(
                    $"Saved upgrade id not found in current database: {nodeId}"
                );

                continue;
            }

            if (unlockedNodeIds.Add(nodeId))
                unlockedNodeOrder.Add(nodeId);
        }
    }

    public void ClearData()
    {
        unlockedNodeIds.Clear();
        unlockedNodeOrder.Clear();

        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();

        RebuildUpgradeEffects();
    }

    [ContextMenu("Debug Upgrade Stats")]
    private void DebugUpgradeStats()
    {
        if (PlayerStats == null)
        {
            Debug.LogWarning("PlayerStatContext is missing.");
            return;
        }

        Debug.Log($"Luck: {PlayerStats.GetFinalStat(UpgradeStatType.Luck, 1f)}");
        Debug.Log($"Max Party Size: {PlayerStats.GetFinalStat(UpgradeStatType.MaxPartySize, 4f)}");
        Debug.Log($"Slime Pool Size: {PlayerStats.GetFinalStat(UpgradeStatType.SlimePoolSize, 5f)}");
    }

    [Serializable]
    private class UpgradeTreeSaveData
    {
        public List<string> unlockedIds = new();
    }
}