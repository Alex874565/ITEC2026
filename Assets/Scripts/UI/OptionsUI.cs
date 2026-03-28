using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsUI : MonoBehaviour
{
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private MenuStaggerAnimation stagger;

    private void Awake()
    {
        closeButton.onClick.AddListener(() =>
        {
            Hide();
        });
        settingsButton.onClick.AddListener( () =>
        {
            SettingsUI.Instance.Show();
        });
        mainMenuButton.onClick.AddListener( () =>
        {
            stagger.CloseMenu(() =>
            {
            SceneManager.LoadScene("MainMenuScene");
            });
        });
    }

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        stagger.OpenMenu();
    }

    public void Hide()
    {
        stagger.CloseMenu(() =>
        {
            gameObject.SetActive(false);
        });
    }
}
