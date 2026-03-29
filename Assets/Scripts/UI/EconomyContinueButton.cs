using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class EconomyContinueButton : OptionUI
{
    private enum ContinueContext
    {
        None,
        Bank,
        Upgrades
    }

    private ContinueContext currentContext = ContinueContext.None;

    public override void Initialize()
    {
        UnsubscribeAll();

        currentContext = ContinueContext.Bank;
        mode = ContinueMode.Bank;
        BankManager.Instance.ContinueButton = this;

        ForceReset();

        BankManager.Instance.ContinueClicks.OnValueChanged += OnBankClicksChanged;
        BankManager.Instance.ContinueUsed.OnValueChanged += OnBankUsedChanged;

        UpdateClicksUI(BankManager.Instance.ContinueClicks.Value);
        UpdateUsedState(BankManager.Instance.ContinueUsed.Value);
    }

    public void InitializeForUpgrades()
    {
        UnsubscribeAll();

        currentContext = ContinueContext.Upgrades;
        mode = ContinueMode.Upgrades;

        ForceReset();

        UpgradesManager.Instance.ContinueClicks.OnValueChanged += OnUpgradeClicksChanged;
        UpgradesManager.Instance.ContinueUsed.OnValueChanged += OnUpgradeUsedChanged;

        UpdateClicksUI(UpgradesManager.Instance.ContinueClicks.Value);
        UpdateUsedState(UpgradesManager.Instance.ContinueUsed.Value);
    }

    private void OnDestroy()
    {
        UnsubscribeAll();
    }

    private void OnDisable()
    {
        UnsubscribeAll();
    }

    private void UnsubscribeAll()
    {
        if (BankManager.Instance != null)
        {
            BankManager.Instance.ContinueClicks.OnValueChanged -= OnBankClicksChanged;
            BankManager.Instance.ContinueUsed.OnValueChanged -= OnBankUsedChanged;
        }

        if (UpgradesManager.Instance != null)
        {
            UpgradesManager.Instance.ContinueClicks.OnValueChanged -= OnUpgradeClicksChanged;
            UpgradesManager.Instance.ContinueUsed.OnValueChanged -= OnUpgradeUsedChanged;
        }

        currentContext = ContinueContext.None;
    }

    private void OnBankClicksChanged(int oldValue, int newValue) => UpdateClicksUI(newValue);
    private void OnBankUsedChanged(bool oldValue, bool newValue) => UpdateUsedState(newValue);

    private void OnUpgradeClicksChanged(int oldValue, int newValue) => UpdateClicksUI(newValue);
    private void OnUpgradeUsedChanged(bool oldValue, bool newValue) => UpdateUsedState(newValue);

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (!CanActivate || HasVotedLocally) return;
        HasVotedLocally = true;

        if (mode == ContinueMode.Bank)
            BankManager.Instance.RegisterClickServerRpc(EconomyOptionType.Continue);
        else
            UpgradesManager.Instance.RegisterContinueClickServerRpc();
    }
    
    public enum ContinueMode
    {
        Bank,
        Upgrades
    }

    private ContinueMode mode;

    public override void Activate()
    {
        base.Activate();

        if (UIManager.Instance != null)
        {
            UIManager.Instance.Bank.SetActive(false);
            UIManager.Instance.Upgrades.SetActive(true);
        }

        ForceReset();
    }
}