using System.Collections;
using UnityEngine;

public class GachaController : MonoBehaviour
{
    [Header("Systems")]
    [SerializeField] private GachaSystem gachaSystem;
    [SerializeField] private InventorySystem inventorySystem;

    [Header("UI")]
    [SerializeField] private GachaUI gachaUI;

    [Header("Timing")]
    [SerializeField] private float rollingDuration = 2f;
    [SerializeField] private float switchInterval = 0.08f;
    [SerializeField] private float resultWaitTime = 1f;

    private Coroutine autoRollCoroutine;
    private Coroutine rollCoroutine;

    private bool isAutoRolling;
    private bool isRolling;
    private bool startAutoAfterCurrentRoll;

    public void Roll()
    {
        if (isRolling || isAutoRolling)
            return;

        SetRollingVisual(true);

        rollCoroutine = StartCoroutine(RollRoutine(false));
    }

    public void AutoRoll()
    {
        if (isAutoRolling)
        {
            StopAutoRoll();
            return;
        }

        if (isRolling)
        {
            startAutoAfterCurrentRoll = true;

            SetRollingVisual(true);

            if (gachaUI != null)
                gachaUI.SetAutoRollVisual(true);

            return;
        }

        StartAutoRoll();
    }

    public void StartAutoRoll()
    {
        if (isAutoRolling)
            return;

        isAutoRolling = true;
        startAutoAfterCurrentRoll = false;

        SetRollingVisual(true);

        if (gachaUI != null)
            gachaUI.SetAutoRollVisual(true);

        autoRollCoroutine = StartCoroutine(AutoRollRoutine());
    }

    public void StopAutoRoll()
    {
        if (!isAutoRolling && !startAutoAfterCurrentRoll)
            return;

        isAutoRolling = false;
        startAutoAfterCurrentRoll = false;

        if (autoRollCoroutine != null)
        {
            StopCoroutine(autoRollCoroutine);
            autoRollCoroutine = null;
        }

        if (!isRolling)
            SetRollingVisual(false);

        if (gachaUI != null)
            gachaUI.SetAutoRollVisual(false);
    }

    private IEnumerator AutoRollRoutine()
    {
        while (isAutoRolling)
        {
            yield return StartCoroutine(RollRoutine(true));

            if (isAutoRolling)
                yield return new WaitForSeconds(resultWaitTime);
        }

        autoRollCoroutine = null;

        if (!isRolling)
            SetRollingVisual(false);
    }

    private IEnumerator RollRoutine(bool fromAutoRoll)
    {
        if (isRolling)
            yield break;

        isRolling = true;

        PetUnitData finalPet = gachaSystem.RollPet();

        if (finalPet == null)
        {
            isRolling = false;

            if (!isAutoRolling && !startAutoAfterCurrentRoll)
                SetRollingVisual(false);

            yield break;
        }

        float timer = 0f;

        while (timer < rollingDuration)
        {
            PetUnitData displayPet = gachaSystem.GetRandomDisplayPet();

            if (gachaUI != null)
                gachaUI.ShowRollingPet(displayPet);

            timer += switchInterval;

            yield return new WaitForSeconds(switchInterval);
        }

        inventorySystem.AddPet(finalPet);

        if (gachaUI != null)
            gachaUI.ShowFinalPet(finalPet);

        Debug.Log(
            $"Gacha Result: {finalPet.unitName} ({finalPet.rarity}) | Amount: {inventorySystem.GetAmount(finalPet)}"
        );

        isRolling = false;

        if (startAutoAfterCurrentRoll)
        {
            yield return new WaitForSeconds(resultWaitTime);
            StartAutoRoll();
            yield break;
        }

        if (!fromAutoRoll && !isAutoRolling)
        {
            yield return new WaitForSeconds(0.3f);
            SetRollingVisual(false);
        }
    }

    private void SetRollingVisual(bool value)
    {
        if (gachaUI == null)
            return;

        gachaUI.SetRollVisual(value);
    }
}