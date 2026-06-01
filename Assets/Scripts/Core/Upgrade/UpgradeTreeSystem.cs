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

    private UpgradeContext context = new();

    private const string SaveKey = "UpgradeTree_SaveData";

    public event Action<UpgradeNodeData> OnNodeUnlocked;
    public event Action OnTreeChanged;

    public UpgradeContext Context => context;

    public IReadOnlyCollection<string> UnlockedNodeIds => unlockedNodeIds;
    public IReadOnlyList<string> UnlockedNodeOrder => unlockedNodeOrder;

    public IReadOnlyList<UpgradeNodeData> AllNodes =>
        database != null ? database.Nodes : Array.Empty<UpgradeNodeData>();

    private void Awake()
    {
        BuildNodeMap();
        Load();
        RebuildContext();
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
        return context != null &&
               context.IsItemUnlocked(itemId);
    }

    public bool CanUnlock(UpgradeNodeData node)
    {
        if (node == null)
            return false;

        if (string.IsNullOrWhiteSpace(node.Id))
            return false;

        if (IsUnlocked(node))
            return false;

        if (!CanPay(node.Cost))
            return false;

        if (node.IsRoot)
            return true;

        return IsUnlocked(node.ParentId);
    }

    private bool CanPay(int cost)
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

    private bool Pay(int cost)
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

        if (!Pay(node.Cost))
            return false;

        bool added = unlockedNodeIds.Add(node.Id);

        if (!added)
            return false;

        unlockedNodeOrder.Add(node.Id);

        ApplyNodeToContext(node);

        Save();

        Debug.Log($"Upgrade unlocked: {node.DisplayName} (ID: {node.Id})");

        OnNodeUnlocked?.Invoke(node);
        OnTreeChanged?.Invoke();

        return true;
    }

    private void ApplyNodeToContext(UpgradeNodeData node)
    {
        if (node == null)
            return;

        node.Apply(context);
    }

    private void RebuildContext()
    {
        context = new UpgradeContext();

        foreach (string nodeId in unlockedNodeOrder)
        {
            UpgradeNodeData node = GetNodeById(nodeId);

            if (node == null)
                continue;

            ApplyNodeToContext(node);
        }
    }

    public void RefreshContext()
    {
        RebuildContext();
        OnTreeChanged?.Invoke();
    }

    public void ClearSave()
    {
        unlockedNodeIds.Clear();
        unlockedNodeOrder.Clear();

        context.Clear();

        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();

        OnTreeChanged?.Invoke();
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

    [Serializable]
    private class UpgradeTreeSaveData
    {
        public List<string> unlockedIds = new();
    }
}