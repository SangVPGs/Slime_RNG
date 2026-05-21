using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaUI : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TMP_Text resultText;

    [Header("Auto Roll Button")]
    [SerializeField] private Image autoRollButtonImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color rollingColor = Color.green;

    public void ShowRollingPet(PetUnitData pet)
    {
        if (resultText == null || pet == null)
            return;

        ChangeTextColor(pet.rarity);
        resultText.text = $"{pet.unitName} ({pet.rarity})";
    }

    public void ShowFinalPet(PetUnitData pet)
    {
        if (resultText == null || pet == null)
            return;

        ChangeTextColor(pet.rarity);
        resultText.text = $"Got: {pet.unitName} ({pet.rarity})";
    }

    public void SetAutoRollVisual(bool isRolling)
    {
        if (autoRollButtonImage == null)
            return;

        autoRollButtonImage.color = isRolling ? rollingColor : normalColor;
    }

    private void ChangeTextColor(PetRarity rarity)
    {
        switch (rarity)
        {
            case PetRarity.Common:
                resultText.color = Color.white;
                break;
            case PetRarity.Uncommon:
                resultText.color = Color.green;
                break;
            case PetRarity.Rare:
                resultText.color = Color.blue;
                break;
            case PetRarity.Epic:
                resultText.color = Color.magenta;
                break;
            case PetRarity.Legendary:
                resultText.color = Color.yellow;
                break;
            default:
                resultText.color = Color.white;
                break;
        }
    }
}