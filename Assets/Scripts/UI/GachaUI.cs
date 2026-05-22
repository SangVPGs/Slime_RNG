using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaUI : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TMP_Text resultText;

    [Header("Roll Button")]
    [SerializeField] private Button rollButton;
    [SerializeField] private Image rollButtonImage;
    [SerializeField] private TMP_Text rollButtonText;

    [Header("Auto Roll Button")]
    [SerializeField] private Image autoRollButtonImage;
    [SerializeField] private TMP_Text autoRollButtonText;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color rollingColor = Color.green;
    [SerializeField] private Color disabledColor = Color.gray;
    [SerializeField] private Color normalTextColor = Color.black;
    [SerializeField] private Color activeTextColor = Color.white;

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
        if (autoRollButtonImage != null)
            autoRollButtonImage.color = isRolling ? rollingColor : normalColor;

        if (autoRollButtonText != null)
            autoRollButtonText.color = isRolling ? activeTextColor : normalTextColor;

        if (rollButton != null)
            rollButton.interactable = !isRolling;

        if (rollButtonImage != null)
            rollButtonImage.color = isRolling ? disabledColor : normalColor;

        if (rollButtonText != null)
            rollButtonText.color = isRolling ? activeTextColor : normalTextColor;
    }

    public void SetRollVisual(bool isRolling)
    {
        if (rollButton != null)
            rollButton.interactable = !isRolling;

        if (rollButtonImage != null)
            rollButtonImage.color = isRolling ? disabledColor : normalColor;

        if (rollButtonText != null)
            rollButtonText.color = isRolling ? activeTextColor : normalTextColor;
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