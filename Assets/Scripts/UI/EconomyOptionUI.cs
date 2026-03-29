using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class EconomyOptionUI : OptionUI
{
    [SerializeField] private TextMeshProUGUI initialValueText;
    [SerializeField] private TextMeshProUGUI multipliedValueText;

    public EconomyOptionType OptionType;

    public override void Initialize()
    {
        // 1. Link to BankManager
        if (OptionType == EconomyOptionType.Invest) BankManager.Instance.InvestOption = this;
        else BankManager.Instance.LoanOption = this;

        ForceReset();

        // 2. Bind Clicks and Used State
        var clicksVar = OptionType == EconomyOptionType.Invest ? BankManager.Instance.InvestClicks : BankManager.Instance.LoanClicks;
        var usedVar = OptionType == EconomyOptionType.Invest ? BankManager.Instance.InvestUsed : BankManager.Instance.LoanUsed;

        clicksVar.OnValueChanged += (o, n) => UpdateClicksUI(n);
        usedVar.OnValueChanged += (o, n) => UpdateUsedState(n);

        // 3. Bind Math Variables (Initialize scores)
        if (OptionType == EconomyOptionType.Invest)
            BindMath(BankManager.Instance.InvestInitial, BankManager.Instance.InvestMult, BankManager.Instance.InvestFinal);
        else
            BindMath(BankManager.Instance.LoanInitial, BankManager.Instance.LoanMult, BankManager.Instance.LoanFinal);

        // Sync visuals
        UpdateClicksUI(clicksVar.Value);
        UpdateUsedState(usedVar.Value);
    }

    private void BindMath(NetworkVariable<int> init, NetworkVariable<float> mult, NetworkVariable<int> final)
    {
        init.OnValueChanged += (o, n) => RefreshEconomyUI(n, mult.Value, final.Value);
        mult.OnValueChanged += (o, n) => RefreshEconomyUI(init.Value, n, final.Value);
        final.OnValueChanged += (o, n) => RefreshEconomyUI(init.Value, mult.Value, n);
        
        RefreshEconomyUI(init.Value, mult.Value, final.Value);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (!CanActivate || HasVotedLocally) return;
        HasVotedLocally = true;
        UpdateUsedState(false); // Update cover locally
        BankManager.Instance.RegisterClickServerRpc(OptionType);
    }

    public override void Activate()
    {
        base.Activate();
        if (NetworkManager.Singleton.IsServer) ApplyEconomyChanges();
    }

    private void ApplyEconomyChanges()
    {
        int init = OptionType == EconomyOptionType.Invest ? BankManager.Instance.InvestInitial.Value : BankManager.Instance.LoanInitial.Value;
        int final = OptionType == EconomyOptionType.Invest ? BankManager.Instance.InvestFinal.Value : BankManager.Instance.LoanFinal.Value;

        if (OptionType == EconomyOptionType.TakeLoan) {
            GameManager.Instance.TotalPoints.Value += init;
            GameManager.Instance.Debt.Value += final;
        } else {
            GameManager.Instance.TotalPoints.Value -= init;
            GameManager.Instance.InvestmentReturn.Value += final;
        }
    }

    private void RefreshEconomyUI(int i, float m, int f)
    {
        initialValueText.text = OptionType == EconomyOptionType.TakeLoan ? $"Borrow {i}" : $"Invest {i}";
        multipliedValueText.text = OptionType == EconomyOptionType.TakeLoan ? $"Repay {f} (x{m:F2})" : $"Receive {f} (x{m:F2})";
    }
}