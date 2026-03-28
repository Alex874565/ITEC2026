using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class OptionUI : NetworkBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public NetworkVariable<int> Clicks = new(0);
    public NetworkVariable<bool> CanActivate = new(false);
    public NetworkVariable<int> Price = new(0);
    
    public TextMeshProUGUI ClicksUI;
    public TextMeshProUGUI PriceUI;
    public GameObject Cover;

    public virtual void Initialize()
    {
    }

    private void OnEnable()
    {
        Clicks.OnValueChanged += UpdateClicks;
        Price.OnValueChanged += UpdatePrice;
        CanActivate.OnValueChanged += UpdateCanBuy;
        
        Initialize();
    }

    private void OnDisable()
    {
        Clicks.OnValueChanged -= UpdateClicks;
        Price.OnValueChanged -= UpdatePrice;
        CanActivate.OnValueChanged -= UpdateCanBuy;
    }
    
    public void UpdateClicks(int oldValue, int newValue)
    {
        ClicksUI.text = $"{newValue}/{GameManager.Instance.PlayerCount.Value}";
        if(newValue >= GameManager.Instance.PlayerCount.Value)
        {
            Activate();
        }
    }

    public void UpdatePrice(int oldValue, int newValue)
    {
        if(PriceUI)
            PriceUI.text = $"{newValue}";
    }

    public void UpdateCanBuy(bool oldValue, bool newValue)
    {
        if (Cover == null)
            return;

        Cover.SetActive(!newValue);
    }

    public virtual void Activate()
    {
        if (IsServer)
        {
            CanActivate.Value = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(CanActivate.Value)
            gameObject.transform.localScale = Vector3.one * 1.1f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (CanActivate.Value)
        {
            gameObject.transform.localScale = Vector3.one;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(CanActivate.Value){
            Clicks.Value++;
        }
    }
}
