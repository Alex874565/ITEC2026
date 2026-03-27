using Unity.Netcode;
using System;

public class GameManager : NetworkBehaviour
{
    private NetworkVariable<bool> gamePaused;
    private NetworkVariable<int> currentWave;
    
    public Action OnWaveStarted;
    public Action OnWaveEnded;
    
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return; // 🔥 IMPORTANT: server initializes
        
        gamePaused = new NetworkVariable<bool>(true);
        currentWave = new NetworkVariable<int>(0);
    }
    
    public void StartGame()
    {
        gamePaused.Value = false;
        currentWave.Value = 0;
        StartWave();
    }
    
    public void StartWave()
    {
        OnWaveStarted?.Invoke();
    }
}