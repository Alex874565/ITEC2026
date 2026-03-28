using Unity.Netcode;
using UnityEngine;

public class PlayerBehaviour : NetworkBehaviour
{
    public PlayerInventory Inventory;
    
    public int PlayerNumber = 0;
    
    public override void OnNetworkSpawn()
    {
        Inventory = FindFirstObjectByType<PlayerInventory>(FindObjectsInactive.Include);
        Inventory.PlayerBehaviour = this;
        PlayerNumber = IsServer ? 1 : 2;
        GameManager.Instance.CurrentWave.OnValueChanged += OnWaveStarted;
        UIManager.Instance.HUD.GetComponent<HUDManager>().Player = this;
    }
    
    public override void OnNetworkDespawn()
    {
        GameManager.Instance.CurrentWave.OnValueChanged -= OnWaveStarted;
    }
    
    public void OnWaveStarted(int oldValue, int newValue)
    {
        if (!IsOwner) return;
        InitializeInventory(newValue);
    }
    
    public void InitializeInventory(int civiliansCount)
    {
        if (!IsOwner) return;
        Inventory.Initialize(civiliansCount);
    }
    
}