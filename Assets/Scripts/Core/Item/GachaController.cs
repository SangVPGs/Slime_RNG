using System.Collections;
using UnityEngine;

public class GachaController : MonoBehaviour
{
    [Header("Systems")]
    [SerializeField] private GachaSystem gachaSystem;
    [SerializeField] private InventorySystem inventorySystem;

    [Header("UI")]
    [SerializeField] private GachaUI gachaUI;
    [SerializeField] private string gachaUIId;

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
        if (isAutoRolling)
        {
            ShowGachaPanel();
            return;
        }

        if (isRolling)
            return;

        ShowGachaPanel();

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

            if (gachaUI != null)
                gachaUI.ShowPet(displayPet);

            timer += switchInterval;

            yield return new WaitForSeconds(switchInterval);
        }

        inventorySystem.AddPet(finalPet);

        if (gachaUI != null)
            gachaUI.ShowPet(finalPet);

        Debug.Log(
            $"Gacha Result: {finalPet.unitName} ({finalPet.rarity})"
        );

        isRolling = false;

        yield return new WaitForSeconds(resultWaitTime);

        if (startAutoAfterCurrentRoll)
        {
            StartAutoRoll();
            yield break;
        }

        if (!fromAutoRoll || !isAutoRolling)
        {
            HideGachaPanel();
        }
    }

    public void ShowGachaPanel()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.Show(gachaUIId);
    }

    private void HideGachaPanel()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.Hide(gachaUIId);
    }
}