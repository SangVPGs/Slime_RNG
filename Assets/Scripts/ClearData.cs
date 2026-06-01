using UnityEngine;

public class ClearData : MonoBehaviour
{
    public void ClearAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Debug.Log("PlayerPrefs Cleared");
    }
}