using UnityEngine;
using System;

public static class MapUnlockSave
{
    private const string KeyPrefix = "UnlockedMap_";
    private const string HighestUnlockedMapKey = "HighestUnlockedMap";

    public static event Action OnMapUnlocked;

    public static bool IsUnlocked(int level)
    {
        if (level == 1)
            return true;

        return PlayerPrefs.GetInt(GetKey(level), 0) == 1;
    }

    public static void SaveUnlocked(int level)
    {
        if (level <= 0)
            return;

        PlayerPrefs.SetInt(GetKey(level), 1);

        int currentHighest = PlayerPrefs.GetInt(HighestUnlockedMapKey, 1);

        if (level > currentHighest)
            PlayerPrefs.SetInt(HighestUnlockedMapKey, level);

        PlayerPrefs.Save();
        OnMapUnlocked?.Invoke();
    }

    public static int GetHighestUnlockedMap()
    {
        return PlayerPrefs.GetInt(HighestUnlockedMapKey, 1);
    }

    public static void ClearUnlocked(int level)
    {
        if (level <= 0)
            return;

        PlayerPrefs.DeleteKey(GetKey(level));
        PlayerPrefs.Save();
    }

    public static void ClearAllUnlocked(int maxLevel)
    {
        for (int level = 1; level <= maxLevel; level++)
        {
            PlayerPrefs.DeleteKey(GetKey(level));
        }

        PlayerPrefs.Save();
    }

    private static string GetKey(int level)
    {
        return $"{KeyPrefix}{level}";
    }
}