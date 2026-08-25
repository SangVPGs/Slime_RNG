using TMPro;
using UnityEngine;

public class ResourcesUI : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private TMP_Text poonText; 

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

        goldText.text = $"{NumberFormatter.Format(GameManager.Instance.Gold)}";
        poonText.text = $"{NumberFormatter.Format(GameManager.Instance.Poon)}";
    }
}