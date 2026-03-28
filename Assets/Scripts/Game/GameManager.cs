using Unity.Netcode;
using System;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public NetworkVariable<bool> GamePaused = new(true);
    public NetworkVariable<int> CurrentWave = new(0);
    public NetworkVariable<bool> GameStarted = new(false);

    public NetworkVariable<int> PlayerCount = new(0);
    public NetworkVariable<int> ActivePlayer = new(0);

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
        GameStarted.OnValueChanged += ToggleUIs;
        CurrentWave.OnValueChanged += OnWaveStarted;
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += OnClientChanged;
            NetworkManager.OnClientDisconnectCallback += OnClientChanged;
        }
    }

    public override void OnNetworkDespawn()
    {
        GameStarted.OnValueChanged -= ToggleUIs;
        CurrentWave.OnValueChanged -= OnWaveStarted;

        if (IsServer){
            NetworkManager.OnClientConnectedCallback -= OnClientChanged;
            NetworkManager.OnClientDisconnectCallback -= OnClientChanged;
        }
    }
    
    private void OnClientChanged(ulong clientId)
    {
        UpdatePlayerCount();
    }

    private void UpdatePlayerCount()
    {
        PlayerCount.Value = NetworkManager.Singleton.ConnectedClients.Count;
    }

    public void StartGame()
    {
        if (!IsServer || !IsSpawned)
            return;

        GamePaused.Value = false;
        GameStarted.Value = true;
        CurrentWave.Value = 1;
    }

    private void ToggleUIs(bool oldValue, bool newValue)
    {
        if (UIManager.Instance == null)
            return;

        UIManager.Instance.HUD.SetActive(newValue);
        UIManager.Instance.Lobby.SetActive(!newValue);
    }

    private void OnWaveStarted(int oldValue, int newValue)
    {
        if (!IsServer || !IsSpawned)
            return;
        
        ActivePlayer.Value = 1;
    }

    public void ChangeActivePlayer()
    {
        if (IsServer)
        {
            ChangeActivePlayerServerLogic();
        }
        else
        {
            RequestChangeActivePlayerRpc();
        }
    }

    [Rpc(SendTo.Server)]
    private void RequestChangeActivePlayerRpc()
    {
        ChangeActivePlayerServerLogic();
    }

    private void ChangeActivePlayerServerLogic()
    {
        if (!IsServer || !IsSpawned)
            return;

        ActivePlayer.Value = (ActivePlayer.Value == 1) ? 2 : 1;
    }
}