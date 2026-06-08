using UnityEngine;

public static class PlayerPrefsUtility
{
    public static void SetLong(string key, long value)
    {
        PlayerPrefs.SetString(key, value.ToString());
    }

    public static long GetLong(string key, long defaultValue = 0)
    {
        string value = PlayerPrefs.GetString(
            key,
            defaultValue.ToString());

        if (long.TryParse(value, out long result))
            return result;

        return defaultValue;
    }
}