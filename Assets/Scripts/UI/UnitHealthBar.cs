using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        int currentHp = unit.CurrentHp;
        int maxHp = unit.MaxHp;

        float percent = maxHp > 0 ? (float)currentHp / maxHp : 0f;

        if (fillImage != null)
            fillImage.fillAmount = percent;

        if (hpText != null)
            hpText.text = $"{currentHp}/{maxHp}";
    }
}