using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PetDetailUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private TMP_Text atkText;

    [SerializeField] private Color ownedColor = Color.white;
    [SerializeField] private Color lockedColor = Color.black;

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

        if (hpText != null)
            hpText.text = isOwned ? $"{NumberFormatter.Format(petData.baseHp)}" : "???";

        if (atkText != null)
            atkText.text = isOwned ? $"{NumberFormatter.Format(petData.baseAtk)}" : "???";

    }
}