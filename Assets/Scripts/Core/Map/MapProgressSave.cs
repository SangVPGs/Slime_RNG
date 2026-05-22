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

        Debug.Log($"Saved current map level: {level}");
    }

    public static int LoadCurrentMapLevel(int defaultLevel = 0)
    {
        return PlayerPrefs.GetInt(CurrentMapLevelKey, defaultLevel);
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(CurrentMapLevelKey);
        PlayerPrefs.Save();
    }
}