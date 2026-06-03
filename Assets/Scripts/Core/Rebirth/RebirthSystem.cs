using UnityEngine;
using UnityEngine.SceneManagement;

public class RebirthSystem : MonoBehaviour
{
    private const string RebirthCountKey = "Rebirth_Count";

    [Header("Cost")]
    [SerializeField, Min(0)] private int baseGoldCost = 1000;
    [SerializeField, Min(1f)] private float costMultiplierPerRebirth = 5f;

    [Header("Luck")]
    [SerializeField, Min(1f)] private float luckMultiplierPerRebirth = 1.18f;

    [Header("Reset")]
    [SerializeField] private int maxMapLevelToClear = 999;

    public int RebirthCount => PlayerPrefs.GetInt(RebirthCountKey, 0);

    public int CurrentGoldCost
    {
        get
        {
            double cost = baseGoldCost * System.Math.Pow(
                costMultiplierPerRebirth,
                RebirthCount
            );

            if (cost > int.MaxValue)
                return int.MaxValue;

            return Mathf.RoundToInt((float)cost);
        }
    }

    public float LuckMultiplierPerRebirth => luckMultiplierPerRebirth;
    public static RebirthSystem Instance { get; set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Rebirth()
    {
        if (!CanRebirth())
            return;

        SpendCost();

        ResetProgress();
        AddLuck();
        AddRebirthCount();

        PlayerPrefs.Save();

        RestartGame();
    }

    private bool CanRebirth()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager is missing.");
            return false;
        }

        if (!GameManager.Instance.HasEnoughGold(CurrentGoldCost))
        {
            Debug.LogWarning($"Not enough gold. Need: {CurrentGoldCost}");
            return false;
        }

        if (PlayerStatContext.Instance == null)
        {
            Debug.LogError("PlayerStatContext is missing.");
            return false;
        }

        return true;
    }

    private void SpendCost()
    {
        GameManager.Instance.SpendGold(CurrentGoldCost);
    }

    private void ResetProgress()
    {
        MapUnlockSave.ClearAllUnlocked(maxMapLevelToClear);
        MapProgressSave.ResetCurrentMapLevel();

        GameManager.Instance.ResetGold();
    }

    private void AddLuck()
    {
        PlayerStatContext.Instance.MultiplyRebirthLuck(
            luckMultiplierPerRebirth
        );
    }

    private void AddRebirthCount()
    {
        PlayerPrefs.SetInt(RebirthCountKey, RebirthCount + 1);
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}