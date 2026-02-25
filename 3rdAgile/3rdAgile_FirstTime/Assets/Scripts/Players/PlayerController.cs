using UnityEngine;
using Fusion;

namespace Network.Player
{
    [RequireComponent(typeof(PlayerInputHandler))]
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] private float moveSpeed = 1.0f;
        //[SerializeField] private float jumpForce = 50.0f;

        private Rigidbody rb;

        public override void Spawned()
        {
            rb = GetComponent<Rigidbody>();
            if (Object.HasInputAuthority)
            {
                var inputGetter = FindAnyObjectByType<PlayerInputGetter>();
                inputGetter.RegisterLocalInput(GetComponent<PlayerInputHandler>());
            }
        }

        public override void FixedUpdateNetwork()
        {
            if(!Object.HasStateAuthority) return;

            if (GetInput<PlayerInputData>(out var input))
            {
                Vector3 move = new Vector3(input.move.x, 0.0f, input.move.y);
                //rb.linearVelocity = move * moveSpeed;
                transform.position += move * moveSpeed * Runner.DeltaTime;
            }
        }
    }
}
