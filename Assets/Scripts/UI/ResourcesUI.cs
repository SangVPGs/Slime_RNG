using TMPro;
using UnityEngine;

public class ResourcesUI : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TMP_Text goldText;

    private void Update()
    {
        UpdateGoldUI();
    }

    private void UpdateGoldUI()
    {
        if (goldText == null)
            return;

        if (GameManager.Instance == null)
            return;

        goldText.text = $"Gold: {GameManager.Instance.Gold}";
    }
}