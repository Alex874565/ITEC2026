using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public NetworkVariable<bool> GamePaused = new(true);
    public NetworkVariable<int> CurrentWave = new(0);
    public NetworkVariable<bool> GameStarted = new(false);

    public NetworkVariable<int> PlayerCount = new(0);
    public NetworkVariable<int> ActivePlayer = new(0);
    public NetworkVariable<int> Score = new(0);

    private NetworkVariable<int> RequiredScore = new(0);

    [Header("Score Progression")]
    [SerializeField] private AnimationCurve requiredScoreCurve = new AnimationCurve(
        new Keyframe(1, 10),
        new Keyframe(2, 20),
        new Keyframe(3, 35),
        new Keyframe(4, 55),
        new Keyframe(5, 80)
    );

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
        Score.OnValueChanged += UpdateScore;
        RequiredScore.OnValueChanged += UpdateScore;

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
        Score.OnValueChanged -= UpdateScore;
        RequiredScore.OnValueChanged -= UpdateScore;

        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback -= OnClientChanged;
            NetworkManager.OnClientDisconnectCallback -= OnClientChanged;
        }

        EndGame();
    }

    private void OnClientChanged(ulong clientId)
    {
        UpdatePlayerCount();
    }

    private void UpdatePlayerCount()
    {
        PlayerCount.Value = NetworkManager.Singleton.ConnectedClients.Count;
    }

    private void UpdateScore(int oldValue, int newValue)
    {
        HUDManager hudManager = UIManager.Instance.HUD.GetComponent<HUDManager>();
        hudManager.ScoreText.text = $"{Score.Value}/{RequiredScore.Value}";
    }

    private int GetRequiredScoreForWave(int wave)
    {
        return Mathf.RoundToInt(requiredScoreCurve.Evaluate(wave));
    }

    public void StartGame()
    {
        if (!IsServer || !IsSpawned)
            return;

        if (GridManager.Instance != null)
            CurrentWave.OnValueChanged += GridManager.Instance.OnWaveStarted;

        GamePaused.Value = false;
        GameStarted.Value = true;
        CurrentWave.Value = 1;
    }

    public void EndGame()
    {
        if (GridManager.Instance != null)
            CurrentWave.OnValueChanged -= GridManager.Instance.OnWaveStarted;
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
        Score.Value = 0;
        RequiredScore.Value = GetRequiredScoreForWave(newValue);
    }

    public void EndWave()
    {
        
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