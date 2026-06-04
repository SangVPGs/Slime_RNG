using System.Collections.Generic;
using UnityEngine;

public class MapChunk : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform modelParent;
    [SerializeField] private MapUnlockUI unlockUI;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private Transform checkPoint;

    public Transform StartPoint => startPoint;
    public Transform EndPoint => endPoint;
    public Transform CheckPoint => checkPoint;

    public int Level => runtimeLevel;
    public int UnlockCost => unlockCost;

    public IReadOnlyList<SlimeUnitData> Enemies =>
        data != null ? data.enemies : null;

    private MapData data;
    private int runtimeLevel;
    private int unlockCost;
    private bool isUnlocked;

    public void Initialize(MapData mapData, int level, int cost)
    {
        data = mapData;
        runtimeLevel = level;
        unlockCost = cost;

        gameObject.name = $"Map_Level_{runtimeLevel}";

        SpawnModel();
        LoadUnlockState();
        SetupUnlockUI();
    }

    private void SpawnModel()
    {
        if (data == null || data.mapPrefab == null || modelParent == null)
            return;

        GameObject model = Instantiate(data.mapPrefab, modelParent);

        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        model.transform.localScale = Vector3.one;
    }

    private void LoadUnlockState()
    {
        isUnlocked = MapUnlockSave.IsUnlocked(runtimeLevel);
    }

    private void SetupUnlockUI()
    {
        if (unlockUI == null)
            return;

        unlockUI.Initialize(this, unlockCost);
        unlockUI.gameObject.SetActive(!isUnlocked);
    }

    public bool TryUnlock()
    {
        if (isUnlocked)
            return true;

        if (GameManager.Instance == null)
            return false;

        if (!GameManager.Instance.SpendGold(unlockCost))
            return false;

        Unlock();
        return true;
    }

    private void Unlock()
    {
        isUnlocked = true;

        MapUnlockSave.SaveUnlocked(runtimeLevel);

        if (unlockUI != null)
            unlockUI.gameObject.SetActive(false);
    }
}