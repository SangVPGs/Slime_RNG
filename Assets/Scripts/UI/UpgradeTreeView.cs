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

    [Header("Layout")]
    [SerializeField] private Vector2 rootPosition = Vector2.zero;
    [SerializeField, Min(10f)] private float nodeDistance = 180f;
    [SerializeField, Min(10f)] private float rootSpacing = 300f;

    [Header("Line")]
    [SerializeField, Min(1f)] private float lineThickness = 6f;

    [Header("Detail")]
    [SerializeField] private UpgradeDetailPanel detailPanel;

    private readonly Dictionary<string, UpgradeNodeView> nodeViews = new();
    private readonly Dictionary<string, Vector2> nodePositions = new();
    private readonly List<UpgradeLineView> lineViews = new();

    private bool built;

    private void Start()
    {
        if (treeSystem != null)
            treeSystem.OnTreeChanged += RefreshAll;

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

        CalculateNodePositions();
        SpawnNodes();
        SpawnLines();

        built = true;
        RefreshAll();
    }

    private void CalculateNodePositions()
    {
        nodePositions.Clear();

        List<UpgradeNodeData> rootNodes = GetRootNodes();

        for (int i = 0; i < rootNodes.Count; i++)
        {
            UpgradeNodeData root = rootNodes[i];

            Vector2 position = rootPosition + new Vector2(i * rootSpacing, 0f);
            CalculateNodePositionRecursive(root, position, new HashSet<string>());
        }
    }

    private void CalculateNodePositionRecursive(
        UpgradeNodeData node,
        Vector2 position,
        HashSet<string> visited)
    {
        if (node == null)
            return;

        if (string.IsNullOrWhiteSpace(node.Id))
            return;

        if (visited.Contains(node.Id))
        {
            Debug.LogError($"UpgradeTreeView: Circular reference detected at node {node.Id}.", node);
            return;
        }

        visited.Add(node.Id);

        if (!nodePositions.TryAdd(node.Id, position))
        {
            Debug.LogWarning($"UpgradeTreeView: Duplicate node position id: {node.Id}", node);
            return;
        }

        List<UpgradeNodeData> children = treeSystem.GetChildren(node.Id);

        foreach (UpgradeNodeData child in children)
        {
            if (child == null)
                continue;

            Vector2 childPosition = position + GetDirectionVector(child.Direction) * nodeDistance;
            CalculateNodePositionRecursive(child, childPosition, visited);
        }

        visited.Remove(node.Id);
    }

    private void SpawnNodes()
    {
        foreach (UpgradeNodeData node in treeSystem.AllNodes)
        {
            if (node == null)
                continue;

            if (string.IsNullOrWhiteSpace(node.Id))
                continue;

            if (!nodePositions.TryGetValue(node.Id, out Vector2 position))
                continue;

            UpgradeNodeView view = Instantiate(nodePrefab, nodeLayer);
            view.RectTransform.anchoredPosition = position;
            view.Setup(treeSystem, this, node);

            if (!nodeViews.TryAdd(node.Id, view))
                Debug.LogWarning($"UpgradeTreeView: Duplicate node view id: {node.Id}", node);
        }
    }

    private void SpawnLines()
    {
        foreach (UpgradeNodeData node in treeSystem.AllNodes)
        {
            if (node == null || node.Parent == null)
                continue;

            UpgradeNodeData parent = node.Parent;

            if (!nodePositions.TryGetValue(parent.Id, out Vector2 parentPosition))
                continue;

            if (!nodePositions.TryGetValue(node.Id, out Vector2 nodePosition))
                continue;

            UpgradeLineView line = Instantiate(linePrefab, lineLayer);
            line.Setup(parent, node);
            line.Draw(parentPosition, nodePosition, lineThickness);

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

    private List<UpgradeNodeData> GetRootNodes()
    {
        List<UpgradeNodeData> result = new();

        foreach (UpgradeNodeData node in treeSystem.AllNodes)
        {
            if (node != null && node.IsRoot)
                result.Add(node);
        }

        return result;
    }

    public void SelectNode(UpgradeNodeData node)
    {
        if (node == null)
            return;

        if (detailPanel != null)
            detailPanel.Show(node);

        if (treeSystem != null && treeSystem.CanUnlock(node))
            treeSystem.Unlock(node);
    }

    private Vector2 GetDirectionVector(UpgradeNodeDirection direction)
    {
        return direction switch
        {
            UpgradeNodeDirection.Right => Vector2.right,
            UpgradeNodeDirection.UpRight => new Vector2(1f, 1f).normalized,
            UpgradeNodeDirection.Up => Vector2.up,
            UpgradeNodeDirection.UpLeft => new Vector2(-1f, 1f).normalized,
            UpgradeNodeDirection.Left => Vector2.left,
            UpgradeNodeDirection.DownLeft => new Vector2(-1f, -1f).normalized,
            UpgradeNodeDirection.Down => Vector2.down,
            UpgradeNodeDirection.DownRight => new Vector2(1f, -1f).normalized,
            _ => Vector2.right
        };
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
        nodePositions.Clear();
        lineViews.Clear();

        built = false;
    }
}