using System.Collections.Generic;
using UnityEngine;

public class UpgradeTreeView : MonoBehaviour
{
    [Header("System")]
    [SerializeField] private UpgradeTreeSystem treeSystem;

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

        if (nodeLayer == null)
        {
            Debug.LogError("UpgradeTreeView: NodeLayer is missing.");
            return;
        }

        if (lineLayer == null)
        {
            Debug.LogError("UpgradeTreeView: LineLayer is missing.");
            return;
        }

        if (nodePrefab == null)
        {
            Debug.LogError("UpgradeTreeView: NodePrefab is missing.");
            return;
        }

        if (linePrefab == null)
        {
            Debug.LogError("UpgradeTreeView: LinePrefab is missing.");
            return;
        }

        SpawnNodes();
        SpawnLines();
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
            line.Draw(parent.UiPosition, node.UiPosition, lineThickness);

            lineViews.Add(line);
        }
    }

    public void RefreshAll()
    {
        foreach (UpgradeNodeView view in nodeViews.Values)
        {
            if (view != null)
                view.Refresh();
        }
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
    }
}