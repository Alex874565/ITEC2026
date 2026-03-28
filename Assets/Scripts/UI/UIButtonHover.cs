using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonHover : MonoBehaviour, IPointerEnterHandler
{
    public UIButtonVisual visual;

    public void OnPointerEnter(PointerEventData eventData)
    {
        UISelectionManager.Instance.SetActiveButton(visual);
    }
}