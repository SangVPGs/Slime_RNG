using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    private long gold;

    public long Gold => gold;

    private const string GoldKey = "Gold";

    public event Action OnGoldChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadGold();
    }

    public void AddGold(long amount)
    {
        if (amount < 0)
            return;

        float multiplier = 1f;

        //if (PlayerStatContext.Instance != null)
        //{
        //    multiplier = PlayerStatContext.Instance.GetFinalStat(UpgradeStatType.GoldGain, 1f);
        //}

        int finalAmount = Mathf.RoundToInt(amount * multiplier);

        gold += finalAmount;

        SaveGold();
    }

    public bool SpendGold(long amount)
    {
        if (gold < amount)
            return false;

        gold -= amount;

        SaveGold();

        return true;
    }

    public bool HasEnoughGold(long amount)
    {
        return gold >= amount;
    }

    private void SaveGold()
    {
        PlayerPrefsUtility.SetLong(GoldKey, gold);
        PlayerPrefs.Save();
        OnGoldChanged?.Invoke();
    }

    private void LoadGold()
    {
        gold = PlayerPrefsUtility.GetLong(GoldKey, 0);
        OnGoldChanged?.Invoke();
    }

    public void ResetGold()
    {
        gold = 0;

        PlayerPrefs.DeleteKey(GoldKey);
        PlayerPrefs.Save();

        OnGoldChanged?.Invoke();
    }
}