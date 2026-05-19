using System.Collections.Generic;
using UnityEngine;

public class Party : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private PartyData partyData;

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

    private readonly List<PetUnit> partyMembers = new();

    private SlimeUnit currentSlimeTarget;
    private float nextScanTime;

    private void Start()
    {
        SpawnPartyFromData();
    }

    private void FixedUpdate()
    {
        if (IsPartyTooFarFromPlayer())
        {
            currentSlimeTarget = null;
            FollowPlayer();
            return;
        }

        if (HasValidTarget())
        {
            AttackTarget();
            return;
        }

        ScanTargetByInterval();

        if (HasValidTarget())
        {
            AttackTarget();
        }
        else
        {
            FollowPlayer();
        }
    }

    public void SpawnPartyFromData()
    {
        ClearSpawnedParty();

        if (partyData == null)
            return;

        int count = Mathf.Min(partyData.Pets.Count, partyData.MaxPartySize);

        for (int i = 0; i < count; i++)
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

    private void ScanTargetByInterval()
    {
        if (Time.time < nextScanTime)
            return;

        nextScanTime = Time.time + scanInterval;

        if (currentSlimeTarget != null && !currentSlimeTarget.IsDead)
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
        return currentSlimeTarget != null && !currentSlimeTarget.IsDead;
    }

    private void AttackTarget()
    {
        foreach (PetUnit pet in partyMembers)
        {
            if (pet == null || pet.IsDead)
                continue;

            if (IsSlimeInAttackRange(pet, currentSlimeTarget))
            {
                pet.Attack(currentSlimeTarget);
            }
            else
            {
                float petStopDistance = pet.Data.atkRange * 0.9f;
                pet.MoveTo(currentSlimeTarget.transform.position, petStopDistance);
            }
        }
    }

    private bool IsSlimeInAttackRange(PetUnit pet, SlimeUnit slime)
    {
        Vector3 offset = slime.transform.position - pet.transform.position;
        offset.y = 0f;

        float range = pet.Data.atkRange;

        return offset.sqrMagnitude <= range * range;
    }

    private bool IsPartyTooFarFromPlayer()
    {
        foreach (PetUnit pet in partyMembers)
        {
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