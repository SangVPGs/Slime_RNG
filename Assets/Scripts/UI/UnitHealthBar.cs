using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image fillImage;
    [SerializeField] private TMP_Text hpText;

    [Header("Position")]
    [SerializeField] private float heightOffset = 2f;

    [Header("Camera")]
    [SerializeField] private bool faceCamera = true;

    private Unit unit;
    private Camera mainCamera;

    private void Awake()
    {
        unit = GetComponentInParent<Unit>();
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (unit == null)
            return;

        UpdatePosition();
        UpdateRotation();
        UpdateHealthUI();
    }

    private void UpdatePosition()
    {
        transform.localPosition = Vector3.up * heightOffset;
    }

    private void UpdateRotation()
    {
        if (!faceCamera || mainCamera == null)
            return;

        transform.forward = mainCamera.transform.forward;
    }

    private void UpdateHealthUI()
    {
        double currentHp = unit.CurrentHp;
        double maxHp = unit.MaxHp;

        double percent = maxHp <= 0 ? 0d : Math.Clamp((double)currentHp / maxHp, 0d, 1d);

        fillImage.fillAmount = (float)percent;

        if (hpText != null)
            hpText.text = $"{NumberFormatter.Format(currentHp)}/{NumberFormatter.Format(maxHp)}";
    }
}