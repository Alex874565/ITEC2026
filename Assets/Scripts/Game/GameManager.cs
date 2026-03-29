using Unity.Netcode;
using UnityEngine;
using System;
using DG.Tweening;

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

    public NetworkVariable<int> TotalPoints = new(0);

    public NetworkVariable<int> InvestmentReturn = new(0);
    
    public NetworkVariable<int> Debt = new(0);

    public event Action OnEndWave;

    [Header("Score Progression")]
    [SerializeField] private AnimationCurve requiredScoreCurve = new AnimationCurve(
        new Keyframe(1, 10),
        new Keyframe(2, 20),
        new Keyframe(3, 35),
        new Keyframe(4, 55),
        new Keyframe(5, 80)
    );

    private bool isOptionsActive = false;

    public event EventHandler OnOptionsActive;
    public event EventHandler OnOptionsInactive;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        GameInput.Instance.OnEscapeAction += GameInput_OnEscapeAction;
    }

    private void GameInput_OnEscapeAction(object sender, EventArgs e)
    {
        Debug.Log("esc");
        isOptionsActive = !isOptionsActive;
        if (isOptionsActive)
        {
            OnOptionsActive?.Invoke(this, EventArgs.Empty);
        } else
        {
            OnOptionsInactive?.Invoke(this, EventArgs.Empty);
        }
    }

    public override void OnNetworkSpawn()
    {
        GameStarted.OnValueChanged += ToggleUIs;
        CurrentWave.OnValueChanged += OnWaveStarted;
        Score.OnValueChanged += UpdateScore;
        RequiredScore.OnValueChanged += UpdateScore;
        TotalPoints.OnValueChanged += UpdateTotalPoints;
        Debt.OnValueChanged += UpdateDebt;
        InvestmentReturn.OnValueChanged += UpdateReturn;

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
        TotalPoints.OnValueChanged -= UpdateTotalPoints;
        Debt.OnValueChanged -= UpdateDebt;
        InvestmentReturn.OnValueChanged -= UpdateReturn;

        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback -= OnClientChanged;
            NetworkManager.OnClientDisconnectCallback -= OnClientChanged;
        }

        
        if (GridManager.Instance != null)
            CurrentWave.OnValueChanged -= GridManager.Instance.OnWaveStarted;
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

    private void UpdateTotalPoints(int oldValue, int newValue)
    {
        HUDManager hudManager = UIManager.Instance.HUD.GetComponent<HUDManager>();
        hudManager.Points.text = $"{newValue}";
    }

    private void UpdateDebt(int oldValue, int newValue)
    {
        HUDManager hudManager = UIManager.Instance.HUD.GetComponent<HUDManager>();
        hudManager.Debt.text = $"{newValue}";
    }

    private void UpdateReturn(int oldValue, int newValue)
    {
        HUDManager hudManager = UIManager.Instance.HUD.GetComponent<HUDManager>();
        hudManager.Investment.text = $"{newValue}";
    }

    private int GetRequiredScoreForWave(int wave)
    {
        return Mathf.RoundToInt(requiredScoreCurve.Evaluate(wave));
    }

    public void StartGame()
    {
        if (!IsServer || !IsSpawned)
            return;

        CloseTutorialClientRpc();

        if (GridManager.Instance != null)
            CurrentWave.OnValueChanged += GridManager.Instance.OnWaveStarted;

        GamePaused.Value = false;
        GameStarted.Value = true;
        CurrentWave.Value = 1;
    }

    [ClientRpc]
    public void CloseTutorialClientRpc()
    {
        AudioManager.Instance.PlayMenuChangeSFX();
        UIManager.Instance.Tutorial.SetActive(false);
    }

    [ClientRpc]
    public void EndGameClientRpc()
    {
        LoseUI.Instance.Show(CurrentWave.Value);
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

    public void StartNextWave()
    {
        CurrentWave.Value += 1;
    }

    public void EndWave()
    {
        if (!IsServer)
            return;

        bool success = Score.Value > RequiredScore.Value;

        if (!success)
        {
            EndWaveClientRpc(false, 0, 0, 0, 0, 0);
            return;
        }

        int scoreValue = Score.Value;
        int requiredScoreValue = RequiredScore.Value;
        int investmentReturnValue = InvestmentReturn.Value;
        int debtValue = Debt.Value;

        int bonus = scoreValue - requiredScoreValue + investmentReturnValue - debtValue;
        int totalPointsBefore = TotalPoints.Value;
        int totalPointsAfter = totalPointsBefore + bonus;

        TotalPoints.Value = totalPointsAfter;
        InvestmentReturn.Value = 0;
        Debt.Value = 0;

        EndWaveClientRpc(
            true,
            scoreValue,
            requiredScoreValue,
            investmentReturnValue,
            debtValue,
            totalPointsBefore
        );
    }

    [ClientRpc]
    private void EndWaveClientRpc(
        bool success,
        int scoreValue,
        int requiredScoreValue,
        int investmentReturnValue,
        int debtValue,
        int totalPointsBefore
    )
    {
        if (!success)
        {
            EndGameClientRpc();
            return;
        }

        OnEndWave?.Invoke();

        int bonus = scoreValue - requiredScoreValue + investmentReturnValue - debtValue;
        int totalPointsAfter = totalPointsBefore + bonus;

        UIManager.Instance.PlayEndWaveSequence(
            scoreValue,
            requiredScoreValue,
            investmentReturnValue,
            debtValue,
            bonus,
            totalPointsBefore,
            totalPointsAfter
        );
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