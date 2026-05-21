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
        public CanvasGroup canvasGroup;
    }

    [Header("UI Panels")]
    [SerializeField] private List<UIEntry> uiEntries = new();

    [SerializeField] private bool hideAllOnStart = true;

    private readonly Dictionary<string, UIEntry> uiDict = new();

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

            if (entry.canvasGroup == null)
            {
                entry.canvasGroup = entry.panel.GetComponent<CanvasGroup>();

                if (entry.canvasGroup == null)
                {
                    entry.canvasGroup = entry.panel.AddComponent<CanvasGroup>();
                }
            }

            if (uiDict.ContainsKey(entry.id))
                continue;

            uiDict.Add(entry.id, entry);

            entry.panel.SetActive(true);

            if (hideAllOnStart)
            {
                SetVisible(entry, false);
            }
        }
    }

    public void Show(string id)
    {
        if (!uiDict.TryGetValue(id, out UIEntry entry))
            return;

        SetVisible(entry, true);
    }

    public void Hide(string id)
    {
        if (!uiDict.TryGetValue(id, out UIEntry entry))
            return;

        SetVisible(entry, false);
    }

    public void Toggle(string id)
    {
        if (!uiDict.TryGetValue(id, out UIEntry entry))
            return;

        bool isVisible = entry.canvasGroup.alpha > 0.5f;

        SetVisible(entry, !isVisible);
    }

    private void SetVisible(UIEntry entry, bool visible)
    {
        entry.panel.SetActive(true);

        entry.canvasGroup.alpha = visible ? 1f : 0f;
        entry.canvasGroup.interactable = visible;
        entry.canvasGroup.blocksRaycasts = visible;
    }

    public void Button_Toggle(string id)
    {
        Toggle(id);
    }

    public void Button_Show(string id)
    {
        Show(id);
    }

    public void Button_Hide(string id)
    {
        Hide(id);
    }
}