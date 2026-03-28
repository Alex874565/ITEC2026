using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    public static SettingsUI Instance { get; private set; }

    [SerializeField] private Button closeButton;
    [SerializeField] private TitleAnimation animator; // assign in inspector
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private HeaderAnimator headerAnimator;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        closeButton.onClick.AddListener(Hide);
    }

    private void Start()
    {
        // Start hidden (UIElementAnimator already sets alpha 0 & scale 0)
        gameObject.SetActive(false);

        // var audio = ServiceLocator.Instance.AudioManager;

        // masterSlider.value = audio.GetMasterVolume();
        // musicSlider.value = audio.GetMusicVolume();
        // sfxSlider.value = audio.GetSFXVolume();

        // masterSlider.onValueChanged.AddListener(audio.SetMasterVolume);
        // musicSlider.onValueChanged.AddListener(audio.SetMusicVolume);
        // sfxSlider.onValueChanged.AddListener(audio.SetSFXVolume);
    }

    public void Show()
    {
        gameObject.SetActive(true); // must activate first for animation to work
        headerAnimator.ShowHeader();
        animator.Show();             // animate in
    }

    public void Hide()
    {
        // Animate out, then deactivate
        animator.Hide();
        // Deactivate after animation duration
        DOVirtual.DelayedCall(animator.Duration, () =>
        {
            gameObject.SetActive(false);
        });
    }
}