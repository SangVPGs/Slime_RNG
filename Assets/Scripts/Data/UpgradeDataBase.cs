using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Database/Upgrade Database")]
public class UpgradeDatabase : ScriptableObject
{
    [SerializeField] private List<UpgradeNodeData> nodes = new();

    public IReadOnlyList<UpgradeNodeData> Nodes => nodes;

#if UNITY_EDITOR
    private void OnValidate()
    {
        HashSet<string> ids = new();

        foreach (UpgradeNodeData node in nodes)
        {
            if (node == null)
                continue;

            if (string.IsNullOrWhiteSpace(node.Id))
            {
                Debug.LogWarning($"UpgradeDatabase has node with empty id: {node.name}", this);
                continue;
            }

            if (!ids.Add(node.Id))
                Debug.LogError($"Duplicate upgrade node id in database: {node.Id}", this);
        }
    }
#endif
}