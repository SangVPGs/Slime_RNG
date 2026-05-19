using System.Collections.Generic;
using UnityEngine;

public class Party: MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private PartyData partyData;

    [Header("Follow")]
    [SerializeField] private float followDistance = 2f;
    [SerializeField] private float spacing = 1.5f;
    [SerializeField] private float stopDistance = 0.25f;

    private readonly List<PetUnit> partyMembers = new();

    private void Start()
    {
        SpawnPartyFromData();
    }

    private void Update()
    {
        FollowPlayer();
    }

    public void SpawnPartyFromData()
    {
        ClearSpawnedParty();

        if (partyData == null)
        {
            return;
        }

        for (int i = 0; i < partyData.MaxPartySize; i++)
        {
            PetUnitData petData = partyData.Pets[i];

            if (petData == null || petData.prefab == null)
                continue;

            Vector3 spawnPosition = GetFollowPosition(i);

            GameObject petObject = Instantiate(
                petData.prefab,
                spawnPosition,
                Quaternion.identity
            );

            PetUnit petUnit = petObject.GetComponent<PetUnit>();

            if (petUnit == null)
            {
                Destroy(petObject);
                continue;
            }

            petUnit.Init(petData);
            partyMembers.Add(petUnit);
        }
    }

    private void FollowPlayer()
    {
        for (int i = 0; i < partyMembers.Count; i++)
        {
            PetUnit pet = partyMembers[i];

            if (pet == null || pet.IsDead)
                continue;

            Vector3 followPosition = GetFollowPosition(i);

            pet.MoveTo(followPosition, stopDistance);
        }
    }

    private Vector3 GetFollowPosition(int index)
    {
        return transform.position -
               transform.forward * (followDistance + spacing * index);
    }

    private void ClearSpawnedParty()
    {
        foreach (PetUnit pet in partyMembers)
        {
            if (pet != null)
                Destroy(pet.gameObject);
        }

        partyMembers.Clear();
    }
}