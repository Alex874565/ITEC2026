using System;
using UnityEngine;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance {get; private set;}
    public event EventHandler OnEscapeAction;
    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        Instance = this;
        
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();

        playerInputActions.Player.Escape.performed += Escape_performed;
    }

    private void Escape_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnEscapeAction?.Invoke(this, EventArgs.Empty);
    }
}
