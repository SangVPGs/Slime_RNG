using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PetListUIItem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;

    [Header("Owned Visual")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private Color lockedIconColor = Color.black;

    private PetUnitData petData;
    private bool isOwned;
    private Action<PetUnitData, bool> onClicked;

    public void Setup(
        PetUnitData petData,
        bool isOwned,
        Action<PetUnitData, bool> clickCallback)
    {
        this.petData = petData;
        this.isOwned = isOwned;
        onClicked = clickCallback;

        if (petData == null)
            return;

        if (iconImage != null)
        {
            iconImage.sprite = petData.icon;
            iconImage.color = isOwned ? Color.white : lockedIconColor;
        }

        if (nameText != null)
        {
            nameText.text = petData.unitName;
            nameText.color = isOwned ? GetRarityColor(petData.rarity) : Color.gray;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = isOwned ? 1f : 0.6f;

        if (lockedOverlay != null)
            lockedOverlay.SetActive(!isOwned);
    }

    public void Click()
    {
        if (petData == null)
            return;

        onClicked?.Invoke(petData, isOwned);
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