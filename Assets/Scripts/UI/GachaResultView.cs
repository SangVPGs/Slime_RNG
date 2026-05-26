using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaResultView : MonoBehaviour
{
    [SerializeField] private TMP_Text petName;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Image petImage;

    public void ShowRollingPet(PetUnitData pet)
    {
        if (pet == null)
            return;

        Color color = GetRarityColor(pet.rarity);

        if (petImage != null)
            petImage.sprite = pet.icon;

        if (petName != null)
        {
            petName.text = pet.unitName;
            petName.color = color;
        }

        if (resultText != null)
            resultText.gameObject.SetActive(false);
    }

    public void ShowFinalPet(PetUnitData pet)
    {
        if (pet == null)
            return;

        Color color = GetRarityColor(pet.rarity);

        if (petImage != null)
            petImage.sprite = pet.icon;

        if (petName != null)
        {
            petName.text = pet.unitName;
            petName.color = color;
        }

        if (resultText != null)
        {
            resultText.gameObject.SetActive(true);
            resultText.text = $"You got: {pet.unitName}";
            resultText.color = color;
        }
    }

    public void Clear()
    {
        if (petImage != null)
            petImage.sprite = null;

        if (petName != null)
            petName.text = "";

        if (resultText != null)
        {
            resultText.text = "";
            resultText.gameObject.SetActive(false);
        }
    }

    private Color GetRarityColor(PetRarity rarity)
    {
        return rarity switch
        {
            PetRarity.Common => Color.white,
            PetRarity.Uncommon => Color.green,
            PetRarity.Rare => Color.blue,
            PetRarity.Epic => Color.magenta,
            PetRarity.Legendary => Color.yellow,
            _ => Color.white
        };
    }
}