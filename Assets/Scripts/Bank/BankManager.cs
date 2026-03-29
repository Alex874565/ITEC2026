using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BankManager : NetworkBehaviour
{
    public static BankManager Instance { get; private set; }

    [Header("Network Data")]
    public NetworkVariable<int> InvestInitial = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> InvestFinal = new(0);
    public NetworkVariable<float> InvestMult = new(1f);
    public NetworkVariable<int> LoanInitial = new(0);
    public NetworkVariable<int> LoanFinal = new(0);
    public NetworkVariable<float> LoanMult = new(1f);

    [Header("Status Sync")]
    public NetworkVariable<int> InvestClicks = new(0);
    public NetworkVariable<int> LoanClicks = new(0);
    public NetworkVariable<int> ContinueClicks = new(0);
    public NetworkVariable<bool> InvestUsed = new(false);
    public NetworkVariable<bool> LoanUsed = new(false);
    public NetworkVariable<bool> ContinueUsed = new(false);

    [Header("References")]
    public EconomyOptionUI InvestOption;
    public EconomyOptionUI LoanOption;
    public EconomyContinueButton ContinueButton;

    private HashSet<ulong> investVoters = new HashSet<ulong>();
    private HashSet<ulong> loanVoters = new HashSet<ulong>();
    private HashSet<ulong> continueVoters = new HashSet<ulong>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer) Initialize();
    }

    public void Initialize()
    {
        if (!IsServer) return;

        investVoters.Clear();
        loanVoters.Clear();
        continueVoters.Clear();
        
        InvestClicks.Value = 0;
        LoanClicks.Value = 0;
        ContinueClicks.Value = 0;
        
        InvestUsed.Value = false;
        LoanUsed.Value = false;
        ContinueUsed.Value = false;

        GenerateOptionValues();
        ForceResetUIClientRpc();
    }

    [ClientRpc]
    private void ForceResetUIClientRpc()
    {
        InvestOption?.ForceReset();
        LoanOption?.ForceReset();
        ContinueButton?.ForceReset();

        if (ContinueButton == null)
        {
            ContinueButton = UIManager.Instance.Bank.GetComponentInChildren<EconomyContinueButton>();
        }
        Debug.Log($"[BankManager] Forcing UI reset on clients. ContinueButton found: {ContinueButton != null}");
        ContinueButton?.Initialize();
    }

    private void GenerateOptionValues()
    {
        int points = GameManager.Instance.TotalPoints.Value;
        float iPercent = Random.Range(0.25f, 0.75f);
        InvestInitial.Value = Mathf.Max(1, Mathf.RoundToInt(points * iPercent));
        InvestMult.Value = 1.5f;
        InvestFinal.Value = Mathf.RoundToInt(InvestInitial.Value * InvestMult.Value);

        float lPercent = Random.Range(0.25f, 0.75f);
        LoanInitial.Value = Mathf.Max(1, Mathf.RoundToInt(points * lPercent));
        LoanMult.Value = 1.2f;
        LoanFinal.Value = Mathf.RoundToInt(LoanInitial.Value * LoanMult.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RegisterClickServerRpc(EconomyOptionType type, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        Debug.Log($"[BankManager] Server received click from {clientId} for {type}");

        switch (type)
        {
            case EconomyOptionType.Invest:
                if (!InvestUsed.Value && investVoters.Add(clientId)) InvestClicks.Value = investVoters.Count;
                break;
            case EconomyOptionType.TakeLoan:
                if (!LoanUsed.Value && loanVoters.Add(clientId)) LoanClicks.Value = loanVoters.Count;
                break;
            case EconomyOptionType.Continue:
                if (!ContinueUsed.Value && continueVoters.Add(clientId)) ContinueClicks.Value = continueVoters.Count;
                break;
        }
        CheckActivation(type);
    }

    private void CheckActivation(EconomyOptionType type)
    {
        int required = GameManager.Instance.PlayerCount.Value;
        if (type == EconomyOptionType.Invest && InvestClicks.Value >= required) { InvestUsed.Value = true; InvestOption.Activate(); }
        if (type == EconomyOptionType.TakeLoan && LoanClicks.Value >= required) { LoanUsed.Value = true; LoanOption.Activate(); }
        if (type == EconomyOptionType.Continue && ContinueClicks.Value >= required) { ContinueUsed.Value = true; ActivateContinueClientRpc(); }
    }

    [ClientRpc]
    private void ActivateContinueClientRpc()
    {
        ContinueButton.Activate();
    }
}