using UnityEngine;
using Fusion;
using System;

namespace Network.Player
{
    public interface IDamage
    {
        void TakeDamage();
    }

    /// <summary>
    /// プレイヤーの移動、アイテムの持ち運び、ダメージ処理、インタラクト処理を担当するクラス
    /// </summary>
    [RequireComponent(typeof(PlayerInputHandler))]
    public class PlayerController : NetworkBehaviour, IDamage, IInteractable
    {
        #region イベント
        // プレイヤーがアイテムを拾ったときのイベント
        public event Action OnPickUpItem;
        // プレイヤーの生死に関するイベント
        public event Action OnPlayerDied;
        public event Action OnPlayerRevived;
        #endregion

        [Header("-- Player Settings --")]
        [Header("プレイヤーの移動速度")]
        [SerializeField] private float moveSpeed = 1.0f;

        [Header("インタラクト判定半径")]
        [SerializeField] private float interactRadius = 2.0f;

        [Header("インタラクト対象レイヤー")]
        [SerializeField] private LayerMask interactLayerMask;

        [Header("アイテムを持っているときの最大距離")]
        [SerializeField] private float maxCarryDistance = 2.0f;

        #region ネットワーク共有変数
        [Networked] public NetworkBool IsHoldingItem { get; set; }

        [Networked] private NetworkBool IsAlive { get; set; }
        #endregion

        // ローカルでプレイヤーの入力を受け取るためのコンポーネント
        private PlayerInputHandler inputHandler;

        // アニメーション制御用のコンポーネント
        private Animator animator;

        #region 前のフレームの状態を保存するための変数
        private bool prevAlive;

        private bool prevHoldingItem;
        #endregion

        // プレイヤーが現在持っているアイテム（持っていないときはnull）
        private ItemInteractable holdingItem;

        // プレイヤーの現在の回転角度
        private Quaternion currentAngle = Quaternion.identity;

        // IInteractableインターフェースの実装
        public Transform Transform => transform;


        /// <summary>
        /// ネットワーク上でオブジェクトが確定したときに呼び出されるコールバック関数
        /// UnityのStart()のようなものだが、ネットワーク上でオブジェクトが確定したときに呼び出されるため、
        /// ネットワークオブジェクトの初期化に適している
        /// </summary>
        public override void Spawned()
        {
            // ローカルプレイヤーだけがPlayerInputHandlerを持つことになる
            inputHandler = GetComponent<PlayerInputHandler>();

            if (Object.HasInputAuthority)
            {
                // ローカルプレイヤーのPlayerInputHandlerをPlayerInputGetterに登録
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
            // 入力を取得
            if (!GetInput<PlayerInputData>(out var input))
                return;

            if (Runner.IsForward)
            {
                UpdateAnimation(input.move);
            }

            if (!Object.HasStateAuthority) return;

            // 死亡中は移動やインタラクトを受け付けない
            if (IsAlive)
            {
                // 移動処理
                Move(input.move);

                // インタラクト処理
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

            // 正規化
            if (move.sqrMagnitude > 1f)
            {
                move.Normalize();
            }

            // アイテムの所持状態に応じて移動速度を変更
            float speed = IsHoldingItem ? maxCarryDistance : moveSpeed;

            // 移動量を計算
            Vector3 nextPosition = transform.position + move * speed * Runner.DeltaTime;

            // アイテムを持っているときは、アイテムとの距離がmaxCarryDistanceを超えないように制限
            if (IsHoldingItem && holdingItem != null)
            {
                // アイテムの位置を取得
                Vector3 itemPosition = holdingItem.Transform.position;

                // プレイヤーとアイテムの距離を計算
                Vector3 diff = nextPosition - itemPosition;
                diff.y = 0f; // 水平方向のみに制限

                // 距離がmaxCarryDistanceを超える場合は、距離をmaxCarryDistanceに制限
                if (diff.magnitude > maxCarryDistance)
                {
                    // 距離をmaxCarryDistanceに制限した位置を計算
                    diff = diff.normalized * maxCarryDistance;
                    // プレイヤーの次の位置を、アイテムからmaxCarryDistanceの位置に修正
                    nextPosition = itemPosition + diff;
                    // Y座標は変えない
                    nextPosition.y = transform.position.y;
                }
                // プレイヤーの回転をアイテムの方向に固定
                Vector3 direction = (holdingItem.Transform.position - transform.position);
                direction.y = 0f; // 水平方向のみに回転させるためにY成分を0にする

                // 入力があるときだけ回転
                if (direction.sqrMagnitude > 0.01f)
                {
                    // アイテムの方向に回転
                    currentAngle = Quaternion.LookRotation(direction);

                    transform.rotation = currentAngle;
                }
            }
            // アイテムを持っていないときは移動方向に回転
            else
            {
                float angle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg;

                currentAngle = Quaternion.Euler(0.0f, angle, 0.0f);

                transform.rotation = currentAngle;
            }

            // プレイヤーの位置を更新
            transform.position = nextPosition;

            //// 入力があるときだけ回転
            //if (move.sqrMagnitude > 0.01f)
            //{
            //    // アイテムを持っているときはアイテムの方向に回転
            //    if (IsHoldingItem)
            //    {
            //        // プレイヤーの回転をアイテムの方向に固定
            //        Vector3 direction = (holdingItem.Transform.position - transform.position);
            //        direction.y = 0f; // 水平方向のみに回転させるためにY成分を0にする

            //        if (direction.sqrMagnitude > 0.01f)
            //        {
            //            currentAngle = Quaternion.LookRotation(direction);

            //            transform.rotation = currentAngle;
            //        }
            //    }
            //    // アイテムを持っていないときは移動方向に回転
            //    else
            //    {
            //        float angle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg;

            //        currentAngle = Quaternion.Euler(0.0f, angle, 0.0f);

            //        transform.rotation = currentAngle;
            //    }
            //}


            // アニメーション制御
            UpdateAnimation(moveInput);
        }

        /// <summary>
        /// インタラクト処理
        /// </summary>
        private void TryInteract()
        {
            // プレイヤーの周囲にあるインタラクト可能なオブジェクトを探す
            IInteractable target = FindNearestInteractable();

            // インタラクト可能なオブジェクトが見つかった場合は、インタラクト処理を実行
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
