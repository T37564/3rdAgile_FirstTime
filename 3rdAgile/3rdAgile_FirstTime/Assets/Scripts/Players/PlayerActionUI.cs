// -----------------------------------------------------------------------------------
// プレイヤーがアイテムを拾える状況で表示する行動UIの制御クラス
// PlayerActionUI.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using Network.Player;

public class PlayerActionUI : MonoBehaviour
{
    // UI表示時に使用する文字
    private readonly string TO_PICK_UP = "拾う";

    // アイテム納品箱まで案内する矢印のタグ名
    private readonly string GREEN_ARROW_TAG_NAME = "GreenArrow";

    // アイテムのタグ名
    private readonly string ITEM_TAG_NAME = "Item";

    [Header("実行可能なアクションを表示するテキスト")]
    [SerializeField] private TextMeshProUGUI actionText = null;

    [Header("アイテム取得時に使用するキャンバス")]
    [SerializeField] private GameObject worldSpaceCanvas = null;

    [Header("ゲームパッド用のボタンUI")]
    [SerializeField] private Image actionImageGamepad = null;
    [Header("キーボード・マウス用のボタンUI")]
    [SerializeField] private Image actionImageMouce = null;

    [Header("アイテムの金額表示テキスト")]
    [SerializeField] private TextMeshProUGUI itemPriceText = null;
    [Header("アイテムの運搬人数表示テキスト")]
    [SerializeField] private TextMeshProUGUI itemTransportCountText = null;

    // 現在触れているアイテムを保持する
    private Collider currentItem = null;

    // プレイヤーコントローラー参照用
    private PlayerController playerController = null;

    // アイテムの情報
    private Item itemData = null;

    // アイテム納品箱まで案内する矢印
    private GameObject greenArrow = null;

    // アイテムの運搬人数参照用
    private ItemInteractable itemInteractable = null;

    private bool isCarryingItemExited = false;

    /// <summary>
    /// 参照用プレイヤーコントローラー取得、UIの初期化
    /// </summary>
    private void Start()
    {
        playerController = GetComponentInParent<PlayerController>();

        greenArrow = GameObject.FindGameObjectWithTag(GREEN_ARROW_TAG_NAME);

        ActionUIDisplay(false);

        // PlayerController が取得できなかった場合は処理を終了
        if (playerController == null) return;

        // 自分が操作していないプレイヤーのUIは無効化する
        if (!playerController.HasInputAuthority)
        {
            gameObject.SetActive(false);
            return;
        }
    }

    /// <summary>
    /// アイテムに触れたときにUIを表示する
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (playerController == null || !playerController.HasInputAuthority) return;

        // Itemタグのオブジェクトに触れたときUIを表示
        if (other.CompareTag(ITEM_TAG_NAME) && itemInteractable == null)
        {
            itemInteractable = other.GetComponent<ItemInteractable>();
            itemData = other.GetComponent<Item>();
            currentItem = other;
            ActionUIDisplay(true);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (playerController == null || !playerController.HasInputAuthority) return;

        // アイテムに触れている間はUIの位置をアイテムの位置に合わせて更新する
        if (other.CompareTag(ITEM_TAG_NAME))
        {
            ItemUIChangePosition(other.transform);

            // アイテムを持っていて運搬人数が必要人数に達した
            if (itemInteractable.carriersItem.Count == int.Parse(itemData.ItemData.ItemTransportCount) && itemInteractable.IsHaveItem)
            {
                ActionUIDisplay(false);
                SetVisible(true);
            }
            else if (itemInteractable.IsHaveItem)
            {
                // まだ人数不足
                ActionUIDisplay(true);
                SetVisible(false);
            }
            else
            {
                // アイテムを持っていない状態
                ActionUIDisplay(true);
                SetVisible(false);
            }
        }
    }

    /// <summary>
    /// アイテムが離れたときにUIを非表示
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (playerController == null || !playerController.HasInputAuthority) return;

        // 持っているアイテムから離れた場合UI非表示
        if (other == currentItem && itemInteractable.IsHaveItem)
        {
            isCarryingItemExited = true;
            ActionUIDisplay(false);
        }
        else if (other == currentItem)// アイテムを持っていない状態で離れた場合UI非表示
        {
            isCarryingItemExited = false;
            currentItem = null;
            itemInteractable = null;
            ActionUIDisplay(false);
            SetVisible(false);
        }
    }

    /// <summary>
    /// 行動UIの表示状態を更新する
    /// </summary>
    private void Update()
    {
        if (playerController == null || !playerController.HasInputAuthority) return;

        // アイテムを離したとき
        if (itemInteractable == null)
        {
            ActionUIDisplay(false);
            SetVisible(false);
            return;
        }

        // 複数人アイテムを運んでいて運搬人数が足りなくなった時
        if (itemInteractable != null && itemInteractable.carriersItem.Count != int.Parse(itemData.ItemData.ItemTransportCount) && itemInteractable.IsHaveItem)
        {
            ActionUIDisplay(true);
            SetVisible(false);
        }

        // アイテムを遠くから離したとき
        if (itemInteractable != null && !itemInteractable.IsHaveItem && isCarryingItemExited)
        {
            isCarryingItemExited = false;
            itemInteractable = null;
        }
    }

    /// <summary>
    /// 行動UIの表示・非表示を切り替え、入力デバイスに応じたボタン画像切り替え
    /// </summary>
    private void ActionUIDisplay(bool display)
    {
        worldSpaceCanvas.SetActive(display);

        if (itemData != null && itemInteractable != null)
        {
            // アイテムの金額と運搬人数をUIに反映する
            itemPriceText.text = itemData.ItemData.ItemPrice + "$";
            itemTransportCountText.text = itemInteractable.carriersItem.Count + "/" + itemData.ItemData.ItemTransportCount;
        }

        // 「拾う」テキストを表示、非表示にする
        if (display)
        {
            actionText.text = TO_PICK_UP;
        }
        else
        {
            actionText.text = "";
        }

        // ボタン画像を一度すべて非表示にする
        actionImageGamepad.enabled = false;
        actionImageMouce.enabled = false;

        // UI非表示時は処理を終了する
        if (!display) return;

        // 使用中の入力デバイスに応じてボタンUIを表示する
        if (Gamepad.current != null)
        {
            // ゲームパッド版UIの表示
            actionImageGamepad.enabled = true;
        }
        else
        {
            // キーボード・マウス用UIを表示
            actionImageMouce.enabled = true;
        }
    }

    /// <summary>
    /// UIの位置をアイテムの位置に合わせて更新する処理
    /// </summary>
    private void ItemUIChangePosition(Transform itemPosition)
    {
        worldSpaceCanvas.transform.position = itemPosition.position;
    }

    /// <summary>
    /// 納品先への矢印の表示・非表示を切り替える
    /// </summary>
    public void SetVisible(bool visible)
    {
        greenArrow.gameObject.SetActive(visible);
    }
}