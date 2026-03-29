using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoseUI : MonoBehaviour
{
    public static LoseUI Instance { get; private set; }

    [SerializeField] private MenuStaggerAnimation stagger;

    [Header("UI")]
    [SerializeField] private Button hubButton;
    [SerializeField] private TextMeshProUGUI lvlReachedText;

    [SerializeField] private int lvlReached;
    [SerializeField] private HeaderAnimator headerAnimator;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        hubButton.onClick.AddListener(() =>
        {
            Debug.Log("hub");
            //SceneManager.LoadScene("HubScene");
        });
    }

    private void Start()
    {
        //gameObject.SetActive(false);
        LoseUI.Instance.Show(lvlReached);
    }

    public void Hide()
    {
        stagger.CloseMenu(() =>
        {
            gameObject.SetActive(false);
        });
    }

    // Call this from your GameManager when the player loses:
// LoseUI.Instance.Show(30); 

    public void Show(int level)
    {
        headerAnimator.ShowHeader();

        lvlReached = level; // Set the internal variable
        
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
        
        // 1. Reset state
        cg.alpha = 0;
        lvlReachedText.text = "0"; 
        transform.localScale = Vector3.one * 0.85f;
        gameObject.SetActive(true);

        // 2. Animate Entry
        Sequence entrySequence = DOTween.Sequence().SetUpdate(true);
        entrySequence.Join(cg.DOFade(1f, 0.5f));
        entrySequence.Join(transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack));
        
        entrySequence.OnComplete(() => 
        {
            // 3. Trigger the stagger (Ensure this calls the action!)
            stagger.OpenMenu(
                onLvlShown: () => AnimateLvl() 
            );
        });
    }

    private void AnimateLvl()
    {
        // DEBUG: See what value we actually have here
        Debug.Log($"AnimateLvl called. lvlReached is: {lvlReached}");

        lvlReachedText.text = "0";

        // If lvlReached is 0, the tween finishes instantly.
        if (lvlReached <= 0) {
            lvlReachedText.text = "0";
            return;
        }

        DOVirtual.Float(0, lvlReached, 1.2f, value =>
        {
            lvlReachedText.text = Mathf.RoundToInt(value).ToString();
        })
        .SetEase(Ease.OutCubic)
        .SetUpdate(true) // Crucial if Time.timeScale is 0
        .OnComplete(() =>
        {
            lvlReachedText.transform
                .DOPunchScale(Vector3.one * 0.2f, 0.3f, 10, 1)
                .SetUpdate(true);
        });
    }

}
