using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class EconomyContinueButton : OptionUI
{
    public override void Initialize()
    {
        Debug.Log("Initializing Continue Button");
        // 1. Ensure the BankManager knows about this button instance
        BankManager.Instance.ContinueButton = this;

        ForceReset();
        
        var clicksVar = BankManager.Instance.ContinueClicks;
        var usedVar = BankManager.Instance.ContinueUsed;

        clicksVar.OnValueChanged += (o, n) => UpdateClicksUI(n);
        usedVar.OnValueChanged += (o, n) => UpdateUsedState(n);

        // 2. Clear old listeners (Clean-up)
        BankManager.Instance.ContinueClicks.OnValueChanged -= OnClicksChanged;
        BankManager.Instance.ContinueUsed.OnValueChanged -= OnUsedStateChanged;

        // 3. Subscribe to NetworkVariables
        BankManager.Instance.ContinueClicks.OnValueChanged += OnClicksChanged;
        BankManager.Instance.ContinueUsed.OnValueChanged += OnUsedStateChanged;

        // 4. Immediate Sync for Clients who join/open late
        UpdateClicksUI(clicksVar.Value);
        UpdateUsedState(usedVar.Value);
    }

    public void InitializeForUpgrades()
    {
        ForceReset();

        // Remove Bank listeners, add UpgradesManager listeners
        UpgradesManager.Instance.ContinueClicks.OnValueChanged += (o, n) => UpdateClicksUI(n);
    
        UpdateClicksUI(UpgradesManager.Instance.ContinueClicks.Value);
    }

// Update OnPointerClick to check which menu is active
    public override void OnPointerClick(PointerEventData eventData)
    {
        if (!CanActivate || HasVotedLocally) return;
        HasVotedLocally = true;

        if (UIManager.Instance.Bank.activeSelf)
            BankManager.Instance.RegisterClickServerRpc(EconomyOptionType.Continue);
        else
            UpgradesManager.Instance.RegisterContinueClickServerRpc();
    }

    public override void Activate()
    {
        // This runs on the SERVER first
        base.Activate();
      
        // Tell all clients to swap their menus
        if (UIManager.Instance != null)
        {
            UIManager.Instance.Bank.SetActive(false);
            UIManager.Instance.Upgrades.SetActive(true);
        }
    
        // Reset the button state for the next time the bank is used
        ForceReset();
    }
}