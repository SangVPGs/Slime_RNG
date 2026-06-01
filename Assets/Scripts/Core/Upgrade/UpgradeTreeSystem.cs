using System;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeTreeSystem : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private UpgradeDatabase database;

    [Header("Currency")]
    [SerializeField, Min(0)] private int defaultUpgradePoint = 10;

    private int upgradePoint;

    private readonly HashSet<string> unlockedNodeIds = new();
    private readonly List<string> unlockedNodeOrder = new();
    private readonly Dictionary<string, UpgradeNodeData> nodeMap = new();

    private UpgradeContext context = new();

    private const string SaveKey = "UpgradeTree_SaveData";

    public event Action<UpgradeNodeData> OnNodeUnlocked;
    public event Action OnTreeChanged;

    public int UpgradePoint => upgradePoint;
    public UpgradeContext Context => context;

    public IReadOnlyCollection<string> UnlockedNodeIds => unlockedNodeIds;
    public IReadOnlyList<string> UnlockedNodeOrder => unlockedNodeOrder;
    public IReadOnlyList<UpgradeNodeData> AllNodes => database != null ? database.Nodes : Array.Empty<UpgradeNodeData>();

    private void Awake()
    {
        upgradePoint = defaultUpgradePoint;

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
        return !string.IsNullOrWhiteSpace(nodeId) && unlockedNodeIds.Contains(nodeId);
    }

    public bool IsUnlocked(UpgradeNodeData node)
    {
        return node != null && IsUnlocked(node.Id);
    }

    public bool CanUnlock(UpgradeNodeData node)
    {
        if (node == null)
            return false;

        if (string.IsNullOrWhiteSpace(node.Id))
            return false;

        if (IsUnlocked(node))
            return false;

        if (upgradePoint < node.Cost)
            return false;

        if (node.IsRoot)
            return true;

        return IsUnlocked(node.ParentId);
    }

    public bool Unlock(UpgradeNodeData node)
    {
        if (!CanUnlock(node))
            return false;

        upgradePoint -= node.Cost;

        unlockedNodeIds.Add(node.Id);
        unlockedNodeOrder.Add(node.Id);

        node.Apply(context);

        Save();

        OnNodeUnlocked?.Invoke(node);
        OnTreeChanged?.Invoke();

        return true;
    }

    public bool UnlockById(string nodeId)
    {
        UpgradeNodeData node = GetNodeById(nodeId);
        return Unlock(node);
    }

    public List<UpgradeNodeData> GetChildren(string parentId)
    {
        List<UpgradeNodeData> result = new();

        if (database == null || string.IsNullOrWhiteSpace(parentId))
            return result;

        foreach (UpgradeNodeData node in database.Nodes)
        {
            if (node == null)
                continue;

            if (node.ParentId == parentId)
                result.Add(node);
        }

        return result;
    }

    public List<UpgradeNodeData> GetRootNodes()
    {
        List<UpgradeNodeData> result = new();

        if (database == null)
            return result;

        foreach (UpgradeNodeData node in database.Nodes)
        {
            if (node != null && node.IsRoot)
                result.Add(node);
        }

        return result;
    }

    public List<UpgradeNodeData> GetAvailableNodes()
    {
        List<UpgradeNodeData> result = new();

        if (database == null)
            return result;

        foreach (UpgradeNodeData node in database.Nodes)
        {
            if (CanUnlock(node))
                result.Add(node);
        }

        return result;
    }

    public void AddUpgradePoint(int amount)
    {
        if (amount <= 0)
            return;

        upgradePoint += amount;

        Save();
        OnTreeChanged?.Invoke();
    }

    public bool SpendUpgradePoint(int amount)
    {
        if (amount <= 0)
            return false;

        if (upgradePoint < amount)
            return false;

        upgradePoint -= amount;

        Save();
        OnTreeChanged?.Invoke();

        return true;
    }

    public void ResetTree(bool resetPoint = true)
    {
        unlockedNodeIds.Clear();
        unlockedNodeOrder.Clear();

        context = new UpgradeContext();

        if (resetPoint)
            upgradePoint = defaultUpgradePoint;

        Save();

        OnTreeChanged?.Invoke();
    }

    public void ClearSave()
    {
        unlockedNodeIds.Clear();
        unlockedNodeOrder.Clear();

        context = new UpgradeContext();
        upgradePoint = defaultUpgradePoint;

        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();

        OnTreeChanged?.Invoke();
    }

    private void RebuildContext()
    {
        context = new UpgradeContext();

        foreach (string nodeId in unlockedNodeOrder)
        {
            UpgradeNodeData node = GetNodeById(nodeId);

            if (node == null)
                continue;

            node.Apply(context);
        }
    }

    private void Save()
    {
        UpgradeTreeSaveData saveData = new()
        {
            upgradePoint = upgradePoint,
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

        upgradePoint = defaultUpgradePoint;

        if (!PlayerPrefs.HasKey(SaveKey))
            return;

        string json = PlayerPrefs.GetString(SaveKey);

        if (string.IsNullOrWhiteSpace(json))
            return;

        UpgradeTreeSaveData saveData = JsonUtility.FromJson<UpgradeTreeSaveData>(json);

        if (saveData == null)
            return;

        upgradePoint = Mathf.Max(0, saveData.upgradePoint);

        if (saveData.unlockedIds == null)
            return;

        foreach (string nodeId in saveData.unlockedIds)
        {
            if (string.IsNullOrWhiteSpace(nodeId))
                continue;

            if (!nodeMap.ContainsKey(nodeId))
            {
                Debug.LogWarning($"Saved upgrade id not found in current database: {nodeId}");
                continue;
            }

            if (unlockedNodeIds.Add(nodeId))
                unlockedNodeOrder.Add(nodeId);
        }
    }

    [Serializable]
    private class UpgradeTreeSaveData
    {
        public int upgradePoint;
        public List<string> unlockedIds = new();
    }
}