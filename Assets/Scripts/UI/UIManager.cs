using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Serializable]
    private class UIEntry
    {
        public string id;
        public GameObject panel;
    }

    [Header("UI Panels")]
    [SerializeField] private List<UIEntry> uiEntries = new();

    [SerializeField] private bool hideAllOnStart = true;

    private readonly Dictionary<string, GameObject> uiDict = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Initialize();
    }

    private void Initialize()
    {
        uiDict.Clear();

        foreach (UIEntry entry in uiEntries)
        {
            if (entry == null)
                continue;

            if (string.IsNullOrWhiteSpace(entry.id))
                continue;

            if (entry.panel == null)
                continue;

            if (uiDict.ContainsKey(entry.id))
                continue;

            uiDict.Add(entry.id, entry.panel);

            if (hideAllOnStart)
            {
                entry.panel.SetActive(false);
            }
        }
    }

    public void Toggle(string id)
    {
        if (!uiDict.TryGetValue(id, out GameObject panel))
        {
            return;
        }

        panel.SetActive(!panel.activeSelf);
    }

    public void Button_Toggle(string id)
    {
        Toggle(id);
    }
}