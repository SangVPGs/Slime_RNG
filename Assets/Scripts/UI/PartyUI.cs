using UnityEngine;

public class PartyUI : MonoBehaviour
{
    [Header("System")]
    [SerializeField] private PartySystem partySystem;

    [Header("UI")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private PetUIItem petItemPrefab;

    private void OnEnable()
    {
        if (partySystem != null)
            partySystem.OnPartyChanged += ShowParty;

        ShowParty();
    }

    private void OnDisable()
    {
        if (partySystem != null)
            partySystem.OnPartyChanged -= ShowParty;
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
        partySystem.RemovePet(petData);
    }

    private void ClearOldItems()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }
}