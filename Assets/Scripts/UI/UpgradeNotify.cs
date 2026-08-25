using TMPro;
using UnityEngine;

public class UpgradeNotify : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject redDot;
    [SerializeField] private TMP_Text countText;

    private UpgradeTreeSystem treeSystem => UpgradeTreeSystem.Instance;
    private GameManager gameManager => GameManager.Instance;

    private void Start()
    {
        if (treeSystem != null)
            treeSystem.OnTreeChanged += Refresh;

        if (gameManager != null)
            gameManager.OnGoldChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (treeSystem != null)
            treeSystem.OnTreeChanged -= Refresh;

        if (gameManager != null)
            gameManager.OnGoldChanged -= Refresh;
    }

    public void Refresh()
    {
        if (treeSystem == null)
        {
            SetVisible(false, 0);
            return;
        }

        int count = treeSystem.GetUnlockableNodeCount();
        SetVisible(count > 0, count);
    }

    private void SetVisible(bool visible, int count)
    {
        if (redDot != null)
            redDot.SetActive(visible);

        if (countText != null)
        {
            countText.gameObject.SetActive(visible);
            countText.text = count > 99 ? "99+" : count.ToString();
        }
    }
}