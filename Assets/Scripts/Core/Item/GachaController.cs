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
    private bool isAutoRolling;

    public void Button_ToggleAutoRoll()
    {
        if (isAutoRolling)
            StopAutoRoll();
        else
            StartAutoRoll();
    }

    public void StartAutoRoll()
    {
        if (isAutoRolling)
            return;

        isAutoRolling = true;
        gachaUI.SetAutoRollVisual(true);

        autoRollCoroutine = StartCoroutine(AutoRollRoutine());
    }

    public void StopAutoRoll()
    {
        if (!isAutoRolling)
            return;

        isAutoRolling = false;
        gachaUI.SetAutoRollVisual(false);

        if (autoRollCoroutine != null)
        {
            StopCoroutine(autoRollCoroutine);
            autoRollCoroutine = null;
        }
    }

    private IEnumerator AutoRollRoutine()
    {
        while (isAutoRolling)
        {
            yield return StartCoroutine(PlayOneRoll());

            yield return new WaitForSeconds(resultWaitTime);
        }
    }

    private IEnumerator PlayOneRoll()
    {
        PetUnitData finalPet = gachaSystem.RollPet();

        if (finalPet == null)
            yield break;

        float timer = 0f;

        while (timer < rollingDuration)
        {
            PetUnitData displayPet = gachaSystem.GetRandomDisplayPet();

            gachaUI.ShowRollingPet(displayPet);

            timer += switchInterval;

            yield return new WaitForSeconds(switchInterval);
        }

        inventorySystem.AddPet(finalPet);

        gachaUI.ShowFinalPet(finalPet);

        Debug.Log(
            $"Gacha Result: {finalPet.unitName} ({finalPet.rarity}) | Amount: {inventorySystem.GetAmount(finalPet)}"
        );
    }
}