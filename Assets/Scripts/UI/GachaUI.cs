using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaUI : MonoBehaviour
{
    [Header("Result")]
    [SerializeField] private GachaResultView resultView;

    [Header("Buttons")]
    [SerializeField] private GameObject hideButton;

    [Header("Auto Roll Button")]
    [SerializeField] private Image autoRollButtonImage;
    [SerializeField] private TMP_Text autoRollButtonText;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color rollingColor = Color.green;
    [SerializeField] private Color normalTextColor = Color.black;
    [SerializeField] private Color activeTextColor = Color.white;

    public void ShowRollingPet(PetUnitData pet)
    {
        if (resultView != null)
            resultView.ShowRollingPet(pet);
    }

    public void ShowFinalPet(PetUnitData pet)
    {
        if (resultView != null)
            resultView.ShowFinalPet(pet);
    }

    public void ClearResult()
    {
        if (resultView != null)
            resultView.Clear();
    }

    public void SetAutoRollVisual(bool isRolling)
    {
        if (autoRollButtonImage != null)
            autoRollButtonImage.color = isRolling ? rollingColor : normalColor;

        if (autoRollButtonText != null)
            autoRollButtonText.color = isRolling ? activeTextColor : normalTextColor;
    }

    public void SetHideButtonVisible(bool visible)
    {
        if (hideButton != null)
            hideButton.SetActive(visible);
    }
}