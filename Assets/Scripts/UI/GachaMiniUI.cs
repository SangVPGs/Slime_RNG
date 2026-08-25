using System;
using System.Collections.Generic;
using UnityEngine;

public class GachaMiniUI : MonoBehaviour
{
    [SerializeField] private GachaResultView resultView;

    public void PlayChainRoll(
        Func<int, int, GachaReward> rollRewardFunc,
        Func<int, int, GachaReward> displayRewardFunc,
        Action<IReadOnlyList<PetUnitData>, GachaReward> onComplete)
    {
        if (resultView != null)
        {
            resultView.PlayChainRoll(rollRewardFunc, displayRewardFunc, onComplete);
            return;
        }

        onComplete?.Invoke(null, null);
    }

    public void ShowRollingPet(PetUnitData pet)
    {
        if (resultView != null)
            resultView.ShowRollingPet(pet);
    }

    public void ShowFinalPet(PetUnitData pet)
    {
        if (resultView != null)
            resultView.ShowFinalPet(pet);
    }

    public void ClearResult()
    {
        if (resultView != null)
            resultView.Clear();
    }
}