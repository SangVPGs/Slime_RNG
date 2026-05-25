using UnityEngine;

public class PartyUI : MonoBehaviour
{
    [Header("System")]
    [SerializeField] private PartySystem partySystem;
    [SerializeField] private InventorySystem inventorySystem;

    [Header("UI")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private PetUIItem petItemPrefab;

    private void OnEnable()
    {
        if (partySystem != null)
            partySystem.OnPartyChanged += ShowParty;
    }

    private void OnDisable()
    {
        if (partySystem != null)
            partySystem.OnPartyChanged -= ShowParty;
    }

    private void Start()
    {
        ShowParty();
    }

    public void ShowParty()
    {
        if (partySystem == null || partySystem.Data == null)
            return;

        ClearOldItems();

        foreach (PetUnitData pet in partySystem.Data.Pets)
        {
            if (pet == null)
                continue;

            PetUIItem item = Instantiate(petItemPrefab, contentParent);
            item.SetupParty(pet, OnPartyPetClicked);
        }
    }

    private void OnPartyPetClicked(PetUnitData petData)
    {
        if (petData == null || partySystem == null || inventorySystem == null)
            return;

        bool removedFromParty = partySystem.RemovePet(petData);

        if (!removedFromParty)
            return;

        inventorySystem.SetPetInParty(petData, false);
    }

    private void ClearOldItems()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }
}