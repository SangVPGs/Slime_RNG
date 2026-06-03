using System.Collections.Generic;
using UnityEngine;

public class UpgradeTreeView : MonoBehaviour
{
    private UpgradeTreeSystem treeSystem => UpgradeTreeSystem.Instance;

    [Header("Layers")]
    [SerializeField] private RectTransform nodeLayer;
    [SerializeField] private RectTransform lineLayer;

    [Header("Prefabs")]
    [SerializeField] private UpgradeNodeView nodePrefab;
    [SerializeField] private UpgradeLineView linePrefab;

    [Header("Line")]
    [SerializeField, Min(1f)] private float lineThickness = 6f;

    private readonly Dictionary<string, UpgradeNodeView> nodeViews = new();
    private readonly List<UpgradeLineView> lineViews = new();

    private bool built;

    private void OnEnable()
    {
        if (treeSystem != null)
            treeSystem.OnTreeChanged += RefreshAll;
    }

    private void Start()
    {
        Build();
    }

    private void OnDisable()
    {
        if (treeSystem != null)
            treeSystem.OnTreeChanged -= RefreshAll;
    }

    public void Build()
    {
        Clear();

        if (treeSystem == null)
        {
            Debug.LogError("UpgradeTreeView: TreeSystem is missing.");
            return;
        }

        if (nodeLayer == null || lineLayer == null || nodePrefab == null || linePrefab == null)
        {
            Debug.LogError("UpgradeTreeView: Missing layer or prefab reference.");
            return;
        }

        SpawnNodes();
        SpawnLines();

        built = true;

        RefreshAll();
    }

    private void SpawnNodes()
    {
        foreach (UpgradeNodeData node in treeSystem.AllNodes)
        {
            if (node == null)
                continue;

            if (string.IsNullOrWhiteSpace(node.Id))
                continue;

            UpgradeNodeView view = Instantiate(nodePrefab, nodeLayer);
            view.RectTransform.anchoredPosition = node.UiPosition;
            view.Setup(treeSystem, node);

            if (!nodeViews.TryAdd(node.Id, view))
                Debug.LogWarning($"UpgradeTreeView: Duplicate node view id: {node.Id}");
        }
    }

    private void SpawnLines()
    {
        foreach (UpgradeNodeData node in treeSystem.AllNodes)
        {
            if (node == null || node.Parent == null)
                continue;

            UpgradeNodeData parent = node.Parent;

            if (!nodeViews.ContainsKey(parent.Id))
                continue;

            if (!nodeViews.ContainsKey(node.Id))
                continue;

            UpgradeLineView line = Instantiate(linePrefab, lineLayer);
            line.Setup(parent, node);
            line.Draw(parent.UiPosition, node.UiPosition, lineThickness);

            lineViews.Add(line);
        }
    }

    public void RefreshAll()
    {
        if (!built)
            return;

        RefreshNodeViews();
        RefreshLineViews();
    }

    private void RefreshNodeViews()
    {
        foreach (UpgradeNodeView view in nodeViews.Values)
        {
            if (view == null || view.Node == null)
                continue;

            bool visible = ShouldShowNode(view.Node);

            view.SetVisible(visible);

            if (visible)
                view.Refresh();
        }
    }

    private void RefreshLineViews()
    {
        foreach (UpgradeLineView line in lineViews)
        {
            if (line == null || line.FromNode == null || line.ToNode == null)
                continue;

            bool visible = ShouldShowLine(line.FromNode, line.ToNode);

            line.SetVisible(visible);
        }
    }

    private bool ShouldShowNode(UpgradeNodeData node)
    {
        if (node == null)
            return false;

        if (node.IsRoot)
            return true;

        return treeSystem.IsUnlocked(node.ParentId);
    }

    private bool ShouldShowLine(UpgradeNodeData fromNode, UpgradeNodeData toNode)
    {
        if (fromNode == null || toNode == null)
            return false;

        return ShouldShowNode(fromNode) && ShouldShowNode(toNode);
    }

    private void Clear()
    {
        foreach (UpgradeNodeView view in nodeViews.Values)
        {
            if (view != null)
                Destroy(view.gameObject);
        }

        foreach (UpgradeLineView line in lineViews)
        {
            if (line != null)
                Destroy(line.gameObject);
        }

        nodeViews.Clear();
        lineViews.Clear();

        built = false;
    }
}