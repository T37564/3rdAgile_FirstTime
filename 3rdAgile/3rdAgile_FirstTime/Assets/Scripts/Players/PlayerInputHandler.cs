//======================================================================
// 担当者：鈴木
//======================================================================

using UnityEngine;
using UnityEngine.InputSystem;

namespace Network.Player
{
    /// <summary>
    /// ローカルのプレイヤー入力情報を受け取るクラス
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputHandler : MonoBehaviour
    {
        private PlayerInput playerInput;

        private Vector2 move;
        private bool picked = false;

        private bool holdCompleted = false;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
        }

        /// <summary>
        /// イベント登録
        /// </summary>
        private void OnEnable()
        {
            if (playerInput == null) return;

            playerInput.actions["Move"].performed += OnMove;
            playerInput.actions["Move"].canceled += OnMove;

            playerInput.actions["ItemPicked"].performed += OnItemPickedPerformed;
            playerInput.actions["ItemPicked"].canceled += OnItemPickedCanceled;
        }

        /// <summary>
        /// イベント登録解除
        /// </summary>
        private void OnDisable()
        {
            if (playerInput == null) return;

            playerInput.onActionTriggered -= OnMove;
        }

        /// <summary>
        /// InputSystem経由で入ってきた移動入力情報を保存
        /// </summary>
        public void OnMove(InputAction.CallbackContext context)
        {
            if (context.action.name != "Move") return;

            Debug.Log("Move input received: " + context.ReadValue<Vector2>());

            move = context.ReadValue<Vector2>();
        }

        #region アイテムを拾う等Aボタン長押し処理
        /// <summary>
        /// ボタンの長押し入力が成立したタイミングで発火
        /// </summary>
        public void OnItemPickedPerformed(InputAction.CallbackContext context)
        {
            // 長押し成立フラグを立てる
            holdCompleted = true;
        }

        /// <summary>
        /// 長押し入力がキャンセルされたら発火
        /// </summary>
        public void OnItemPickedCanceled(InputAction.CallbackContext context)
        {
            // 長押し入力が成立していなかったらメソッドから抜ける
            if (!holdCompleted) return;

            //成立したとホストに通知するためのbool値をtrueで保存
            picked = true;

            // ボタンが離されたので再度長押し判定をとれるようにfalseに
            holdCompleted = false;
        }
        #endregion

        #region アイテムを離す
        /// <summary>
        /// 長押しが成立したら発火
        /// </summary>
        public void OnItemDropedPerformed(InputAction.CallbackContext context)
        {
            holdCompleted = true;
        }

        /// <summary>
        /// 長押し入力がキャンセルされたら発火
        /// </summary>
        public void OnItemDropedCanceled(InputAction.CallbackContext context)
        {
            if (!holdCompleted) return;
            picked = false;
            holdCompleted = false;
        }
        #endregion

        /// <summary>
        /// 保存した情報を入力構造体に渡す
        /// </summary>
        public PlayerInputData GetInput()
        {
            PlayerInputData data = new PlayerInputData()
            {
                move = move,
                picked = picked
            };
            Debug.Log("InputData created: Move = " + data.move + ", Picked = " + data.picked);
            return data;
        }

        
    }
}
