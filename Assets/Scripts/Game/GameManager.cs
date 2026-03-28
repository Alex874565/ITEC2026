using Unity.Netcode;
using System;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public NetworkVariable<bool> GamePaused = new(true);
    public NetworkVariable<int> CurrentWave = new(0);
    public NetworkVariable<bool> GameStarted = new(false);

    public Action OnWaveStarted;
    public Action OnWaveEnded;

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
    }

    public override void OnNetworkDespawn()
    {
        GameStarted.OnValueChanged -= ToggleUIs;
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
}