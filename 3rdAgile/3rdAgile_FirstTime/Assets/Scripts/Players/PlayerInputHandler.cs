//==========================================================================
// プレイヤーの入力をローカルで受け取り、構造体にまとめてホストに送るクラス
// 担当者：鈴木
//==========================================================================

using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Network.Player
{
    /// <summary>
    /// ローカルのプレイヤー入力情報を受け取るクラス
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputHandler : MonoBehaviour
    {
        //プレイヤーの入力情報を保存するためのコンポーネント
        private PlayerInput playerInput;

        // プレイヤーの移動入力を保存するための変数
        private Vector2 moveInput = Vector2.zero;
        private bool hasItem = false;
        private bool interactPressed = false;
        private bool interactHeld = false;

        // 拾う入力
        private bool interactHoldCompleted = false;
        private bool interactTriggered = false;

        // 離す入力
        private bool dropHoldCompleted = false;
        private bool dropTriggered = false;

        // プレイヤーがインタラクト可能なオブジェクトを保存するリスト
        private List<IInteractable> interactables = new List<IInteractable>();

        public IInteractable interactObject = null;

        public Transform Transform => transform;

        public string tagName = "";

        // アイテムを拾う等の長押し入力が成立したかどうかを保存するための変数
        private bool picked = false;

        /// <summary>
        /// 初期化
        /// </summary>
        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            playerInput.DeactivateInput();

            CountDownUI.OnCountDownFinished += HandleCountDownFinished;
        }

        private void HandleCountDownFinished()
        {
            playerInput.ActivateInput();
        }

        private void OnDestroy()
        {
            CountDownUI.OnCountDownFinished -= HandleCountDownFinished;
        }

        #region イベント登録と解除
        /// <summary>
        /// イベント登録
        /// </summary>
        private void OnEnable()
        {
            if (playerInput == null) return;

            playerInput.actions["Move"].performed += OnMove;
            playerInput.actions["Move"].canceled += OnMove;

            playerInput.actions["ItemPicked"].performed += OnInteractPerformed;
            playerInput.actions["ItemPicked"].canceled += OnInteractCanceled;

            playerInput.actions["ItemPicked"].started += OnInteractStarted;
            playerInput.actions["ItemDroped"].performed += OnItemDropedPerformed;
            playerInput.actions["ItemDroped"].canceled += OnItemDropedCanceled;
        }

        /// <summary>
        /// イベント登録解除
        /// </summary>
        private void OnDisable()
        {
            if (playerInput == null) return;

            playerInput.actions["Move"].performed -= OnMove;
            playerInput.actions["Move"].canceled -= OnMove;

            playerInput.actions["ItemPicked"].started -= OnInteractStarted;
            playerInput.actions["ItemPicked"].performed -= OnInteractPerformed;
            playerInput.actions["ItemPicked"].canceled -= OnInteractCanceled;

            //playerInput.actions["ItemDroped"].performed -= OnItemDropedPerformed;
            //playerInput.actions["ItemDroped"].canceled -= OnItemDropedCanceled;
        }
        #endregion

        #region プレイヤーの移動入力
        /// <summary>
        /// InputSystem経由で入ってきた移動入力情報を保存
        /// </summary>
        public void OnMove(InputAction.CallbackContext context)
        {

            moveInput = context.ReadValue<Vector2>();
        }
        #endregion

        #region アイテムを拾う等Aボタン長押し処理
        public void OnInteractStarted(InputAction.CallbackContext context)
        {
            interactPressed = true;
            interactHeld = false;
        }

        /// <summary>
        /// ボタンの長押し入力が成立したタイミングで発火
        /// </summary>
        public void OnInteractPerformed(InputAction.CallbackContext context)
        {
            // 長押し成立フラグを立てる
            interactHeld = true;
        }

        /// <summary>
        /// 長押し入力がキャンセルされたら発火
        /// </summary>
        public void OnInteractCanceled(InputAction.CallbackContext context)
        {
            interactPressed = false;
            interactHeld = false;
        }
        #endregion

        #region アイテムを離す
        /// <summary>
        /// 長押しが成立したら発火
        /// </summary>
        public void OnItemDropedPerformed(InputAction.CallbackContext context)
        {
            dropHoldCompleted = true;
            Debug.Log("成立！");
        }

        /// <summary>
        /// 長押し入力がキャンセルされたら発火
        /// </summary>
        public void OnItemDropedCanceled(InputAction.CallbackContext context)
        {
            if (!dropHoldCompleted) return;
            dropTriggered = true;
            dropHoldCompleted = false;
        }
        #endregion

        /// <summary>
        /// 保存した情報を入力構造体に渡す
        /// </summary>
        public PlayerInputData GetInput()
        {
            return new PlayerInputData
            {
                move = moveInput,
                interactPressed = interactPressed,
                interactHeld = interactHeld
            };
        }

        /// <summary>
        /// プレイヤーの一定距離内入ったオブジェクトを保存するためのメソッド
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            var interactable = other.GetComponent<IInteractable>();
            if (interactable != null)
                interactables.Add(interactable);


            // プレイヤーの周囲にあるインタラクト可能なオブジェクトの中で一番近いものを探す
            float minDistance = float.MaxValue;

            foreach (var interactableObj in interactables)
            {
                // 距離の計算
                float distance = (interactableObj.Transform.position - Transform.position).sqrMagnitude;
                // 最も近いオブジェクトを保存
                if (distance < minDistance)
                {
                    minDistance = distance;
                    interactObject = interactableObj;
                    tagName = interactObject.Transform.tag;
                }
            }
        }

        /// <summary>
        /// 登録していたオブジェクトからプレイヤーが一定距離外に出たときにリストから削除するためのメソッド
        /// </summary>
        private void OnTriggerExit(Collider other)
        {
            var interactable = other.GetComponent<IInteractable>();
            if (interactable != null)
                interactables.Remove(interactable);
        }
    }
}