using UnityEngine;

public class PetListUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private PetDatabase database;

    [Header("UI")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private PetUIItem petItemPrefab;

    private void Start()
    {
        ShowPets();
    }

    public void ShowPets()
    {
        ClearOldItems();

        foreach (PetUnitData pet in database.Pets)
        {
            PetUIItem item = Instantiate(petItemPrefab, contentParent);
            item.SetupIndex(pet);
        }
    }

    private void ClearOldItems()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }
}