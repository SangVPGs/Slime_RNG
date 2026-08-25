using UnityEngine;

public static class NumberFormatter
{
    private static readonly string[] Suffixes =
    {
        "",
        "K",
        "M",
        "B",
        "T",
        "Qa",
        "Qi",
        "Sx",
        "Sp",
        "Oc",
        "No",
        "Dc",
        "Ud",
        "Dd",
        "Td",
        "Qad",
        "Qid",
    };

    public static string Format(double value)
    {
        if (value < 1000)
            return value.ToString();

        double number = value;
        int suffixIndex = 0;

        while (number >= 1000d && suffixIndex < Suffixes.Length - 1)
        {
            number /= 1000d;
            suffixIndex++;
        }

        if (number >= 100)
            return $"{number:F0}{Suffixes[suffixIndex]}";

        if (number >= 10)
            return $"{number:F1}{Suffixes[suffixIndex]}";

        return $"{number:F2}{Suffixes[suffixIndex]}";
    }
}