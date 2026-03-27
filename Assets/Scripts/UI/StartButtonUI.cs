using Unity.Services.Multiplayer;
using UnityEngine.UI;
using Unity.Multiplayer.Widgets;
using UnityEngine;

public class StartButtonUI : MonoBehaviour
{
    public ISession Session { get; set; }
    
    Button m_Button;
        
    void Start()
    {
        m_Button = GetComponent<Button>();
        m_Button.onClick.AddListener(StartGame);
        SetButtonActive();
    }

    public void OnSessionLeft()
    {
        SetButtonActive();
    }

    public void OnSessionJoined()
    {
        SetButtonActive();
    }

        
    void SetButtonActive()
    {
        m_Button.interactable = Session?.Players?.Count == 2;
    }
        
    private void StartGame()
    {
        if(Session?.Players?.Count == 2)
        {
            GameManager.Instance.StartGame();
        }
    }
}