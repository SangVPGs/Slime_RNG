using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PetInventoryUIItem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text combatPowerText;

    [Header("Button")]
    [SerializeField] private Button button;
    [SerializeField] private Image buttonImage;
    [SerializeField] private TMP_Text buttonText;

    [Header("Button Color")]
    [SerializeField] private Color addColor = Color.green;
    [SerializeField] private Color removeColor = Color.red;

    private InventorySystem.PetInventoryEntry entry;
    private Action<InventorySystem.PetInventoryEntry> onClicked;

    public void SetupPetInventory(
        InventorySystem.PetInventoryEntry inventoryEntry,
        Action<InventorySystem.PetInventoryEntry> clickCallback,
        bool showButton)
    {
        SetupInfo(inventoryEntry);

        onClicked = clickCallback;

        SetupButton(showButton, "Add", addColor);
    }

    public void SetupParty(
        InventorySystem.PetInventoryEntry partyEntry,
        Action<InventorySystem.PetInventoryEntry> clickCallback,
        bool showButton)
    {
        SetupInfo(partyEntry);

        onClicked = clickCallback;

        SetupButton(showButton, "Remove", removeColor);
    }

    public void SetupUseItem(
        InventorySystem.PetInventoryEntry partyEntry,
        Action<InventorySystem.PetInventoryEntry> clickCallback,
        bool showButton)
    {
        SetupInfo(partyEntry);

        onClicked = clickCallback;

        SetupButton(showButton, "Use", addColor);
    }

    private void SetupInfo(InventorySystem.PetInventoryEntry inventoryEntry)
    {
        entry = inventoryEntry;

        if (entry == null || entry.petData == null)
        {
            ClearInfo();
            return;
        }

        PetUnitData petData = entry.petData;

        if (iconImage != null)
        {
            iconImage.sprite = petData.icon;
            iconImage.color = Color.white;
        }

        if (nameText != null)
        {
            nameText.text = petData.unitName;
            nameText.color = GetRarityColor(petData.rarity);
        }

        if (levelText != null)
            levelText.text = $"Lv.{entry.level}";

        if (combatPowerText != null)
        {
            long cp = PetUnit.CalculateCombatPower(entry.petData, entry.level);
            combatPowerText.text = $"CP {cp}";
        }
    }

    private void ClearInfo()
    {
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.color = Color.white;
        }

        if (nameText != null)
            nameText.text = string.Empty;

        if (levelText != null)
            levelText.text = string.Empty;

        if (combatPowerText != null)
            combatPowerText.text = string.Empty;

        SetupButton(false, string.Empty, Color.white);
    }

    private void SetupButton(bool visible, string text, Color color)
    {
        if (button == null)
            return;

        button.gameObject.SetActive(visible);
        button.onClick.RemoveAllListeners();

        if (!visible)
            return;

        button.onClick.AddListener(OnButtonClicked);

        if (buttonImage != null)
            buttonImage.color = color;

        if (buttonText != null)
            buttonText.text = text;
    }

    private void OnButtonClicked()
    {
        if (entry == null)
            return;

        onClicked?.Invoke(entry);
    }

    private Color GetRarityColor(PetRarity rarity)
    {
        return rarity switch
        {
            PetRarity.Common => Color.gray,
            PetRarity.Uncommon => Color.green,
            PetRarity.Rare => Color.blue,
            PetRarity.Epic => Color.magenta,
            PetRarity.Legendary => Color.yellow,
            _ => Color.white
        };
    }
}