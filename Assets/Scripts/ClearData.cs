using UnityEngine;
using UnityEngine.SceneManagement;

public class ClearData : MonoBehaviour
{
    public void ClearAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        GameManager.Instance.ResetGold();
        InventorySystem.Instance.ClearData();
        PartySystem.Instance.ClearData();
        UpgradeTreeSystem.Instance.ClearData();
        PlayerPrefs.Save();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}