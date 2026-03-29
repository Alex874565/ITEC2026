using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class OptionUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public TextMeshProUGUI ClicksUI;
    public GameObject Cover;

    protected bool CanActivate = true;
    protected bool HasVotedLocally = false;

    public virtual void Initialize()
    {
        ForceReset();
    }
    
    public void ForceReset()
    {
        HasVotedLocally = false;
        CanActivate = true;
        if (Cover != null) Cover.SetActive(false);
        transform.localScale = Vector3.one;
    }

    // Must match (T old, T next) for NetworkVariable events
    protected void OnUsedStateChanged(bool previousValue, bool newValue)
    {
        UpdateUsedState(newValue);
    }

    protected void OnClicksChanged(int previousValue, int newValue)
    {
        Debug.Log("OnClicksChanged");
        Debug.Log($"Previous: {previousValue}, New: {newValue}");
        UpdateClicksUI(newValue);
    }

    protected virtual void UpdateUsedState(bool isUsed)
    {
        CanActivate = !isUsed;
        if (Cover != null) 
        {
            // The cover stays on if the server says it's used OR if we personally voted
            Cover.SetActive(isUsed || HasVotedLocally);
        }
    }

    public void UpdateClicksUI(int newValue)
    {
        if (ClicksUI != null)
        {
            int total = GameManager.Instance != null ? GameManager.Instance.PlayerCount.Value : 1;
            ClicksUI.text = $"{newValue} / {total} Players";
        }
    }

    public virtual void Activate() { CanActivate = false; }
    public void OnPointerEnter(PointerEventData eventData) { if (CanActivate && !HasVotedLocally) transform.localScale = Vector3.one * 1.05f; }
    public void OnPointerExit(PointerEventData eventData) { transform.localScale = Vector3.one; }
    public virtual void OnPointerClick(PointerEventData eventData) { }
}