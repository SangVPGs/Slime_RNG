using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Database/Upgrade Database")]
public class UpgradeDatabase : ScriptableObject
{
    [SerializeField] private List<UpgradeNodeData> nodes = new();

    public IReadOnlyList<UpgradeNodeData> Nodes => nodes;
}