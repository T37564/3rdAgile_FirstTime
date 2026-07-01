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

    [Header("ゲームパッド用のボタンUI")]
    [SerializeField] private Image actionImageGamepad = null;
    [Header("キーボード・マウス用のボタンUI")]
    [SerializeField] private Image actionImageMouce = null;

    // 現在触れているアイテムを保持する
    private Collider currentItem = null;

    // プレイヤーコントローラー参照用
    private PlayerController playerController = null;

    // アイテム納品箱まで案内する矢印
    private GameObject greenArrow = null;

    /// <summary>
    /// 参照用プレイヤーコントローラー取得、UIの初期化
    /// </summary>
    private void Start()
    {
        playerController = GetComponentInParent<PlayerController>();

        greenArrow = GameObject.FindGameObjectWithTag(GREEN_ARROW_TAG_NAME);

        // 最初はUIを非表示
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
        if (other.CompareTag(ITEM_TAG_NAME))
        {
            currentItem = other;
            ActionUIDisplay(true);
        }
    }

    /// <summary>
    /// アイテムが離れたときにUIを非表示
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (playerController == null || !playerController.HasInputAuthority) return;

        // 触れていたアイテムに離れたときUI非表示
        if (other == currentItem)
        {
            currentItem = null;
            ActionUIDisplay(false);
        }
    }

    /// <summary>
    /// 行動UIの表示状態を更新する
    /// </summary>
    private void Update()
    {
        if (playerController == null || !playerController.HasInputAuthority) return;

        // アイテムを持っている間は拾うUIを非表示にし、納品先への矢印を表示
        if (playerController.IsHoldingItem)
        {
            ActionUIDisplay(false);
            SetVisible(true);
            return;
        }

        // 対象アイテムが無くなったらUIと矢印を更新
        if (currentItem == null)
        {
            ActionUIDisplay(false);
            SetVisible(false);
        }
    }

    /// <summary>
    /// 行動UIの表示・非表示を切り替え、入力デバイスに応じたボタン画像切り替え
    /// </summary>
    private void ActionUIDisplay(bool display)
    {
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
    /// 納品先への矢印の表示・非表示を切り替える
    /// </summary>
    public void SetVisible(bool visible)
    {
        greenArrow.gameObject.SetActive(visible);
    }
}