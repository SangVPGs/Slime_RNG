using System;
using System.Collections.Generic;
using UnityEngine;

public enum UIPanelMode
{
    Screen,
    Overlay
}

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Serializable]
    private class UIEntry
    {
        public string id;
        public GameObject panel;
        public CanvasGroup canvasGroup;
        public UIPanelMode mode = UIPanelMode.Screen;
    }

    [Header("Main Panel")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private CanvasGroup mainCanvasGroup;
    [SerializeField] private bool showMainOnStart = true;

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

        InitializeMainPanel();
        InitializePanels();

        if (showMainOnStart)
            ShowMainPanel();
        else
            HideMainPanel();
    }

    private void InitializeMainPanel()
    {
        if (mainPanel == null)
            return;

        if (mainCanvasGroup == null)
        {
            mainCanvasGroup = mainPanel.GetComponent<CanvasGroup>();

            if (mainCanvasGroup == null)
                mainCanvasGroup = mainPanel.AddComponent<CanvasGroup>();
        }

        mainPanel.SetActive(true);
    }

    private void InitializePanels()
    {
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
                    entry.canvasGroup = entry.panel.AddComponent<CanvasGroup>();
            }

            if (uiDict.ContainsKey(entry.id))
                continue;

            uiDict.Add(entry.id, entry);

            entry.panel.SetActive(true);

            if (hideAllOnStart)
                SetVisible(entry, false);
        }
    }

    public void Show(string id)
    {
        if (!uiDict.TryGetValue(id, out UIEntry entry))
            return;

        if (entry.mode == UIPanelMode.Screen)
            HideMainPanel();

        SetVisible(entry, true);
    }

    public void Hide(string id)
    {
        if (!uiDict.TryGetValue(id, out UIEntry entry))
            return;

        SetVisible(entry, false);

        if (entry.mode == UIPanelMode.Screen && !HasAnyScreenPanelVisible())
            ShowMainPanel();
    }

    public void Toggle(string id)
    {
        if (!uiDict.TryGetValue(id, out UIEntry entry))
            return;

        if (IsVisible(entry))
            Hide(id);
        else
            Show(id);
    }

    private bool HasAnyScreenPanelVisible()
    {
        foreach (UIEntry entry in uiDict.Values)
        {
            if (entry.mode != UIPanelMode.Screen)
                continue;

            if (IsVisible(entry))
                return true;
        }

        return false;
    }

    private void ShowMainPanel()
    {
        if (mainPanel == null || mainCanvasGroup == null)
            return;

        SetVisible(mainPanel, mainCanvasGroup, true);
    }

    private void HideMainPanel()
    {
        if (mainPanel == null || mainCanvasGroup == null)
            return;

        SetVisible(mainPanel, mainCanvasGroup, false);
    }

    private bool IsVisible(UIEntry entry)
    {
        return entry.canvasGroup != null && entry.canvasGroup.alpha > 0.5f;
    }

    private void SetVisible(UIEntry entry, bool visible)
    {
        if (entry == null || entry.panel == null || entry.canvasGroup == null)
            return;

        SetVisible(entry.panel, entry.canvasGroup, visible);
    }

    private void SetVisible(GameObject panel, CanvasGroup canvasGroup, bool visible)
    {
        panel.SetActive(true);

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }

    public void Button_Show(string id)
    {
        Show(id);
    }

    public void Button_Hide(string id)
    {
        Hide(id);
    }

    public void Button_Toggle(string id)
    {
        Toggle(id);
    }
}