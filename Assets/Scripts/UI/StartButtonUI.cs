using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine;

public class StartButtonUI : NetworkBehaviour
{
    private Button m_Button;

    public NetworkVariable<int> PlayerCount =
        new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        m_Button = GetComponent<Button>();
        m_Button.onClick.AddListener(StartGame);

        PlayerCount.OnValueChanged += SetButtonActive;

        if (IsServer)
        {
            UpdatePlayerCount();
            NetworkManager.OnClientConnectedCallback += OnClientChanged;
            NetworkManager.OnClientDisconnectCallback += OnClientChanged;
        }

        SetButtonActive(0, PlayerCount.Value);
    }

    public override void OnNetworkDespawn()
    {
        if (m_Button != null)
            m_Button.onClick.RemoveListener(StartGame);

        PlayerCount.OnValueChanged -= SetButtonActive;

        if (IsServer && NetworkManager != null)
        {
            NetworkManager.OnClientConnectedCallback -= OnClientChanged;
            NetworkManager.OnClientDisconnectCallback -= OnClientChanged;
        }
    }

    private void OnClientChanged(ulong clientId)
    {
        UpdatePlayerCount();
    }

    private void UpdatePlayerCount()
    {
        PlayerCount.Value = NetworkManager.Singleton.ConnectedClients.Count;
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

        if (PlayerCount.Value == 2)
        {
            GameManager.Instance.StartGame();
        }
    }
}