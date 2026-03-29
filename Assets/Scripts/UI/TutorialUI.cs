using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class TutorialUI : MonoBehaviour, IPointerClickHandler
{
    private GameObject text;

    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Instance.StartGame();
    }
}