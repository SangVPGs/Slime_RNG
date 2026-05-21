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

    public int Level => data != null ? data.level : -1;

    private MapData data;
    private bool isUnlocked;

    public void Initialize(MapData mapData)
    {
        data = mapData;

        gameObject.name = $"Map_Level_{data.level}";

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
        isUnlocked = MapUnlockSave.IsUnlocked(Level);
    }

    private void SetupUnlockUI()
    {
        if (unlockUI == null)
            return;

        unlockUI.Initialize(this, data.unlockCost);
        unlockUI.gameObject.SetActive(!isUnlocked);
    }

    public bool TryUnlock()
    {
        if (isUnlocked)
            return true;

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager missing.");
            return false;
        }

        if (!GameManager.Instance.SpendGold(data.unlockCost))
        {
            Debug.Log($"Not enough gold. Need: {data.unlockCost}");
            return false;
        }

        Unlock();
        return true;
    }

    private void Unlock()
    {
        isUnlocked = true;

        MapUnlockSave.SaveUnlocked(Level);

        if (unlockUI != null)
            unlockUI.gameObject.SetActive(false);

        Debug.Log($"Unlocked map level {Level}");
    }
}