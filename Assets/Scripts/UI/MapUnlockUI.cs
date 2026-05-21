using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapUnlockUI : MonoBehaviour
{
    [SerializeField] private Button unlockButton;
    [SerializeField] private TMP_Text costText;

    private MapChunk owner;

    public void Initialize(MapChunk mapChunk, int unlockCost)
    {
        owner = mapChunk;

        if (costText != null)
            costText.text = unlockCost.ToString();

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