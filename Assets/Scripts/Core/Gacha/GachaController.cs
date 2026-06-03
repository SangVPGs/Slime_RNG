using System.Collections;
using UnityEngine;

public class GachaController : MonoBehaviour
{
    private GachaSystem gachaSystem => GachaSystem.Instance;
     private InventorySystem inventorySystem => InventorySystem.Instance;

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

    private bool isRolling;
    private bool isAutoRolling;
    private bool stopAutoAfterCurrentRoll;
    private bool startAutoAfterCurrentRoll;
    private bool useMiniResult;

    public void Roll()
    {
        if (!CanRoll())
            return;

        if (isAutoRolling || startAutoAfterCurrentRoll)
        {
            ShowFullPanel(true);
            return;
        }

        if (isRolling)
            return;

        useMiniResult = false;
        ShowFullPanel(false);

        StartCoroutine(RollRoutine(false));
    }

    public void AutoRoll()
    {
        if (!CanRoll())
            return;

        if (isAutoRolling || startAutoAfterCurrentRoll)
        {
            RequestStopAutoRoll();
            return;
        }

        if (isRolling)
        {
            startAutoAfterCurrentRoll = true;
            stopAutoAfterCurrentRoll = false;
            useMiniResult = false;

            ShowFullPanel(true);
            gachaUI?.SetAutoRollVisual(true);

            return;
        }

        StartAutoRoll(true);
    }

    public void HideButton()
    {
        bool autoMode = isAutoRolling || startAutoAfterCurrentRoll;

        if (!autoMode)
        {
            if (!isRolling)
                HideAllPanels();

            return;
        }

        useMiniResult = true;
        ShowMiniPanel();
    }

    private bool CanRoll()
    {
        if (gachaSystem == null)
        {
            Debug.LogError("GachaSystem is missing.");
            return false;
        }

        if (inventorySystem == null)
        {
            Debug.LogError("InventorySystem is missing.");
            return false;
        }

        return true;
    }

    private void StartAutoRoll(bool showFullPanel)
    {
        if (isAutoRolling)
            return;

        isAutoRolling = true;
        startAutoAfterCurrentRoll = false;
        stopAutoAfterCurrentRoll = false;

        if (showFullPanel)
        {
            useMiniResult = false;
            ShowFullPanel(true);
        }
        else
        {
            useMiniResult = true;
            ShowMiniPanel();
        }

        gachaUI?.SetAutoRollVisual(true);

        autoRollCoroutine = StartCoroutine(AutoRollRoutine());
    }

    private void RequestStopAutoRoll()
    {
        isAutoRolling = false;
        startAutoAfterCurrentRoll = false;
        stopAutoAfterCurrentRoll = true;

        gachaUI?.SetAutoRollVisual(false);
        gachaUI?.SetHideButtonVisible(false);

        if (!isRolling)
            StopAutoRollNow();
    }

    private void StopAutoRollNow()
    {
        isAutoRolling = false;
        startAutoAfterCurrentRoll = false;
        stopAutoAfterCurrentRoll = false;
        useMiniResult = false;

        if (autoRollCoroutine != null)
        {
            StopCoroutine(autoRollCoroutine);
            autoRollCoroutine = null;
        }

        gachaUI?.SetAutoRollVisual(false);
        gachaUI?.SetHideButtonVisible(false);

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

            if (!isAutoRolling)
                HideAllPanels();

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

        if (stopAutoAfterCurrentRoll)
        {
            StopAutoRollNow();
            yield break;
        }

        if (startAutoAfterCurrentRoll)
        {
            bool shouldUseMini = useMiniResult;

            startAutoAfterCurrentRoll = false;
            StartAutoRoll(!shouldUseMini);

            yield break;
        }

        if (!isAutoRolling && !fromAutoRoll)
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

    private void ShowFullPanel(bool showHideButton)
    {
        useMiniResult = false;

        UIManager.Instance?.Show(gachaUIId);
        UIManager.Instance?.Hide(gachaMiniUIId);

        gachaMiniUI?.ClearResult();
        gachaUI?.SetHideButtonVisible(showHideButton);
    }

    private void ShowMiniPanel()
    {
        useMiniResult = true;

        UIManager.Instance?.Hide(gachaUIId);
        UIManager.Instance?.Show(gachaMiniUIId);

        gachaUI?.ClearResult();
        gachaUI?.SetHideButtonVisible(false);
    }

    private void HideAllPanels()
    {
        UIManager.Instance?.Hide(gachaUIId);
        UIManager.Instance?.Hide(gachaMiniUIId);

        gachaUI?.ClearResult();
        gachaMiniUI?.ClearResult();

        gachaUI?.SetAutoRollVisual(false);
        gachaUI?.SetHideButtonVisible(false);
    }
}