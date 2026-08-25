using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapUnlockUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button unlockButton;
    [SerializeField] private TMP_Text costText;

    private MapChunk owner;

    public void Initialize(MapChunk mapChunk, double unlockCost)
    {
        owner = mapChunk;

        if (costText != null)
            costText.text = $"{NumberFormatter.Format(unlockCost)}";

        if (unlockButton != null)
        {
            unlockButton.onClick.RemoveAllListeners();
            unlockButton.onClick.AddListener(OnUnlockClicked);
        }
    }

    private void OnUnlockClicked()
    {
        if (owner == null)
            return;

        owner.TryUnlock();
    }
}