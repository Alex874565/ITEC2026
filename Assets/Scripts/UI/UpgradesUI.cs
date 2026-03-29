using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class UpgradesUI : MonoBehaviour
{
    public UpgradeOptionUI Slot1;
    public UpgradeOptionUI Slot2;
    public UpgradeOptionUI Slot3;
    public EconomyContinueButton ContinueButton;

    public TextMeshProUGUI Money;
    public TextMeshProUGUI Return;
    public TextMeshProUGUI Debt;
    
    private void OnEnable()
    {
        GameManager.Instance.TotalPoints.OnValueChanged += OnTotalPointsChanged;
        GameManager.Instance.Debt.OnValueChanged += OnDebtChanged;
        GameManager.Instance.InvestmentReturn.OnValueChanged += OnInvestmentReturnChanged;

        OnTotalPointsChanged(GameManager.Instance.TotalPoints.Value, GameManager.Instance.TotalPoints.Value);
        OnDebtChanged(GameManager.Instance.Debt.Value, GameManager.Instance.Debt.Value);
        OnInvestmentReturnChanged(GameManager.Instance.InvestmentReturn.Value, GameManager.Instance.InvestmentReturn.Value);
        
        if (NetworkManager.Singleton.IsServer)
        {
            UpgradesManager.Instance.Initialize();
        }
    }

    private void OnDisable()
    {
        GameManager.Instance.TotalPoints.OnValueChanged -= OnTotalPointsChanged;
        GameManager.Instance.Debt.OnValueChanged -= OnDebtChanged;
        GameManager.Instance.InvestmentReturn.OnValueChanged -= OnInvestmentReturnChanged;
    }

    private void OnTotalPointsChanged(int oldValue, int newValue)
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
    
    public void InitializeUI()
    {
        Slot1.Initialize();
        Slot2.Initialize();
        Slot3.Initialize();
        ContinueButton.InitializeForUpgrades();
    }
}