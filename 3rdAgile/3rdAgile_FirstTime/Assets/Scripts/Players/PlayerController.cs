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

        [SerializeField] private float moveSpeed = 1.0f;

        [Networked] private NetworkBool isHolding { get; set; }

        [Networked] private NetworkBool isAlive { get; set; }

        private PlayerInputHandler inputHandler;
        private IInteractable interactable;
        public Transform Transform => transform;


        /// <summary>
        /// ネットワーク上でオブジェクトが確定したときに呼び出されるコールバック関数
        /// UnityのStart()のようなものだが、ネットワーク上でオブジェクトが確定したときに呼び出されるため、
        /// ネットワークオブジェクトの初期化に適している
        /// </summary>
        public override void Spawned()
        {
            isAlive = true;
            if (Object.HasInputAuthority)
            {
                inputHandler = GetComponent<PlayerInputHandler>();
                var inputGetter = FindAnyObjectByType<PlayerInputGetter>();
                inputGetter.RegisterLocalInput(inputHandler);
            }
        }

        /// <summary>
        /// ネットワーク上でオブジェクトが確定した後、毎Tick呼び出されるコールバック関数
        /// </summary>
        public override void FixedUpdateNetwork()
        {
            Debug.Log("FixedUpdateNetwork");
            if (!isAlive) return;

            Debug.Log("生きてるよ");
            if (GetInput<PlayerInputData>(out var input))
            {
                if (!Object.HasStateAuthority) return;

                Vector3 move = new Vector3(input.move.x, 0.0f, input.move.y);
                transform.position += move * moveSpeed * Runner.DeltaTime;

                if (input.tryPick)
                {
                    //isHolding = input.tryPick;

                    // tryPickは長押しが成立したときにtrueになるため以下でアイテムを拾う処理、プレイヤーを復活させる処理に分岐
                    // PlayerInputHandlerで保存した一番近い位置にあるIInteractableを取得する
                    interactable = inputHandler.interactObject;

                    // タグ名を取得して取得したIInteractableがアイテムなのかプレイヤーなのかを判別し処理を分岐
                    string interactableObjectTagName = inputHandler.tagName;
                    switch (interactableObjectTagName)
                    {
                        case "Item":
                            isHolding = true;
                            // アイテムのIInteractableを継承しているクラスにinteractableをキャストして、Interact()を呼び出す
                            break;
                        case "Player":
                            // プレイヤーのIInteractableを継承しているクラスにinteractableをキャストしてisAliveをtrueにする
                            PlayerController playerController = interactable as PlayerController;
                            playerController.isAlive = true;
                            break;
                        default:
                            break;
                    }
                }
            }

            //if (isHolding)
            //{
            //    // PlayerInputHandlerで保存した一番近い位置にあるIInteractableを取得する
            //    interactable = inputHandler.interactObject;

            //    string interactableObjectTagName = inputHandler.tagName;
            //    switch (interactableObjectTagName)
            //    {
            //        case "Item":
            //            // アイテムのIInteractableを継承しているクラスにinteractableをキャストして、Interact()を呼び出す
            //            break;
            //        default:
            //            break;
            //    }
            //}
        }

        /// <summary>
        /// ローカル通知用の関数
        /// </summary>
        public override void Render()
        {
            if (isHolding)
                OnPickUpItem?.Invoke();

            if (!isAlive)
                OnPlayerDied?.Invoke();
        }

        /// <summary>
        /// モンスターからダメージを受けたときに実行される関数
        /// </summary>
        public void TakeDamage()
        {
            if (!isAlive) return;
            isAlive = false;
        }

        public void Interact()
        {
        }
    }
}
