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

        [Networked] public NetworkBool IsHoldingItem { get; set; }

        [Networked] private NetworkBool IsAlive { get; set; }

        private PlayerInputHandler inputHandler;

        private Animator animator;

        private bool prevAlive;

        private bool prevHoldingItem;

        private ItemInteractable holdingItem;

        private Quaternion currentAngle = Quaternion.identity;
        private Quaternion previousAngle = Quaternion.identity;

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
                interactLayerMask = LayerMask.GetMask("Item");
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
            if (!GetInput<PlayerInputData>(out var input))
                return;

            if (Runner.IsForward)
            {
                UpdateAnimation(input.move);
            }

            if (!Object.HasStateAuthority) return;

            if (IsAlive)
            {
                Move(input.move);

                if (input.tryInteract)
                {
                    TryInteract();
                }
            }
        }

        /// <summary>
        /// アニメーション再生処理
        /// </summary>
        private void UpdateAnimation(Vector2 moveInput)
        {
            bool isMoving = moveInput.sqrMagnitude > 0.01f;

            bool isRunning = moveInput.magnitude > 0.8f;

            animator.SetBool("Walk", isMoving && !isRunning);

            animator.SetBool("Run", isRunning);
        }

        /// <summary>
        /// 受け取ったVector2型を使って移動
        /// </summary>
        private void Move(Vector2 moveInput)
        {
            //Vector3に変換
            Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);

            if (move.sqrMagnitude > 1f)
            {
                move.Normalize();
            }

            transform.position += move * moveSpeed * Runner.DeltaTime;

            // 入力があるときだけ回転
            if (move.sqrMagnitude > 0.01f)
            {
                if (!IsHoldingItem)
                {
                    float angle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg;

                    currentAngle = Quaternion.Euler(0.0f, angle, 0.0f);

                    transform.rotation = currentAngle;
                }
                else
                {
                    // プレイヤーの回転をアイテムの方向に固定
                    Vector3 distance = (transform.position - holdingItem.Transform.position);
                    distance = distance.normalized;
                    currentAngle = Quaternion.Euler(distance);

                    transform.rotation = currentAngle;
                }
            }


            // アニメーション制御
            UpdateAnimation(moveInput);
        }

        private void TryInteract()
        {
            Debug.Log("Trying to interact...");
            IInteractable target = FindNearestInteractable();
            Debug.Log($"Nearest interactable: {target}");

            if (target == null) return;
            Debug.Log($"Found interactable: {target}");
            if (!target.CanInteract(this)) return;
            Debug.Log($"Can interact with: {target}");

            target.Interact(this);
            Debug.Log($"Interacted with: {target}");
        }

        private IInteractable FindNearestInteractable()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, interactRadius, interactLayerMask);

            IInteractable nearest = null;
            float minSqrDistance = float.MaxValue;

            foreach (Collider hit in hits)
            {
                if (hit.transform == transform) continue;

                Debug.Log($"Hit: {hit.gameObject.name}");

                IInteractable interactable = hit.GetComponentInParent<IInteractable>();
                if (interactable == null) continue;

                if (interactable.Transform == transform) continue;

                float sqrDistance =
                    (interactable.Transform.position - transform.position).sqrMagnitude;

                if (sqrDistance < minSqrDistance)
                {
                    minSqrDistance = sqrDistance;
                    nearest = interactable;
                }
            }

            Debug.Log($"{nearest}");
            return nearest;
        }

        public void SetHoldingItem(ItemInteractable item)
        {
            if (!Object.HasStateAuthority) return;
            holdingItem = item;
            IsHoldingItem = true;
        }
        public void ClearHoldingItem(ItemInteractable item)
        {
            if (!Object.HasStateAuthority) return;
            if (holdingItem != item) return;
            holdingItem = null;
            IsHoldingItem = false;
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

            if (holdingItem != null)
            {
                holdingItem.Release(this);
                holdingItem = null;
            }

            IsHoldingItem = false;
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
