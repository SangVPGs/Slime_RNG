using UnityEngine;

public class ItemDropSystem : MonoBehaviour
{
    public static ItemDropSystem Instance { get; private set; }

    [Header("Database")]
    [SerializeField] private ItemDatabase itemDatabase;

    [Header("Upgrade")]
    [SerializeField] private UpgradeTreeSystem upgradeTreeSystem;

    [Header("Pickup")]
    [SerializeField] private ItemPickup itemPickupPrefab;

    [Header("Drop Settings")]
    [Range(0f, 100f)]
    [SerializeField] private float dropChance = 30f;

    [SerializeField] private int minAmount = 1;
    [SerializeField] private int maxAmount = 1;

    [SerializeField] private float dropRadius = 0.75f;
    [SerializeField] private float dropY = 0.2f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void DropRandomItem(Vector3 position)
    {
        if (itemDatabase == null || itemPickupPrefab == null)
            return;

        float roll = Random.Range(0f, 100f);

        if (roll > dropChance)
            return;

        UpgradeContext context =
            upgradeTreeSystem != null
                ? upgradeTreeSystem.Context
                : null;

        ItemData itemData =
            itemDatabase.GetRandomUnlockedFood(context);

        if (itemData == null)
            return;

        int safeMin = Mathf.Max(1, minAmount);
        int safeMax = Mathf.Max(safeMin, maxAmount);

        int amount = Random.Range(safeMin, safeMax + 1);

        Vector2 randomCircle = Random.insideUnitCircle * dropRadius;

        Vector3 spawnPosition = position + new Vector3(
            randomCircle.x,
            dropY,
            randomCircle.y
        );

        ItemPickup pickup = Instantiate(
            itemPickupPrefab,
            spawnPosition,
            Quaternion.identity
        );

        pickup.SetData(itemData, amount);
    }
}