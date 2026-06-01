using System.Collections.Generic;
using UnityEngine;

public class UpgradeTreeUI : MonoBehaviour
{
    [Header("System")]
    [SerializeField] private UpgradeTreeSystem treeSystem;

    [Header("UI Roots")]
    [SerializeField] private RectTransform nodesRoot;
    [SerializeField] private RectTransform arrowsRoot;

    [Header("Prefabs")]
    [SerializeField] private UpgradeNodeView nodeViewPrefab;
    [SerializeField] private UpgradeArrowView arrowViewPrefab;

    [Header("Radial Layout")]
    [SerializeField, Min(50f)] private float nodeSpacing = 220f;
    [SerializeField, Min(0f)] private float siblingSpreadAngle = 35f;
    [SerializeField] private float startAngle = 0f;

    private readonly Dictionary<UpgradeNodeData, UpgradeNodeView> nodeViews = new();
    private readonly Dictionary<UpgradeNodeData, Vector2> nodePositions = new();
    private readonly List<ArrowBinding> arrowBindings = new();

    private void OnEnable()
    {
        if (treeSystem != null)
            treeSystem.OnTreeChanged += RefreshAll;

        BuildUI();
    }

    private void OnDisable()
    {
        if (treeSystem != null)
            treeSystem.OnTreeChanged -= RefreshAll;
    }

    public void BuildUI()
    {
        Clear();

        if (treeSystem == null || nodesRoot == null || arrowsRoot == null || nodeViewPrefab == null)
        {
            Debug.LogWarning("UpgradeTreeUI is missing references.");
            return;
        }

        List<UpgradeNodeData> roots = treeSystem.GetRootNodes();

        if (roots.Count == 0)
        {
            Debug.LogWarning("Upgrade tree has no root node.");
            return;
        }

        if (roots.Count > 1)
            Debug.LogWarning("Upgrade tree has multiple root nodes. Radial layout will use the first root only.");

        UpgradeNodeData root = roots[0];

        CalculateRadialLayout(root);
        SpawnNodes();
        SpawnArrows();
        RefreshAll();
    }

    private void CalculateRadialLayout(UpgradeNodeData root)
    {
        nodePositions.Clear();

        nodePositions[root] = Vector2.zero;

        List<UpgradeNodeData> branchRoots = treeSystem.GetChildren(root.Id);

        int branchCount = branchRoots.Count;

        if (branchCount == 0)
            return;

        float angleStep = 360f / branchCount;

        for (int i = 0; i < branchCount; i++)
        {
            float branchAngle = startAngle + angleStep * i;
            UpgradeNodeData branchRoot = branchRoots[i];

            Vector2 direction = AngleToDirection(branchAngle);
            Vector2 position = direction * nodeSpacing;

            nodePositions[branchRoot] = position;

            HashSet<string> path = new();
            path.Add(root.Id);

            CalculateBranchRecursive(
                branchRoot,
                position,
                branchAngle,
                1,
                path
            );
        }
    }

    private void CalculateBranchRecursive(
        UpgradeNodeData parent,
        Vector2 parentPosition,
        float branchAngle,
        int depth,
        HashSet<string> path)
    {
        if (parent == null)
            return;

        if (path.Contains(parent.Id))
        {
            Debug.LogError($"Circular upgrade tree reference detected at node: {parent.Id}");
            return;
        }

        path.Add(parent.Id);

        List<UpgradeNodeData> children = treeSystem.GetChildren(parent.Id);

        if (children.Count == 0)
        {
            path.Remove(parent.Id);
            return;
        }

        float startOffset = -siblingSpreadAngle * 0.5f;
        float step = children.Count > 1
            ? siblingSpreadAngle / (children.Count - 1)
            : 0f;

        for (int i = 0; i < children.Count; i++)
        {
            UpgradeNodeData child = children[i];

            float offsetAngle = children.Count == 1
                ? 0f
                : startOffset + step * i;

            float childAngle = branchAngle + offsetAngle;

            Vector2 direction = AngleToDirection(childAngle);
            Vector2 childPosition = parentPosition + direction * nodeSpacing;

            if (!nodePositions.ContainsKey(child))
                nodePositions.Add(child, childPosition);

            CalculateBranchRecursive(
                child,
                childPosition,
                branchAngle,
                depth + 1,
                path
            );
        }

        path.Remove(parent.Id);
    }

    private void SpawnNodes()
    {
        foreach (KeyValuePair<UpgradeNodeData, Vector2> pair in nodePositions)
        {
            UpgradeNodeData node = pair.Key;
            Vector2 position = pair.Value;

            UpgradeNodeView view = Instantiate(nodeViewPrefab, nodesRoot);
            view.Setup(treeSystem, node);

            if (view.RectTransform != null)
                view.RectTransform.anchoredPosition = position;

            nodeViews.Add(node, view);
        }
    }

    private void SpawnArrows()
    {
        if (arrowViewPrefab == null)
            return;

        foreach (KeyValuePair<UpgradeNodeData, Vector2> pair in nodePositions)
        {
            UpgradeNodeData child = pair.Key;

            if (child == null || child.IsRoot)
                continue;

            UpgradeNodeData parent = child.Parent;

            if (parent == null)
                continue;

            if (!nodePositions.TryGetValue(parent, out Vector2 parentPosition))
                continue;

            Vector2 childPosition = pair.Value;

            UpgradeArrowView arrow = Instantiate(arrowViewPrefab, arrowsRoot);
            arrow.Setup(parentPosition, childPosition);

            arrowBindings.Add(new ArrowBinding(parent, child, arrow));
        }
    }

    private void RefreshAll()
    {
        foreach (UpgradeNodeView view in nodeViews.Values)
        {
            if (view != null)
                view.Refresh();
        }

        foreach (ArrowBinding binding in arrowBindings)
        {
            if (binding.arrow == null)
                continue;

            bool parentUnlocked = treeSystem.IsUnlocked(binding.parent);
            bool childUnlocked = treeSystem.IsUnlocked(binding.child);
            bool childCanUnlock = treeSystem.CanUnlock(binding.child);

            binding.arrow.RefreshState(parentUnlocked, childUnlocked, childCanUnlock);
        }
    }

    private Vector2 AngleToDirection(float angle)
    {
        float rad = angle * Mathf.Deg2Rad;

        return new Vector2(
            Mathf.Cos(rad),
            Mathf.Sin(rad)
        );
    }

    private void Clear()
    {
        nodeViews.Clear();
        nodePositions.Clear();
        arrowBindings.Clear();

        if (nodesRoot != null)
        {
            for (int i = nodesRoot.childCount - 1; i >= 0; i--)
                Destroy(nodesRoot.GetChild(i).gameObject);
        }

        if (arrowsRoot != null)
        {
            for (int i = arrowsRoot.childCount - 1; i >= 0; i--)
                Destroy(arrowsRoot.GetChild(i).gameObject);
        }
    }

    private readonly struct ArrowBinding
    {
        public readonly UpgradeNodeData parent;
        public readonly UpgradeNodeData child;
        public readonly UpgradeArrowView arrow;

        public ArrowBinding(UpgradeNodeData parent, UpgradeNodeData child, UpgradeArrowView arrow)
        {
            this.parent = parent;
            this.child = child;
            this.arrow = arrow;
        }
    }
}