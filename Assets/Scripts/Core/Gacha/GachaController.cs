using System.Collections;
using UnityEngine;

public class GachaController : MonoBehaviour
{
    [Header("Systems")]
    [SerializeField] private GachaSystem gachaSystem;
    [SerializeField] private InventorySystem inventorySystem;

    [Header("UI Views")]
    [SerializeField] private GachaUI gachaUI;
    [SerializeField] private GachaMiniUI gachaMiniUI;

    [Header("UI Manager Ids")]
    [SerializeField] private string gachaUIId = "2";
    [SerializeField] private string gachaMiniUIId = "4";

    [Header("Timing")]
    [SerializeField] private float rollingDuration = 2f;
    [SerializeField] private float switchInterval = 0.08f;
    [SerializeField] private float resultWaitTime = 1f;

    private Coroutine autoRollCoroutine;

    private bool isAutoRolling;
    private bool isRolling;
    private bool startAutoAfterCurrentRoll;
    private bool useMiniResult;

    public void Roll()
    {
        if (isAutoRolling)
        {
            ShowFullPanel();
            return;
        }

        if (isRolling)
            return;

        useMiniResult = false;
        ShowFullPanel();

        StartCoroutine(RollRoutine(false));
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
            useMiniResult = false;

            ShowFullPanel();

            if (gachaUI != null)
                gachaUI.SetAutoRollVisual(true);

            return;
        }

        StartAutoRoll();
    }

    public void HideButton()
    {
        if (isAutoRolling)
        {
            useMiniResult = true;
            ShowMiniPanel();
        }
        else
        {
            HideAllPanels();
        }
    }

    private void StartAutoRoll()
    {
        if (isAutoRolling)
            return;

        isAutoRolling = true;
        startAutoAfterCurrentRoll = false;
        useMiniResult = false;

        ShowFullPanel();

        if (gachaUI != null)
            gachaUI.SetAutoRollVisual(true);

        autoRollCoroutine = StartCoroutine(AutoRollRoutine());
    }

    private void StopAutoRoll()
    {
        isAutoRolling = false;
        startAutoAfterCurrentRoll = false;
        useMiniResult = false;

        if (autoRollCoroutine != null)
        {
            StopCoroutine(autoRollCoroutine);
            autoRollCoroutine = null;
        }

        if (gachaUI != null)
            gachaUI.SetAutoRollVisual(false);

        HideAllPanels();
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
            yield break;
        }

        float timer = 0f;

        while (timer < rollingDuration)
        {
            PetUnitData displayPet = gachaSystem.GetRandomDisplayPet();
            ShowRollingPet(displayPet);

            timer += switchInterval;

            yield return new WaitForSeconds(switchInterval);
        }

        inventorySystem.AddPet(finalPet);

        ShowFinalPet(finalPet);

        Debug.Log($"Gacha Result: {finalPet.unitName} ({finalPet.rarity})");

        isRolling = false;

        yield return new WaitForSeconds(resultWaitTime);

        if (startAutoAfterCurrentRoll)
        {
            startAutoAfterCurrentRoll = false;
            StartAutoRoll();
            yield break;
        }

        if (!fromAutoRoll && !isAutoRolling)
            HideAllPanels();
    }

    private void ShowRollingPet(PetUnitData pet)
    {
        if (useMiniResult)
            gachaMiniUI?.ShowRollingPet(pet);
        else
            gachaUI?.ShowRollingPet(pet);
    }

    private void ShowFinalPet(PetUnitData pet)
    {
        if (useMiniResult)
            gachaMiniUI?.ShowFinalPet(pet);
        else
            gachaUI?.ShowFinalPet(pet);
    }

    private void ShowFullPanel()
    {
        useMiniResult = false;

        if (UIManager.Instance == null)
            return;

        UIManager.Instance.Show(gachaUIId);
        UIManager.Instance.Hide(gachaMiniUIId);

        gachaMiniUI?.ClearResult();
    }

    private void ShowMiniPanel()
    {
        useMiniResult = true;

        if (UIManager.Instance == null)
            return;

        UIManager.Instance.Hide(gachaUIId);
        UIManager.Instance.Show(gachaMiniUIId);

        gachaUI?.ClearResult();
    }

    private void HideAllPanels()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.Hide(gachaUIId);
            UIManager.Instance.Hide(gachaMiniUIId);
        }

        gachaUI?.ClearResult();
        gachaMiniUI?.ClearResult();
    }
}