using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    private int gold;

    public int Gold => gold;

    private const string GoldKey = "Gold";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        LoadGold();
    }

    public void AddGold(int amount)
    {
        if (amount <= 0)
            return;

        gold += amount;

        SaveGold();
    }

    public bool SpendGold(int amount)
    {
        if (gold < amount)
            return false;

        gold -= amount;

        SaveGold();

        return true;
    }

    public bool HasEnoughGold(int amount)
    {
        return gold >= amount;
    }

    private void SaveGold()
    {
        PlayerPrefs.SetInt(GoldKey, gold);
        PlayerPrefs.Save();
    }

    private void LoadGold()
    {
        gold = PlayerPrefs.GetInt(GoldKey, 0);
    }

    public void ClearGold()
    {
        gold = 0;
        SaveGold();
    }
}