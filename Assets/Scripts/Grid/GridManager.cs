using Unity.Netcode;
using UnityEngine;

public class GridManager : NetworkBehaviour
{
    public int CiviliansTargetCount;
    
    public NetworkVariable<ActiveTraitCivilians> ActiveTraitCivilians; 
    
    
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            GameManager.Instance.CurrentWave.OnValueChanged += OnWaveStarted;
        }
    }
    
    public void OnWaveStarted(int oldValue, int newValue)
    {
        if (!IsServer) return;
        
        CiviliansTargetCount = newValue;
    }
}