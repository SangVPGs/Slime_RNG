using UnityEngine;

public class ItemDropSystem : MonoBehaviour
{
    public static ItemDropSystem Instance { get; private set; }

    [Header("Database")]
    [SerializeField] private ItemDatabase itemDatabase;

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

        ItemData itemData = itemDatabase.GetRandomItem();

        if (itemData == null)
            return;

        int amount = Random.Range(
            Mathf.Max(1, minAmount),
            Mathf.Max(1, maxAmount) + 1
        );

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