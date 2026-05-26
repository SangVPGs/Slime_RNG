using UnityEngine;

public class GachaMiniUI : MonoBehaviour
{
    [SerializeField] private GachaResultView resultView;

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