using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInput playerInput;

    private PlayerInputData currentInput;
    public PlayerInputData CurrentInput => currentInput;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        if (playerInput == null) return;

        playerInput.onActionTriggered += OnMove;
        playerInput.onActionTriggered += OnJump;
    }

    private void OnDisable()
    {
        if (playerInput == null) return;

        playerInput.onActionTriggered -= OnMove;
        playerInput.onActionTriggered -= OnJump;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.action.name != "Move") return;

        currentInput.move = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.action.name != "Jump") return;

        if (context.started)
            currentInput.jump = true;
    }

    public void ClearOneShotInput()
    {
        currentInput.jump = false;
    }
}
