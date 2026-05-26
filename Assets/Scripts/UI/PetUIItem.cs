using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PetUIItem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;

    [Header("Button")]
    [SerializeField] private Button button;
    [SerializeField] private Image buttonImage;
    [SerializeField] private TMP_Text buttonText;

    private Color addColor = Color.green;
    private Color removeColor = Color.red;

    private PetUnitData petData;
    private Action<PetUnitData> onClicked;

    private bool showButton;

    public void SetupInventory(PetUnitData data, Action<PetUnitData> clickCallback, bool showButton)
    {
        SetupInfo(data);

        onClicked = clickCallback;

        SetupButton(showButton,"Add",addColor);
    }

    public void SetupParty(PetUnitData data,Action<PetUnitData> clickCallback, bool showButton)
    {
        SetupInfo(data);

        onClicked = clickCallback;

        SetupButton(showButton,"Remove",removeColor);
    }

    public void SetupIndex(PetUnitData data)
    {
        SetupInfo(data);
        button.gameObject.SetActive(false);
    }

    private void SetupInfo(PetUnitData data)
    {
        petData = data;

        if (petData == null)
            return;

        if (iconImage != null)
            iconImage.sprite = petData.icon;

        if (nameText != null)
        {
            nameText.text = petData.unitName;

            nameText.color =
                GetRarityColor(petData.rarity);
        }
    }

    private void SetupButton(
        bool visible,
        string text,
        Color color)
    {
        if (button == null)
            return;

        button.gameObject.SetActive(visible);

        if (!visible)
            return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnButtonClicked);

        if (buttonImage != null)
            buttonImage.color = color;

        if (buttonText != null)
            buttonText.text = text;
    }

    private void OnButtonClicked()
    {
        onClicked?.Invoke(petData);
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