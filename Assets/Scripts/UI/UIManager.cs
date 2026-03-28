using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    public GameObject HUD;
    public GameObject Lobby;
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
        titleAnimation.ShowTitle();
        Invoke(nameof(SetDefaultButton), 0.01f);
    }

    void SetDefaultButton()
    {
        UISelectionManager.Instance.SetDefault(startButton);
    }
}