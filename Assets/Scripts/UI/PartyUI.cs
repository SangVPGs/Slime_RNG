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

        foreach (InventorySystem.PetInventoryEntry entry in partySystem.Data.Pets)
        {
            if (entry == null || entry.petData == null)
                continue;

            PetUIItem item = Instantiate(petItemPrefab, contentParent);

            item.SetupParty(
                entry,
                OnPartyPetClicked,
                !partySystem.AutoEquip
            );
        }
    }

    private void OnPartyPetClicked(InventorySystem.PetInventoryEntry entry)
    {
        if (entry == null || partySystem == null)
            return;

        if (partySystem.AutoEquip)
            return;

        partySystem.RemovePet(entry);
    }

    private void ClearOldItems()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }
}