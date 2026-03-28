using TMPro;
using Unity.Netcode;
using UnityEngine;

public enum EconomyOptionType
{
    TakeLoan,
    Invest
}

public class EconomyOptionUI : OptionUI
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI initialValueText;
    [SerializeField] private TextMeshProUGUI multipliedValueText;

    [Header("Ranges")]
    [SerializeField] private Vector2 percentageRange = new Vector2(0.25f, 0.75f);
    [SerializeField] private Vector2 multiplierRange = new Vector2(1.25f, 1.75f);

    public NetworkVariable<int> OptionType = new((int)EconomyOptionType.TakeLoan);
    public NetworkVariable<int> InitialValue = new(0);
    public NetworkVariable<float> Multiplier = new(1f);
    public NetworkVariable<int> FinalValue = new(0);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        InitialValue.OnValueChanged += UpdateInitialValueUI;
        FinalValue.OnValueChanged += UpdateFinalValueUI;

        UpdateInitialValueUI(InitialValue.Value, InitialValue.Value);
        UpdateFinalValueUI(FinalValue.Value, FinalValue.Value);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        InitialValue.OnValueChanged -= UpdateInitialValueUI;
        FinalValue.OnValueChanged -= UpdateFinalValueUI;
    }

    public override void Initialize()
    {
        if (!IsServer)
            return;

        int totalPoints = GameManager.Instance.TotalPoints.Value;

        EconomyOptionType randomType = Random.value < 0.5f
            ? EconomyOptionType.TakeLoan
            : EconomyOptionType.Invest;

        float percent = Random.Range(percentageRange.x, percentageRange.y);
        int initial = Mathf.Max(1, Mathf.RoundToInt(totalPoints * percent));

        float multiplier = Random.Range(multiplierRange.x, multiplierRange.y);
        multiplier = Mathf.Round(multiplier * 100f) / 100f;

        int final = Mathf.RoundToInt(initial * multiplier);

        OptionType.Value = (int)randomType;
        InitialValue.Value = initial;
        Multiplier.Value = multiplier;
        FinalValue.Value = final;

        Price.Value = 0;
        CanActivate.Value = true;
        Clicks.Value = 0;
    }

    public override void Activate()
    {
        if (!IsServer)
            return;

        EconomyOptionType type = (EconomyOptionType)OptionType.Value;

        switch (type)
        {
            case EconomyOptionType.TakeLoan:
                // Example:
                // gain InitialValue now, repay FinalValue later
                GameManager.Instance.TotalPoints.Value += InitialValue.Value;
                GameManager.Instance.Debt.Value += FinalValue.Value;
                break;

            case EconomyOptionType.Invest:
                // Example:
                // pay InitialValue now, receive FinalValue later
                GameManager.Instance.TotalPoints.Value -= InitialValue.Value;
                GameManager.Instance.InvestmentReturn.Value += FinalValue.Value;
                break;
        }

        base.Activate();
    }

    private void UpdateInitialValueUI(int oldValue, int newValue)
    {
        if (initialValueText != null)
            if(OptionType.Value == (int)EconomyOptionType.TakeLoan)
                initialValueText.text = $"Borrow {newValue}";
            else
                initialValueText.text = $"Invest {newValue}";
    }

    private void UpdateFinalValueUI(int oldValue, int newValue)
    {
        if (multipliedValueText != null)
            if(OptionType.Value == (int)EconomyOptionType.TakeLoan)
                multipliedValueText.text = $"Repay {newValue} (x{Multiplier.Value})";
            else
                multipliedValueText.text = $"Receive {newValue} (x{Multiplier.Value})";
    }
}