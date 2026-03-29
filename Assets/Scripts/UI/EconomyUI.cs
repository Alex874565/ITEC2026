using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class EconomyUI : MonoBehaviour
{
    public TextMeshProUGUI Money;
    public TextMeshProUGUI Return;
    public TextMeshProUGUI Debt;
    
    public EconomyOptionUI InvestOption;
    public EconomyOptionUI LoanOption;
    public EconomyContinueButton ContinueButton;

    [SerializeField] private HeaderAnimator headerAnimator;
    [SerializeField] private MenuStaggerAnimation stagger;
    
    private void OnEnable()
    {
        GameManager.Instance.TotalPoints.OnValueChanged += OnTotalPointsChanged;
        GameManager.Instance.Debt.OnValueChanged += OnDebtChanged;
        GameManager.Instance.InvestmentReturn.OnValueChanged += OnInvestmentReturnChanged;

        OnTotalPointsChanged(GameManager.Instance.TotalPoints.Value, GameManager.Instance.TotalPoints.Value);
        OnDebtChanged(GameManager.Instance.Debt.Value, GameManager.Instance.Debt.Value);
        OnInvestmentReturnChanged(GameManager.Instance.Debt.Value, GameManager.Instance.Debt.Value);
        
        // Just give the references
        BankManager.Instance.InvestOption = InvestOption;
        BankManager.Instance.LoanOption = LoanOption;
        BankManager.Instance.ContinueButton = ContinueButton;

        // Initialize the UI logic
        InvestOption.Initialize();
        LoanOption.Initialize();

        if (NetworkManager.Singleton.IsServer)
        {
            BankManager.Instance.Initialize();
        }
    }

    private void OnDisable()
    {
        GameManager.Instance.TotalPoints.OnValueChanged -= OnTotalPointsChanged;
        GameManager.Instance.Debt.OnValueChanged -= OnDebtChanged;
        GameManager.Instance.InvestmentReturn.OnValueChanged -= OnInvestmentReturnChanged;
    }

    private void OnTotalPointsChanged(int  oldValue, int newValue)
    {
        Money.text = newValue.ToString();
    }

    private void OnDebtChanged(int oldValue, int newValue)
    {
        Debt.text = newValue.ToString();
    }

    private void OnInvestmentReturnChanged(int oldValue, int newValue)
    {
        Return.text = newValue.ToString();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        headerAnimator.ShowHeader();
        stagger.OpenMenu();
    }

    public void Hide()
    {
        stagger.CloseMenu(() =>
        {
            gameObject.SetActive(false);
        });
    }
}