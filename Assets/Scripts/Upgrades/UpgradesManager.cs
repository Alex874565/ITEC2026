using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UpgradesManager : NetworkBehaviour
{
    public static UpgradesManager Instance { get; private set; }

    [Header("Sync Data")]
    public NetworkList<ModifierUpgrade> UpgradeSlot1 = new();
    public NetworkList<ModifierUpgrade> UpgradeSlot2 = new();
    public NetworkList<ModifierUpgrade> UpgradeSlot3 = new();

    public NetworkVariable<int> UpgradePrice1 = new(0);
    public NetworkVariable<int> UpgradePrice2 = new(0);
    public NetworkVariable<int> UpgradePrice3 = new(0);

    [Header("Vote State")]
    public NetworkVariable<int> Slot1Clicks = new(0);
    public NetworkVariable<int> Slot2Clicks = new(0);
    public NetworkVariable<int> Slot3Clicks = new(0);
    public NetworkVariable<int> ContinueClicks = new(0);

    public NetworkVariable<bool> Slot1Used = new(false);
    public NetworkVariable<bool> Slot2Used = new(false);
    public NetworkVariable<bool> Slot3Used = new(false);
    public NetworkVariable<bool> ContinueUsed = new(false);

    private readonly HashSet<ulong> slot1Voters = new();
    private readonly HashSet<ulong> slot2Voters = new();
    private readonly HashSet<ulong> slot3Voters = new();
    private readonly HashSet<ulong> continueVoters = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            Initialize();
    }

    public void Initialize()
    {
        if (!IsServer) return;

        slot1Voters.Clear();
        slot2Voters.Clear();
        slot3Voters.Clear();
        continueVoters.Clear();

        Slot1Clicks.Value = 0;
        Slot2Clicks.Value = 0;
        Slot3Clicks.Value = 0;
        ContinueClicks.Value = 0;

        Slot1Used.Value = false;
        Slot2Used.Value = false;
        Slot3Used.Value = false;
        ContinueUsed.Value = false;

        GenerateUpgrades();
        ForceResetUIClientRpc();
    }

    [ClientRpc]
    private void ForceResetUIClientRpc()
    {
        FindFirstObjectByType<UpgradesUI>()?.InitializeUI();
    }

    public void GenerateUpgrades()
    {
        if (!IsServer) return;

        UpgradeSlot1.Clear();
        UpgradeSlot2.Clear();
        UpgradeSlot3.Clear();

        UpgradePrice1.Value = ModifiersManager.Instance.GetUpgradePrice();
        UpgradePrice2.Value = ModifiersManager.Instance.GetUpgradePrice();
        UpgradePrice3.Value = ModifiersManager.Instance.GetUpgradePrice();

        int modifiersCount = Random.Range(1, 4);
        for (int i = 0; i < modifiersCount; i++)
            UpgradeSlot1.Add(ModifiersManager.Instance.GenerateRandomUpgrade());

        modifiersCount = Random.Range(1, 4);
        for (int i = 0; i < modifiersCount; i++)
            UpgradeSlot2.Add(ModifiersManager.Instance.GenerateRandomUpgrade());

        modifiersCount = Random.Range(1, 4);
        for (int i = 0; i < modifiersCount; i++)
            UpgradeSlot3.Add(ModifiersManager.Instance.GenerateRandomUpgrade());
    }

    [ServerRpc(RequireOwnership = false)]
    public void RegisterUpgradeClickServerRpc(int slotIndex, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        Debug.Log($"[UpgradesManager] Server received click from {clientId} for slot {slotIndex}");

        switch (slotIndex)
        {
            case 1:
                if (!Slot1Used.Value && slot1Voters.Add(clientId))
                    Slot1Clicks.Value = slot1Voters.Count;
                break;

            case 2:
                if (!Slot2Used.Value && slot2Voters.Add(clientId))
                    Slot2Clicks.Value = slot2Voters.Count;
                break;

            case 3:
                if (!Slot3Used.Value && slot3Voters.Add(clientId))
                    Slot3Clicks.Value = slot3Voters.Count;
                break;
        }

        CheckUpgradeActivation(slotIndex);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RegisterContinueClickServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (!ContinueUsed.Value && continueVoters.Add(clientId))
        {
            ContinueClicks.Value = continueVoters.Count;
            CheckContinueActivation();
        }
    }

    private void CheckUpgradeActivation(int slotIndex)
    {
        int required = GameManager.Instance.PlayerCount.Value;

        switch (slotIndex)
        {
            case 1:
                if (Slot1Clicks.Value >= required && !Slot1Used.Value)
                {
                    Slot1Used.Value = true;
                    ApplyUpgradeSlot(UpgradeSlot1, UpgradePrice1.Value);
                }
                break;

            case 2:
                if (Slot2Clicks.Value >= required && !Slot2Used.Value)
                {
                    Slot2Used.Value = true;
                    ApplyUpgradeSlot(UpgradeSlot2, UpgradePrice2.Value);
                }
                break;

            case 3:
                if (Slot3Clicks.Value >= required && !Slot3Used.Value)
                {
                    Slot3Used.Value = true;
                    ApplyUpgradeSlot(UpgradeSlot3, UpgradePrice3.Value);
                }
                break;
        }
    }

    private void CheckContinueActivation()
    {
        int required = GameManager.Instance.PlayerCount.Value;

        if (ContinueClicks.Value >= required && !ContinueUsed.Value)
        {
            ContinueUsed.Value = true;
            EndUpgradePhaseClientRpc();
            GameManager.Instance.StartNextWave();
        }
    }

    private void ApplyUpgradeSlot(NetworkList<ModifierUpgrade> upgrades, int price)
    {
        if (!IsServer) return;

        foreach (ModifierUpgrade mod in upgrades)
            ModifiersManager.Instance.ApplyUpgrade(mod);

        GameManager.Instance.TotalPoints.Value -= price;

        ActivateChosenUpgradeClientRpc();
    }

    [ClientRpc]
    private void ActivateChosenUpgradeClientRpc()
    {
        
    }

    [ClientRpc]
    private void EndUpgradePhaseClientRpc()
    {
        UIManager.Instance.Upgrades.SetActive(false);
        AudioManager.Instance.PlayMenuChangeSFX();
    }
}