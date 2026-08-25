using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PetListUIItem : MonoBehaviour
{
    [Header("Icon")]
    [SerializeField] private Image iconImage;

    [Header("Visual")]
    [SerializeField] private Image backgroundImage;

    [Header("Owned Visual")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private Color lockedIconColor = Color.black;

    [Header("Result Effect")]
    [SerializeField] private float resultScale = 1.15f;
    [SerializeField] private float iconPunchScale = 0.18f;
    [SerializeField] private float effectDuration = 0.25f;
    [SerializeField] private Ease effectEase = Ease.OutBack;

    [Header("Rarity Colors")]
    [SerializeField] private Color commonColor = new Color32(180, 180, 180, 255);
    [SerializeField] private Color uncommonColor = new Color32(76, 175, 80, 255);
    [SerializeField] private Color rareColor = new Color32(33, 150, 243, 255);
    [SerializeField] private Color epicColor = new Color32(156, 39, 176, 255);
    [SerializeField] private Color legendaryColor = new Color32(255, 193, 7, 255);

    private PetUnitData petData;
    private bool isOwned;
    private Action<PetUnitData, bool> onClicked;

    private Tween effectTween;
    private Vector3 baseScale;
    private Vector3 iconBaseScale;

    private void Awake()
    {
        baseScale = transform.localScale;

        if (iconImage != null)
            iconBaseScale = iconImage.transform.localScale;

        ResetResultEffect();
    }

    private void OnDisable()
    {
        KillEffect();
        ResetResultEffect();
    }

    public void Setup(
        PetUnitData petData,
        bool isOwned,
        Action<PetUnitData, bool> clickCallback)
    {
        KillEffect();
        ResetResultEffect();

        this.petData = petData;
        this.isOwned = isOwned;
        onClicked = clickCallback;

        if (petData == null)
            return;

        SetIcon(petData.icon, isOwned ? Color.white : lockedIconColor);
        SetBackgroundColor(GetRarityColor(petData.rarity));

        if (canvasGroup != null)
            canvasGroup.alpha = isOwned ? 1f : 0.6f;

        if (lockedOverlay != null)
            lockedOverlay.SetActive(!isOwned);
    }

    private void SetBackgroundColor(Color color)
    {
        if (backgroundImage == null)
            return;

        backgroundImage.color = color;
    }

    public void SetupGachaRoll(PetUnitData pet)
    {
        KillEffect();
        ResetResultEffect();

        petData = pet;
        isOwned = true;
        onClicked = null;

        if (pet == null)
            return;

        SetIcon(pet.icon, Color.white);

        SetBackgroundColor(GetRarityColor(pet.rarity));

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        if (lockedOverlay != null)
            lockedOverlay.SetActive(false);
    }

    public void SetupGachaSpecial(Sprite sprite, Color color)
    {
        KillEffect();
        ResetResultEffect();

        petData = null;
        isOwned = false;
        onClicked = null;

        SetIcon(sprite, color);

        SetBackgroundColor(color);

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        if (lockedOverlay != null)
            lockedOverlay.SetActive(false);
    }

    public void PlayResultEffect()
    {
        KillEffect();

        Sequence sequence = DOTween.Sequence();

        sequence.Join(
            transform
                .DOScale(baseScale * resultScale, effectDuration)
                .SetEase(effectEase)
        );

        if (iconImage != null)
        {
            iconImage.transform.localScale = iconBaseScale;

            sequence.Join(
                iconImage.transform
                    .DOPunchScale(Vector3.one * iconPunchScale, effectDuration, 8, 0.75f)
            );
        }

        effectTween = sequence;
    }

    public void ResetResultEffect()
    {
        transform.localScale = baseScale == Vector3.zero ? Vector3.one : baseScale;

        if (iconImage != null)
            iconImage.transform.localScale = iconBaseScale == Vector3.zero ? Vector3.one : iconBaseScale;
    }

    private void KillEffect()
    {
        effectTween?.Kill();
        effectTween = null;

        transform.DOKill();

        if (iconImage != null)
            iconImage.transform.DOKill();
    }

    private void SetIcon(Sprite sprite, Color color)
    {
        if (iconImage == null)
            return;

        iconImage.sprite = sprite;
        iconImage.color = color;
        iconImage.enabled = sprite != null;
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
            PetRarity.Common => commonColor,
            PetRarity.Uncommon => uncommonColor,
            PetRarity.Rare => rareColor,
            PetRarity.Epic => epicColor,
            PetRarity.Legendary => legendaryColor,
            _ => Color.white
        };
    }
}