using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine;

public class StartButtonUI : NetworkBehaviour
{
    private Button m_Button;

    public override void OnNetworkSpawn()
    {
        m_Button = GetComponent<Button>();
        m_Button.onClick.AddListener(StartGame);

        GameManager.Instance.PlayerCount.OnValueChanged += SetButtonActive;

        SetButtonActive(0, GameManager.Instance.PlayerCount.Value);
    }

    public override void OnNetworkDespawn()
    {
        if (m_Button != null)
            m_Button.onClick.RemoveListener(StartGame);

        GameManager.Instance.PlayerCount.OnValueChanged -= SetButtonActive;
    }

    private void SetButtonActive(int oldValue, int newValue)
    {
        Debug.Log($"Connected Clients: {newValue}");
        m_Button.interactable = (newValue == 2);
    }

    private void StartGame()
    {
        if (!IsServer)
        {
            return;
        }

        if (GameManager.Instance.PlayerCount.Value == 2)
        {
            GameManager.Instance.StartGame();
        }
    }
}