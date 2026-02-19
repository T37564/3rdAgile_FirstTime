using UnityEngine;
using Fusion;

namespace Network.Player
{
    [RequireComponent(typeof(PlayerInputHandler))]
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] private PlayerInputHandler inputHandler;

        [SerializeField] private float moveSpeed = 1.0f;
        [SerializeField] private float jumpForce = 50.0f;

        private Rigidbody rb;

        public override void Spawned()
        {
            rb = GetComponent<Rigidbody>();
            if (Object.HasInputAuthority)
                inputHandler = GetComponent<PlayerInputHandler>();
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out PlayerInputData input))
            {
                Vector3 move = new Vector3(input.move.x, 0.0f, input.move.y);
                rb.MovePosition(rb.position + move * moveSpeed * Runner.DeltaTime);
            }
        }
    }
}
