using System.Collections.Generic;
using UnityEngine;

public class PartyController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private PartySystem partySystem;

    [Header("Pet Prefab")]
    [SerializeField] private PetUnit petPrefab;

    [Header("Follow")]
    [SerializeField] private float followDistance = 2f;
    [SerializeField] private float spacing = 1.5f;
    [SerializeField] private float stopDistance = 0.25f;

    [Header("Return To Player")]
    [SerializeField] private float maxDistanceFromPlayer = 10f;

    [Header("Combat Detection")]
    [SerializeField] private LayerMask slimeLayer;
    [SerializeField] private float scanInterval = 0.25f;
    [SerializeField] private float detectRange = 5f;

    private readonly Dictionary<string, PetUnit> partyMembers = new();

    private SlimeUnit currentSlimeTarget;
    private float nextScanTime;

    private void OnEnable()
    {
        if (partySystem != null)
            partySystem.OnPartyChanged += SyncParty;
    }

    private void OnDisable()
    {
        if (partySystem != null)
            partySystem.OnPartyChanged -= SyncParty;
    }

    private void Start()
    {
        SyncParty();
    }

    private void FixedUpdate()
    {
        ScanTargetByInterval();

        if (HasValidTarget())
        {
            if (HasAnyPetInAttackRange(currentSlimeTarget))
            {
                AttackTarget();
                return;
            }

            if (!IsPartyTooFarFromPlayer())
            {
                AttackTarget();
                return;
            }
        }

        FollowPlayer();
    }

    public void SyncParty()
    {
        if (partySystem == null ||
            partySystem.Data == null ||
            petPrefab == null)
        {
            return;
        }

        IReadOnlyList<InventorySystem.PetInventoryEntry> partyPets = partySystem.Data.Pets;

        RemoveMissingPets(partyPets);
        SpawnNewPets(partyPets);

        currentSlimeTarget = null;
    }

    private void RemoveMissingPets(IReadOnlyList<InventorySystem.PetInventoryEntry> partyPets)
    {
        List<string> removeList = new();

        foreach (KeyValuePair<string, PetUnit> pair in partyMembers)
        {
            bool stillInParty = false;

            foreach (InventorySystem.PetInventoryEntry entry in partyPets)
            {
                if (entry != null && entry.petId == pair.Key)
                {
                    stillInParty = true;
                    break;
                }
            }

            if (!stillInParty)
            {
                if (pair.Value != null)
                    Destroy(pair.Value.gameObject);

                removeList.Add(pair.Key);
            }
        }

        foreach (string petId in removeList)
        {
            partyMembers.Remove(petId);
        }
    }

    private void SpawnNewPets(IReadOnlyList<InventorySystem.PetInventoryEntry> partyPets)
    {
        for (int i = 0; i < partyPets.Count; i++)
        {
            InventorySystem.PetInventoryEntry entry = partyPets[i];

            if (entry == null || entry.petData == null)
                continue;

            if (string.IsNullOrEmpty(entry.petId))
                continue;

            if (partyMembers.ContainsKey(entry.petId))
                continue;

            Vector3 spawnPosition = GetFollowPosition(i);

            PetUnit petUnit = Instantiate(
                petPrefab,
                spawnPosition,
                Quaternion.identity
            );

            petUnit.Init(entry.petData, entry.petId);
            petUnit.SetLevel(entry.level);

            partyMembers.Add(entry.petId, petUnit);
        }
    }

    private void ScanTargetByInterval()
    {
        if (Time.time < nextScanTime)
            return;

        nextScanTime = Time.time + scanInterval;

        if (HasValidTarget())
            return;

        currentSlimeTarget = FindNearestAliveSlimeAroundPlayer();
    }

    private SlimeUnit FindNearestAliveSlimeAroundPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            detectRange,
            slimeLayer
        );

        SlimeUnit nearestSlime = null;
        float nearestSqrDistance = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            SlimeUnit slime = hit.GetComponentInParent<SlimeUnit>();

            if (slime == null || slime.IsDead)
                continue;

            Vector3 offset = slime.transform.position - transform.position;
            offset.y = 0f;

            float sqrDistance = offset.sqrMagnitude;

            if (sqrDistance < nearestSqrDistance)
            {
                nearestSqrDistance = sqrDistance;
                nearestSlime = slime;
            }
        }

        return nearestSlime;
    }

    private bool HasValidTarget()
    {
        return currentSlimeTarget != null &&
               !currentSlimeTarget.IsDead &&
               IsTargetInsideDetectRange(currentSlimeTarget);
    }

    private bool IsTargetInsideDetectRange(SlimeUnit slime)
    {
        if (slime == null)
            return false;

        Vector3 offset = slime.transform.position - transform.position;
        offset.y = 0f;

        return offset.sqrMagnitude <= detectRange * detectRange;
    }

    private void AttackTarget()
    {
        if (currentSlimeTarget == null)
            return;

        foreach (KeyValuePair<string, PetUnit> pair in partyMembers)
        {
            PetUnit pet = pair.Value;

            if (pet == null || pet.IsDead)
                continue;

            if (IsSlimeInAttackRange(pet, currentSlimeTarget))
            {
                pet.Attack(currentSlimeTarget);
            }
            else
            {
                float petStopDistance = pet.AtkRange * 0.9f;

                pet.MoveTo(
                    currentSlimeTarget.transform.position,
                    petStopDistance
                );
            }
        }
    }

    private bool IsSlimeInAttackRange(PetUnit pet, SlimeUnit slime)
    {
        if (pet == null || slime == null)
            return false;

        Vector3 offset = slime.transform.position - pet.transform.position;
        offset.y = 0f;

        float range = pet.AtkRange;

        return offset.sqrMagnitude <= range * range;
    }

    private bool HasAnyPetInAttackRange(SlimeUnit slime)
    {
        if (slime == null)
            return false;

        foreach (KeyValuePair<string, PetUnit> pair in partyMembers)
        {
            PetUnit pet = pair.Value;

            if (pet == null || pet.IsDead)
                continue;

            if (IsSlimeInAttackRange(pet, slime))
                return true;
        }

        return false;
    }

    private bool IsPartyTooFarFromPlayer()
    {
        foreach (KeyValuePair<string, PetUnit> pair in partyMembers)
        {
            PetUnit pet = pair.Value;

            if (pet == null || pet.IsDead)
                continue;

            Vector3 offset = pet.transform.position - transform.position;
            offset.y = 0f;

            if (offset.sqrMagnitude > maxDistanceFromPlayer * maxDistanceFromPlayer)
                return true;
        }

        return false;
    }

    private void FollowPlayer()
    {
        int index = 0;

        foreach (KeyValuePair<string, PetUnit> pair in partyMembers)
        {
            PetUnit pet = pair.Value;

            if (pet == null || pet.IsDead)
                continue;

            Vector3 followPosition = GetFollowPosition(index);

            pet.MoveTo(
                followPosition,
                stopDistance
            );

            index++;
        }
    }

    private Vector3 GetFollowPosition(int index)
    {
        return transform.position -
               transform.forward * (followDistance + spacing * index);
    }

    public void ClearParty()
    {
        foreach (KeyValuePair<string, PetUnit> pair in partyMembers)
        {
            if (pair.Value != null)
                Destroy(pair.Value.gameObject);
        }

        partyMembers.Clear();
        currentSlimeTarget = null;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxDistanceFromPlayer);
    }
#endif
}