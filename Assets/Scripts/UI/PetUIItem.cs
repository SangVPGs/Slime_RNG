using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PetUIItem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;

    private PetUnitData petData;
    private PetListUI owner;

    public void Setup(PetUnitData data, PetListUI listUI)
    {
        petData = data;
        owner = listUI;

        iconImage.sprite = data.icon;
        nameText.text = data.unitName;
        
        switch(data.rarity)
        {
            case PetRarity.Common:
                nameText.color = Color.gray;
                break;
            case PetRarity.Uncommon:
                nameText.color = Color.green;
                break;
            case PetRarity.Rare:
                nameText.color = Color.blue;
                break;
            case PetRarity.Epic:
                nameText.color = Color.magenta;
                break;
            case PetRarity.Legendary:
                nameText.color = Color.yellow;
                break;
        }
    }
}