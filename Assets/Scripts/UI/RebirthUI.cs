using TMPro;
using UnityEngine;

public class RebirthUI : MonoBehaviour
{
    private RebirthSystem rebirthSystem => RebirthSystem.Instance;

    [Header("Texts")]
    [SerializeField] private TMP_Text currentMapText;
    [SerializeField] private TMP_Text currentGoldText;
    [SerializeField] private TMP_Text currentLuckText;
    [SerializeField] private TMP_Text newLuckText;
    [SerializeField] private TMP_Text costText;

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGoldChanged += Refresh;
            GameManager.Instance.OnPoonChanged += Refresh;
            MapUnlockSave.OnMapUnlocked += Refresh;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGoldChanged -= Refresh;
            GameManager.Instance.OnPoonChanged -= Refresh;
            MapUnlockSave.OnMapUnlocked -= Refresh;
        }
    }

    public void Refresh()
    {
        int currentMapLevel = MapUnlockSave.GetHighestUnlockedMap();

        double currentGold;
        double currentPoon;

        if (GameManager.Instance != null)
        {
            currentGold = GameManager.Instance.Gold;
            currentPoon = GameManager.Instance.Poon;
        }
        else
        {
            currentGold = 0;
            currentPoon = 0;
        }
        

        float currentLuck = PlayerStatContext.Instance != null ? PlayerStatContext.Instance.GetFinalStat(UpgradeStatType.Luck, 1f) : 1f;

        float nextLuck = rebirthSystem != null ? currentLuck * rebirthSystem.LuckMultiplierPerRebirth : currentLuck;

        if (currentMapText != null)
            currentMapText.text = $"{currentMapLevel}";

        if (currentGoldText != null)
            currentGoldText.text = $"{NumberFormatter.Format(currentGold)}";

        if (currentLuckText != null)
            currentLuckText.text = $"{currentLuck:0.###}";

        if(newLuckText != null)
            newLuckText.text = $"{nextLuck:0.###}";

        if (costText != null && rebirthSystem != null)
            costText.text = $"{NumberFormatter.Format(currentPoon)} / {NumberFormatter.Format(rebirthSystem.CurrentPoonCost)}";
    }
}