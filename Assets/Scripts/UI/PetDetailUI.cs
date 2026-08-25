using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PetDetailUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text atkText;

    [Header("Own Colors")]
    [SerializeField] private Color ownedColor = Color.white;
    [SerializeField] private Color lockedColor = Color.black;

    [Header("Rarity Colors")]
    [SerializeField] private Color commonColor = new Color32(180, 180, 180, 255);
    [SerializeField] private Color uncommonColor = new Color32(76, 175, 80, 255);
    [SerializeField] private Color rareColor = new Color32(33, 150, 243, 255);
    [SerializeField] private Color epicColor = new Color32(156, 39, 176, 255);
    [SerializeField] private Color legendaryColor = new Color32(255, 193, 7, 255);

    public void Show(PetUnitData petData, bool isOwned)
    {
        if (petData == null)
            return;

        if (iconImage != null)
        {
            iconImage.sprite = petData.icon;
            iconImage.color = isOwned ? ownedColor : lockedColor;
        }

        if (nameText != null)
            nameText.text = petData.unitName;

        if(rarityText != null)
        {
            rarityText.text = petData.rarity.ToString();
            rarityText.color = GetRarityColor(petData.rarity);
        }

        if (hpText != null)
            hpText.text = isOwned ? $"{NumberFormatter.Format(petData.baseHp)}" : "???";

        if (atkText != null)
            atkText.text = isOwned ? $"{NumberFormatter.Format(petData.baseAtk)}" : "???";

    }

    private Color GetRarityColor(PetRarity rarity)
    {
        return rarity switch
        {
            PetRarity.Common => commonColor,
            PetRarity.Uncommon => uncommonColor,
            PetRarity.Rare => rareColor,
            PetRarity.Epic => epicColor,
            PetRarity.Legendary => legendaryColor,
            _ => Color.white
        };
    }
}