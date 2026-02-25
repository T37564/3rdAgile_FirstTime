using UnityEngine;
using Fusion;

namespace Network.Player
{
    [RequireComponent(typeof(PlayerInputHandler))]
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] private float moveSpeed = 1.0f;

        [Networked] private NetworkBool isHolding { get; set; }

        private Rigidbody rb;

        /// <summary>
        /// ネットワーク上でオブジェクトが確定したときに呼び出されるコールバック関数
        /// UnityのStart()のようなものだが、ネットワーク上でオブジェクトが確定したときに呼び出されるため、
        /// ネットワークオブジェクトの初期化に適している
        /// </summary>
        public override void Spawned()
        {
            rb = GetComponent<Rigidbody>();
            if (Object.HasInputAuthority)
            {
                var inputGetter = FindAnyObjectByType<PlayerInputGetter>();
                inputGetter.RegisterLocalInput(GetComponent<PlayerInputHandler>());
            }
        }

        /// <summary>
        /// ネットワーク上でオブジェクトが確定した後、毎Tick呼び出されるコールバック関数
        /// </summary>
        public override void FixedUpdateNetwork()
        {
            if(!Object.HasStateAuthority) return;

            if (GetInput<PlayerInputData>(out var input))
            {
                Vector3 move = new Vector3(input.move.x, 0.0f, input.move.y);
                transform.position += move * moveSpeed * Runner.DeltaTime;

                if (input.tryPick)
                {

                }
            }
        }
    }
}
