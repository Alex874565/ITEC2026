using Unity.Netcode;
using UnityEngine;

public class PlayerBehaviour : NetworkBehaviour
{
    public PlayerInventory Inventory;
    
    private StartButtonUI _startButtonUI;
    
    public override void OnNetworkSpawn()
    {
        Inventory = FindFirstObjectByType<PlayerInventory>();
        _startButtonUI = FindFirstObjectByType<StartButtonUI>();
        GameManager.Instance.CurrentWave.OnValueChanged += OnWaveStarted;
    }
    
    public override void OnNetworkDespawn()
    {
        GameManager.Instance.CurrentWave.OnValueChanged -= OnWaveStarted;
    }
    
    public void OnWaveStarted(int oldValue, int newValue)
    {
        if (!IsOwner) return;
        InitializeInventory(2);
    }
    
    public void InitializeInventory(int civiliansCount)
    {
        if (!IsOwner) return;
        Inventory.Initialize(civiliansCount);
    }
    
}