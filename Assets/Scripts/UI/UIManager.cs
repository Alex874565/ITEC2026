using DG.Tweening;
using UnityEngine;
using Unity.Netcode;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    public GameObject HUD;
    public GameObject Lobby;
    public GameObject Upgrades;
    public GameObject Bank;
    public GameObject Tutorial;
    public UIButtonVisual startButton;
    [SerializeField] private TitleAnimation titleAnimation;
    
    [Header("End Wave Animation")]
    [SerializeField] private float bonusCountDuration = 0.8f;
    [SerializeField] private float pointsCountDuration = 0.8f;
    [SerializeField] private float beforeGridClearDelay = 0.2f;
    [SerializeField] private float afterGridClearDelay = 0.5f;
    private Sequence endWaveSequence;
    
    private bool waitForPointsBeforeOpeningBank;
    private int expectedBankPoints;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    void Start()
    {
        if(titleAnimation != null)
            titleAnimation.Show();
        Invoke(nameof(SetDefaultButton), 0.01f);
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TotalPoints.OnValueChanged += OnTotalPointsChanged;
        }
    }
    
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TotalPoints.OnValueChanged -= OnTotalPointsChanged;
        }
    }
    
    private void OnTotalPointsChanged(int oldValue, int newValue)
    {
        if (!waitForPointsBeforeOpeningBank)
            return;

        if (newValue < expectedBankPoints)
            return;

        waitForPointsBeforeOpeningBank = false;
        Bank.SetActive(true);
    }

    void SetDefaultButton()
    {
        if(startButton != null)
            UISelectionManager.Instance.SetDefault(startButton);
    }
    
    public void PlayEndWaveSequence(
        int scoreValue,
        int requiredScoreValue,
        int investmentReturnValue,
        int debtValue,
        int bonus,
        int totalPointsBefore,
        int totalPointsAfter
    )
    {
        if (HUD == null)
            return;

        if (endWaveSequence != null && endWaveSequence.IsActive())
            endWaveSequence.Kill();

        HUDManager hud = HUD.GetComponent<HUDManager>();
        if (hud == null)
            return;

        int visibleScore = scoreValue;
        int visiblePoints = totalPointsBefore;

        hud.ScoreText.text = $"{visibleScore}/{requiredScoreValue}";
        hud.Points.text = $"{visiblePoints}";
        hud.Investment.text = $"{investmentReturnValue}";
        hud.Debt.text = $"{debtValue}";

        Bank.GetComponent<EconomyUI>().Show();

        endWaveSequence = DOTween.Sequence();

        // Animate score down to required/required
        endWaveSequence.Append(
            DOTween.To(
                () => visibleScore,
                x =>
                {
                    visibleScore = x;
                    hud.ScoreText.text = $"{visibleScore}/{requiredScoreValue}";
                },
                requiredScoreValue,
                bonusCountDuration
            ).SetEase(Ease.OutQuad)
        );

        // Animate total points up at the same time
        endWaveSequence.Join(
            DOTween.To(
                () => visiblePoints,
                x =>
                {
                    visiblePoints = x;
                    hud.Points.text = $"{visiblePoints}";
                },
                totalPointsAfter,
                pointsCountDuration
            ).SetEase(Ease.OutQuad)
        );

        // Flash score and points after the transfer finishes
        endWaveSequence.AppendCallback(() =>
        {
            hud.ScoreText.transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 8, 0.8f);
            hud.Points.transform.DOPunchScale(Vector3.one * 0.15f, 0.3f, 8, 0.8f);
        });

        endWaveSequence.AppendInterval(beforeGridClearDelay);

        endWaveSequence.AppendCallback(() =>
        {
            if (GridManager.Instance != null)
            {
                GridManager.Instance.ClearGrid();
            }
        });

        float clearDuration =
            GridManager.Instance != null
                ? GridManager.Instance.GetEstimatedClearDuration()
                : 1.0f;

        endWaveSequence.AppendInterval(clearDuration);
        endWaveSequence.AppendInterval(afterGridClearDelay);

        endWaveSequence.AppendCallback(() =>
        {
            expectedBankPoints = totalPointsAfter;
            waitForPointsBeforeOpeningBank = true;

            if (GameManager.Instance.TotalPoints.Value >= expectedBankPoints)
            {
                waitForPointsBeforeOpeningBank = false;
                Bank.SetActive(true);
            }
        });
    }
}