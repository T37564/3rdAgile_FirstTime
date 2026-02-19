using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;

namespace Network.Player
{
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputHandler : MonoBehaviour
    {
        private PlayerInput playerInput;

        private Vector2 move;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
        }

        private void OnEnable()
        {
            if (playerInput == null) return;

            playerInput.actions["Move"].performed += OnMove;
            playerInput.actions["Move"].canceled += OnMove;
        }

        private void OnDisable()
        {
            if (playerInput == null) return;

            playerInput.onActionTriggered -= OnMove;
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            if (context.action.name != "Move") return;

            move = context.ReadValue<Vector2>();
        }

        public PlayerInputData GetInput()
        {
            PlayerInputData data = new PlayerInputData()
            {
                move = move,
            };
            return data;
        }

        //public void OnJump(InputAction.CallbackContext context)
        //{
        //    if (context.action.name != "Jump") return;

        //    if (context.started)
        //        currentInput.jump = true;
        //}

        //public void ClearOneShotInput()
        //{
        //    currentInput.jump = false;
        //}
    }
}
