using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class PlayerPickupItem : MonoBehaviour
{
    private InventorySystem inventorySystem => InventorySystem.Instance;

    [Header("UI")]
    [SerializeField] private Button pickupButton;

    [Header("Settings")]
    [SerializeField] private Key pickupKey = Key.F;

    private readonly List<ItemPickup> nearbyItems = new();

    private ItemPickup currentItem;
    private Collider detectorCollider;

    private void Awake()
    {
        detectorCollider = GetComponent<Collider>();
        detectorCollider.isTrigger = true;

        if (pickupButton != null)
        {
            pickupButton.onClick.RemoveListener(PickupCurrentItem);
            pickupButton.onClick.AddListener(PickupCurrentItem);
            pickupButton.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        nearbyItems.Clear();
        currentItem = null;
        UpdatePickupButton();
    }

    private void Update()
    {
        CleanInvalidItems();
        UpdateCurrentItem();
        UpdatePickupButton();

        if (Keyboard.current != null &&
            Keyboard.current[pickupKey].wasPressedThisFrame)
        {
            PickupCurrentItem();
        }
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
        UpdateCurrentItem();
        UpdatePickupButton();
    }

    private void UnregisterItem(ItemPickup item)
    {
        if (item == null)
            return;

        nearbyItems.Remove(item);

        if (currentItem == item)
            currentItem = null;

        UpdateCurrentItem();
        UpdatePickupButton();
    }

    private void PickupCurrentItem()
    {
        if (currentItem == null || inventorySystem == null)
            return;

        ItemPickup itemToPickup = currentItem;

        nearbyItems.Remove(itemToPickup);
        currentItem = null;

        bool success = itemToPickup.Pickup(inventorySystem);

        if (!success)
            nearbyItems.Add(itemToPickup);

        UpdateCurrentItem();
        UpdatePickupButton();
    }

    private void UpdateCurrentItem()
    {
        currentItem = GetNearestItem();
    }

    private ItemPickup GetNearestItem()
    {
        ItemPickup nearest = null;
        float nearestDistanceSqr = float.MaxValue;
        Vector3 origin = transform.position;

        foreach (ItemPickup item in nearbyItems)
        {
            if (item == null)
                continue;

            float distanceSqr =
                (item.transform.position - origin).sqrMagnitude;

            if (distanceSqr < nearestDistanceSqr)
            {
                nearestDistanceSqr = distanceSqr;
                nearest = item;
            }
        }

        return nearest;
    }

    private void CleanInvalidItems()
    {
        for (int i = nearbyItems.Count - 1; i >= 0; i--)
        {
            if (nearbyItems[i] == null)
                nearbyItems.RemoveAt(i);
        }
    }

    private void UpdatePickupButton()
    {
        if (pickupButton == null)
            return;

        pickupButton.gameObject.SetActive(currentItem != null);
    }
}