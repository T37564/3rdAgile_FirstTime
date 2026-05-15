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
    public class PlayerController : NetworkBehaviour, IDamage, IInteractable
    {
        // プレイヤーがアイテムを拾ったときのイベント
        public event Action OnPickUpItem;

        // プレイヤーの生死に関するイベント
        public event Action OnPlayerDied;

        public event Action OnPlayerRevived;

        [Header("-- Player Settings --")]
        [Header("プレイヤーの移動速度")]
        [SerializeField] private float moveSpeed = 1.0f;

        [Header("インタラクト判定半径")]
        [SerializeField] private float interactRadius = 2.0f;

        [Header("インタラクト対象レイヤー")]
        [SerializeField] private LayerMask interactLayerMask;

        [Networked] private NetworkBool IsHoldingItem { get; set; }

        [Networked] private NetworkBool IsAlive { get; set; }

        private PlayerInputHandler inputHandler;

        private bool prevAlive;

        private bool prevHoldingItem;

        private Animator animator;

        public Transform Transform => transform;


        /// <summary>
        /// ネットワーク上でオブジェクトが確定したときに呼び出されるコールバック関数
        /// UnityのStart()のようなものだが、ネットワーク上でオブジェクトが確定したときに呼び出されるため、
        /// ネットワークオブジェクトの初期化に適している
        /// </summary>
        public override void Spawned()
        {
            inputHandler = GetComponent<PlayerInputHandler>();

            if (Object.HasInputAuthority)
            {
                PlayerInputGetter inputGetter = FindAnyObjectByType<PlayerInputGetter>();
                if (inputGetter != null)
                {
                    inputGetter.RegisterLocalInput(inputHandler);
                }
            }

            if (Object.HasStateAuthority)
            {
                IsAlive = true;
                IsHoldingItem = false;
            }

            prevAlive = IsAlive;
            prevHoldingItem = IsHoldingItem;

            animator = GetComponent<Animator>();
        }

        /// <summary>
        /// ネットワーク上でオブジェクトが確定した後、毎Tick呼び出されるコールバック関数
        /// </summary>
        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority) return;
            if (!GetInput<PlayerInputData>(out var input)) return;

            if (IsAlive)
            {
                Move(input.move);
            }

            if (input.tryInteract)
            {
                TryInteract();
            }
        }

        private void Move(Vector2 moveInput)
        {
            Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);

            if (move.sqrMagnitude > 1f)
            {
                move.Normalize();
            }

            transform.position += move * moveSpeed * Runner.DeltaTime;

            // =========================
            // アニメーション制御
            // =========================
            bool isMoving = move.sqrMagnitude > 0.01f;

            bool isRunning = moveInput.magnitude > 0.8f;

            animator.SetBool("Walk", isMoving && !isRunning);

            animator.SetBool("Run", isRunning);
        }

        private void TryInteract()
        {
            IInteractable target = FindNearestInteractable();

            if (target == null) return;
            if (!target.CanInteract(this)) return;

            target.Interact(this);
        }

        private IInteractable FindNearestInteractable()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, interactRadius, interactLayerMask);

            IInteractable nearest = null;
            float minSqrDistance = float.MaxValue;

            foreach (Collider hit in hits)
            {
                if (hit.transform == transform) continue;

                IInteractable interactable = hit.GetComponent<IInteractable>();
                if (interactable == null) continue;

                float sqrDistance = (interactable.Transform.position - transform.position).sqrMagnitude;
                if (sqrDistance < minSqrDistance)
                {
                    minSqrDistance = sqrDistance;
                    nearest = interactable;
                }
            }

            return nearest;
        }

        /// <summary>
        /// ローカル通知用の関数
        /// </summary>
        public override void Render()
        {
            if (prevAlive && !IsAlive)
            {
                OnPlayerDied?.Invoke();
            }

            if (!prevAlive && IsAlive)
            {
                OnPlayerRevived?.Invoke();
            }

            if (!prevHoldingItem && IsHoldingItem)
            {
                OnPickUpItem?.Invoke();
            }

            prevAlive = IsAlive;
            prevHoldingItem = IsHoldingItem;
        }

        /// <summary>
        /// モンスターからダメージを受けたときに実行される関数
        /// </summary>
        public void TakeDamage()
        {
            if (!Object.HasStateAuthority) return;
            if (!IsAlive) return;

            IsAlive = false;
        }

        public void Revive()
        {
            if (!Object.HasStateAuthority) return;
            if (IsAlive) return;

            IsAlive = true;
        }


        // =========================
        // IInteractable 実装
        // 「死亡中プレイヤーは蘇生対象になる」
        // =========================

        public bool CanInteract(PlayerController player)
        {
            if (player == null) return false;

            // 自分自身にはインタラクトしない
            if (player == this) return false;

            // 死亡中のプレイヤーだけ蘇生対象
            return !IsAlive;
        }

        public void Interact(PlayerController player)
        {
            if (!Object.HasStateAuthority) return;
            if (IsAlive) return;

            Revive();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}
