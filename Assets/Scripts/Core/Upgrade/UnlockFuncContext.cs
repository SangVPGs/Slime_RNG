using System;
using System.Collections.Generic;
using UnityEngine;

public class UnlockFuncContext : MonoBehaviour
{
    public static UnlockFuncContext Instance { get; private set; }

    private readonly HashSet<string> unlockedFunctions = new();

    public event Action<string> OnFunctionUnlocked;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Unlock(string functionId)
    {
        if (string.IsNullOrWhiteSpace(functionId))
            return;

        bool added = unlockedFunctions.Add(functionId);

        if (!added)
            return;

        OnFunctionUnlocked?.Invoke(functionId);
    }

    public bool IsUnlocked(string functionId)
    {
        return unlockedFunctions.Contains(functionId);
    }

    public void Clear()
    {
        unlockedFunctions.Clear();
    }
}