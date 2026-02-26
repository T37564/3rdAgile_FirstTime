using UnityEngine;
using Fusion;
using System;

namespace Network.Player
{
    public interface IDamage
    {
        void TakeDamage();
    }

    [RequireComponent(typeof(PlayerInputHandler))]
    public class PlayerController : NetworkBehaviour, IDamage
    {
        // プレイヤーがアイテムを拾ったときのイベント
        public event Action OnPickUpItem;

        // プレイヤーの生死に関するイベント
        public event Action OnPlayerDied;

        [SerializeField] private float moveSpeed = 1.0f;

        [Networked] private NetworkBool isHolding { get; set; }

        [Networked] private NetworkBool isAlive { get; set; }


        /// <summary>
        /// ネットワーク上でオブジェクトが確定したときに呼び出されるコールバック関数
        /// UnityのStart()のようなものだが、ネットワーク上でオブジェクトが確定したときに呼び出されるため、
        /// ネットワークオブジェクトの初期化に適している
        /// </summary>
        public override void Spawned()
        {
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

            if (GetInput<PlayerInputData>(out var input))
            {
                if (!Object.HasStateAuthority) return;

                Vector3 move = new Vector3(input.move.x, 0.0f, input.move.y);
                transform.position += move * moveSpeed * Runner.DeltaTime;

                if (input.tryPick)
                {
                    isHolding = input.tryPick;
                }
            }

            if (isHolding)
            {
                // ここでは、オブジェクトを持っている状態の処理を記述することができます。
            }
        }

        public override void Render()
        {
            if (isHolding)
                OnPickUpItem?.Invoke();

            if(!isAlive)
                OnPlayerDied?.Invoke();
        }

        public void TakeDamage()
        {
        }
    }
}
