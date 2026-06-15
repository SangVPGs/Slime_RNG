using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaUI : MonoBehaviour
{
    [Header("Rate")]
    [SerializeField] private TMP_Text rateText;

    [Header("Buttons")]
    [SerializeField] private GameObject hideButton;
    [SerializeField] private GameObject autoRollButton;

    [Header("Auto Roll Visual")]
    [SerializeField] private Image autoRollButtonImage;
    [SerializeField] private TMP_Text autoRollButtonText;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color rollingColor = Color.green;
    [SerializeField] private Color normalTextColor = Color.black;
    [SerializeField] private Color activeTextColor = Color.white;

    private void OnEnable()
    {
        RefreshRate();
    }

    public void RefreshRate()
    {
        if (rateText == null)
            return;

        rateText.text = GachaSystem.Instance != null
            ? GachaSystem.Instance.GetCurrentRateText()
            : "Rate: N/A";
    }

    public void ShowFullControls(bool showHideButton)
    {
        RefreshRate();
        SetHideButtonVisible(showHideButton);
    }

    public void SetAutoRollVisual(bool isAutoRolling)
    {
        if (autoRollButtonImage != null)
            autoRollButtonImage.color = isAutoRolling ? rollingColor : normalColor;

        if (autoRollButtonText != null)
            autoRollButtonText.color = isAutoRolling ? activeTextColor : normalTextColor;
    }

    public void SetHideButtonVisible(bool visible)
    {
        if (hideButton != null)
            hideButton.SetActive(visible);
    }
}