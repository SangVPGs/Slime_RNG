using System;
using UnityEngine;

public class PartySystem : MonoBehaviour
{
    public event Action OnPartyChanged;

    [SerializeField] private PartyData partyData;

    public PartyData Data => partyData;

    public bool AddPet(PetUnitData petData)
    {
        Debug.Log($"PartySystem AddPet called: {petData?.unitName}");

        if (partyData == null)
        {
            Debug.LogError("PartyData missing.");
            return false;
        }

        bool success = partyData.AddPet(petData);

        Debug.Log($"PartyData AddPet success: {success}");

        if (success)
            OnPartyChanged?.Invoke();

        return success;
    }

    public bool RemovePet(PetUnitData petData)
    {
        if (partyData == null)
            return false;

        bool success = partyData.RemovePet(petData);

        if (success)
            OnPartyChanged?.Invoke();

        return success;
    }
}