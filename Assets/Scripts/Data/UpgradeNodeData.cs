using UnityEngine;

public class UpgradeNodeData : ScriptableObject
{
    private string id;
    public string Id => id;

    [SerializeField] private string upgradeName;
    [SerializeField] Sprite icon;
    [SerializeField] private int upgradeCost;
    [SerializeField] private UpgradeNodeData upgradeNodeParent;
}
