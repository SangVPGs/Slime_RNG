using UnityEngine;

public class ItemDropSystem : MonoBehaviour
{
    public static ItemDropSystem Instance { get; private set; }

    [Header("Database")]
    [SerializeField] private ItemDatabase itemDatabase;

    [Header("Pickup")]
    [SerializeField] private ItemPickup itemPickupPrefab;

    [Header("Drop")]
    [Range(0f, 100f)]
    [SerializeField] private float dropChance = 30f;

    [Header("Amount")]
    [SerializeField] private int minAmount = 1;
    [SerializeField] private int maxAmount = 1;

    [Header("Spawn")]
    [SerializeField] private float dropRadius = 0.75f;
    [SerializeField] private float dropY = 0.2f;

    private UnlockItemContext UnlockContext => UnlockItemContext.Instance;

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

        ItemData itemData = itemDatabase.GetRandomDropItemByWeight(UnlockContext);

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