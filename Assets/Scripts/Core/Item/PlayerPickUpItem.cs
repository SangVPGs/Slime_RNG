using System.Collections.Generic;
using UnityEngine;

// Chưa có hiệu ứng nhặt

[RequireComponent(typeof(Collider))]
public class PlayerPickupItem : MonoBehaviour
{
    private InventorySystem Inventory => InventorySystem.Instance;

    [Header("Auto Pickup")]
    [SerializeField] private float pickupInterval = 0.1f;

    private readonly List<ItemPickup> nearbyItems = new();

    private Collider detectorCollider;
    private float nextPickupTime;

    private void Awake()
    {
        detectorCollider = GetComponent<Collider>();
        detectorCollider.isTrigger = true;
    }

    private void OnDisable()
    {
        nearbyItems.Clear();
        nextPickupTime = 0f;
    }

    private void Update()
    {
        CleanInvalidItems();

        if (Time.time < nextPickupTime)
            return;

        nextPickupTime = Time.time + pickupInterval;

        TryPickupNearbyItems();
    }

    private void OnTriggerEnter(Collider other)
    {
        ItemPickup item = other.GetComponentInParent<ItemPickup>();

        if (item == null)
            return;

        RegisterItem(item);
    }

    private void OnTriggerExit(Collider other)
    {
        ItemPickup item = other.GetComponentInParent<ItemPickup>();

        if (item == null)
            return;

        UnregisterItem(item);
    }

    private void RegisterItem(ItemPickup item)
    {
        if (item == null)
            return;

        if (nearbyItems.Contains(item))
            return;

        nearbyItems.Add(item);

        TryPickupItem(item);
    }

    private void UnregisterItem(ItemPickup item)
    {
        if (item == null)
            return;

        nearbyItems.Remove(item);
    }

    private void TryPickupNearbyItems()
    {
        if (Inventory == null)
            return;

        for (int i = nearbyItems.Count - 1; i >= 0; i--)
        {
            ItemPickup item = nearbyItems[i];

            if (item == null)
            {
                nearbyItems.RemoveAt(i);
                continue;
            }

            bool success = TryPickupItem(item);

            if (success)
                nearbyItems.RemoveAt(i);
        }
    }

    private bool TryPickupItem(ItemPickup item)
    {
        if (item == null)
            return false;

        if (Inventory == null)
            return false;

        bool success = item.Pickup(Inventory);

        return success;
    }

    private void CleanInvalidItems()
    {
        for (int i = nearbyItems.Count - 1; i >= 0; i--)
        {
            if (nearbyItems[i] == null)
                nearbyItems.RemoveAt(i);
        }
    }
}