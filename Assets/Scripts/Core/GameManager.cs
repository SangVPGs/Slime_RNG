using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    private double gold;
    public double Gold => gold;
    private const string GoldKey = "Gold";
    public event Action OnGoldChanged;


    private double poon;
    public double Poon => poon;
    private const string PoonKey = "Poon";
    public event Action OnPoonChanged;

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
        LoadPoon();
    }

    public void AddGold(double amount)
    {
        if (amount < 0)
            return;

        float multiplier = 1f;

        //if (PlayerStatContext.Instance != null)
        //{
        //    multiplier = PlayerStatContext.Instance.GetFinalStat(UpgradeStatType.GoldGain, 1f);
        //}

        double finalAmount = Math.Round(amount * multiplier);

        gold += finalAmount;

        SaveGold();
    }

    public bool SpendGold(double amount)
    {
        if (gold < amount)
            return false;

        gold -= amount;

        SaveGold();

        return true;
    }

    public bool HasEnoughGold(double amount)
    {
        return gold >= amount;
    }

    private void SaveGold()
    {
        PlayerPrefsUtility.SetDouble(GoldKey, gold);
        PlayerPrefs.Save();
        OnGoldChanged?.Invoke();
    }

    private void LoadGold()
    {
        gold = PlayerPrefsUtility.GetDouble(GoldKey, 0);
        OnGoldChanged?.Invoke();
    }

    public void AddPoon(double amount)
    {
        if (amount <= 0)
            return;

        poon += amount;

        SavePoon();
    }

    public bool SpendPoon(double amount)
    {
        if (poon < amount)
            return false;

        poon -= amount;

        SavePoon();

        return true;
    }

    public bool HasEnoughPoon(double amount)
    {
        return poon >= amount;
    }

    private void SavePoon()
    {
        PlayerPrefsUtility.SetDouble(PoonKey, poon);
        PlayerPrefs.Save();

        OnPoonChanged?.Invoke();
    }

    private void LoadPoon()
    {
        poon = PlayerPrefsUtility.GetDouble(PoonKey, 0);

        OnPoonChanged?.Invoke();
    }

    public void ResetGold()
    {
        gold = 0;

        PlayerPrefs.DeleteKey(GoldKey);
        PlayerPrefs.Save();

        OnGoldChanged?.Invoke();
    }
}