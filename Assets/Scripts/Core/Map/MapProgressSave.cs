using UnityEngine;

public static class MapProgressSave
{
    private const string CurrentMapLevelKey = "CurrentMapLevel";

    public static void SaveCurrentMapLevel(int level)
    {
        if (level < 0)
            return;

        PlayerPrefs.SetInt(CurrentMapLevelKey, level);
        PlayerPrefs.Save();
    }

    public static int LoadCurrentMapLevel(int defaultLevel = 1)
    {
        return PlayerPrefs.GetInt(CurrentMapLevelKey, defaultLevel);
    }

    public static void ResetCurrentMapLevel()
    {
        PlayerPrefs.DeleteKey(CurrentMapLevelKey);
        PlayerPrefs.Save();
    }
}