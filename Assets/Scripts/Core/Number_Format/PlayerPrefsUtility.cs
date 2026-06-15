using UnityEngine;

public static class PlayerPrefsUtility
{
    public static void SetDouble(string key, double value)
    {
        PlayerPrefs.SetString(key, value.ToString());
    }

    public static double GetDouble(string key, double defaultValue = 0)
    {
        string value = PlayerPrefs.GetString(
            key,
            defaultValue.ToString());

        if (double.TryParse(value, out double result))
            return result;

        return defaultValue;
    }
}