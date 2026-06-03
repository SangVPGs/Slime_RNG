using UnityEngine;

public static class MapUnlockSave
{
    private const string KeyPrefix = "UnlockedMap_";

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
        PlayerPrefs.Save();
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