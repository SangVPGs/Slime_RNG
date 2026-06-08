using TMPro;
using UnityEngine;

public class UpgradeDetailPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text descriptionText;

    public void Show(UpgradeNodeData node)
    {
        if (descriptionText == null)
            return;

        if (node == null)
        {
            descriptionText.text = string.Empty;
            return;
        }

        descriptionText.text = node.Description;
    }

    public void Clear()
    {
        if (descriptionText != null)
            descriptionText.text = string.Empty;
    }
}