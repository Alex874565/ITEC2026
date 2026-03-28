using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    public GameObject HUD;
    public GameObject Lobby;
    public GameObject Upgrades;
    public GameObject Bank;
    public UIButtonVisual startButton;
    [SerializeField] private TitleAnimation titleAnimation;
    
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
            titleAnimation.ShowTitle();
        Invoke(nameof(SetDefaultButton), 0.01f);
    }

    void SetDefaultButton()
    {
        if(startButton != null)
            UISelectionManager.Instance.SetDefault(startButton);
    }
}