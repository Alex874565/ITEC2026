using Unity.Netcode;
using System;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    private NetworkVariable<bool> gamePaused = new(true);
    private NetworkVariable<int> currentWave = new(0);
    private NetworkVariable<bool> gameStarted = new(false);
    
    public Action OnWaveStarted;
    public Action OnWaveEnded;
    
    private void Awake()
    {
        if (!IsOwner) return;
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    public override void OnNetworkSpawn()
    {
        gameStarted.OnValueChanged += ToggleUIs;
    }
    
    public override void OnNetworkDespawn()
    {
        gameStarted.OnValueChanged -= ToggleUIs;
    }
    
    public void StartGame()
    {
        if(!IsOwner) return; // 🔥 IMPORTANT: only server can start the game
        gamePaused.Value = false;
        currentWave.Value = 0;
        gameStarted.Value = true;
        StartWave();
    }

    private void ToggleUIs(bool oldValue, bool newValue)
    {
        UIManager.Instance.HUD.SetActive(true);
        UIManager.Instance.Lobby.SetActive(false);
    }
    
    public void StartWave()
    {
        if(!IsOwner) return; // 🔥 IMPORTANT: only server can start the wave
        OnWaveStarted?.Invoke();
    }
}