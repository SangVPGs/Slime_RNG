using TMPro;
using UnityEngine;

public class RebirthUI : MonoBehaviour
{
    private RebirthSystem rebirthSystem => RebirthSystem.Instance;

    [Header("Texts")]
    [SerializeField] private TMP_Text mapTitleText;
    [SerializeField] private TMP_Text goldTitleText;
    [SerializeField] private TMP_Text luckTitleText;
    [SerializeField] private TMP_Text costText;

    private void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        int currentMapLevel = MapProgressSave.LoadCurrentMapLevel();

        int currentGold = GameManager.Instance != null ? GameManager.Instance.Gold : 0;

        float currentLuck = PlayerStatContext.Instance != null ? PlayerStatContext.Instance.GetFinalStat(UpgradeStatType.Luck, 1f) : 1f;

        float nextLuck = rebirthSystem != null ? currentLuck * rebirthSystem.LuckMultiplierPerRebirth : currentLuck;

        if (mapTitleText != null)
            mapTitleText.text = $"Map: Lv {currentMapLevel} => Lv 1";

        if (goldTitleText != null)
            goldTitleText.text = $"Gold: {currentGold} => 0";

        if (luckTitleText != null)
            luckTitleText.text = $"Luck: {currentLuck:0.###} => {nextLuck:0.###}";

        if (costText != null && rebirthSystem != null)
            costText.text = $"Cost: {rebirthSystem.CurrentGoldCost:N0} Gold";
    }
}