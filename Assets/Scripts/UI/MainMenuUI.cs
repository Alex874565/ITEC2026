using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private MenuStaggerAnimation stagger;
    [SerializeField] private TitleAnimation titleAnimation;

    private void Awake()
    {
        startButton.onClick.AddListener(() =>
        {
            stagger.CloseMenu(() =>
            {
                AudioManager.Instance.PlayMenuChangeSFX();
                SceneManager.LoadScene("SampleScene");
            });
            
            // Hide title simultaneously
            titleAnimation.Hide();
        });
        settingsButton.onClick.AddListener(() =>
        {
            SettingsUI.Instance.Show();
        });
        quitButton.onClick.AddListener(() =>
        {
            stagger.CloseMenu(() =>
            {
                Debug.Log("quit clicked");
                Application.Quit();
            });
            
            titleAnimation.Hide();
        });
    }

    private void Start()
    {
        stagger.OpenMenu();

        //ServiceLocator.Instance.AudioManager.PlayMenuMusic();
    }

    public void Hide()
    {
        stagger.CloseMenu();
    }
}
