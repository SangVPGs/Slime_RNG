using UnityEngine;
using UnityEngine.EventSystems;

public class PetListClickArea : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private PetListUIItem item;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (item == null)
            return;

        item.Click();
    }
}