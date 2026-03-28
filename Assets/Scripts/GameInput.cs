using System;
using UnityEngine;

public class GameInput : MonoBehaviour
{
    public event EventHandler OnEscapeAction;
    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();

        playerInputActions.Player.Pause.performed += Escape_performed;
    }

    private void Escape_performed(UnityEngine.InputAction.CallbackContext obj)
    {
        OnEscapeAction?.Invoke(this, EventArgs.Empty);
    }
}
