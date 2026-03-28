using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsUI : MonoBehaviour
{
    public static OptionsUI Instance { get; private set; }

    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mainMenuButton;
    //[SerializeField] private Button closeButton;
    [SerializeField] private MenuStaggerAnimation stagger;

    private void Awake()
    {
        Debug.Log("OptionsUI Awake on: " + gameObject.name);

        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate OptionsUI destroyed: " + gameObject.name);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        // closeButton.onClick.AddListener(() =>
        // {
        //     Hide();
        // });
        settingsButton.onClick.AddListener( () =>
        {
            SettingsUI.Instance.Show();
        });
        mainMenuButton.onClick.AddListener( () =>
        {
            Debug.Log("main menu");
            stagger.CloseMenu(() =>
            {
                SceneManager.LoadScene("MainMenuScene");
            });
        });
    }

    private void Start()
    {
        GameManager.Instance.OnOptionsActive += GameManager_OnOptionsActive;
        GameManager.Instance.OnOptionsInactive += GameManager_OnOptionsInactive;
        
        gameObject.SetActive(false);
    }

    private void GameManager_OnOptionsInactive(object sender, EventArgs e)
    {
        Hide();
    }

    private void GameManager_OnOptionsActive(object sender, EventArgs e)
    {
        Show();
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
