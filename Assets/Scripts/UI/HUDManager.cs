using System;
using TMPro;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    public TextMeshProUGUI TurnText;
    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI Points;
    public TextMeshProUGUI Investment;
    public TextMeshProUGUI Debt;

    public PlayerBehaviour Player;
    
    private void Start()
    {
        GameManager.Instance.ActivePlayer.OnValueChanged += OnActivePlayerChanged;
        gameObject.SetActive(false);
    }
    
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ActivePlayer.OnValueChanged -= OnActivePlayerChanged;
    }
    
    private void OnActivePlayerChanged(int oldValue, int newValue)
    {
        Debug.Log($"Active Player changed to: {newValue}, Player Number: {Player.PlayerNumber}");
        if (Player.PlayerNumber == newValue)
        {
            TurnText.text = "Your Turn";
        }
        else
        {
            TurnText.text = $"Others' Turn";
        }
    }

    private void OnEnable()
    {
        if (Player != null)
        {
            OnActivePlayerChanged(0, GameManager.Instance.ActivePlayer.Value);
        }
    }
}