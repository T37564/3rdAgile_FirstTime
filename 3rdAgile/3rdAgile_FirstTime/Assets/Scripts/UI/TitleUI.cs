// -----------------------------------------------------------------------------------
// タイトルのUIを切り替えるクラス
// TitleUI.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class TitleUI : MonoBehaviour
{
    // メッセージログ表示時間
    private readonly float MESSAGELOG_DISPLAY_TIME = 3.0f;

    // フォーカス時の色（灰色）
    private readonly Color FOCUSE_BUTTON_BACKIMAGE_COLOR = new Color(0.5f, 0.5f, 0.5f, 1.0f);

    [Header("タイトルのUIDocument")]
    [SerializeField] private UIDocument uiDocument = null;

    [Header("タイトルで使用するVisualTreeAsset")]
    [SerializeField] private UIAssetData uIAssetData = null;

    [Header("タイトルボタン参照用")]
    [SerializeField] private TitleButtonController titleButtonController = null;

    // ルーム作成ボタン
    private Button createRoom = null;
    // ルーム入室ボタン
    private Button enterRoom = null;

    // メッセージログのVisualElement
    private VisualElement messageLog = null;

    // フォーカスされているUI
    private Focusable nowFocusedButton = null;

    /// <summary>
    /// UI要素を取得し、各種イベントを登録する
    /// </summary>
    private void OnEnable()
    {
        // タイトルのUIに切り替える
        uiDocument.rootVisualElement.Clear();
        uIAssetData.titleUI.CloneTree(uiDocument.rootVisualElement);

        VisualElement root = uiDocument.rootVisualElement;

        // CreateRoomボタンとEnterRoomボタンを取得
        createRoom = root.Q<Button>("CreateRoom");
        enterRoom = root.Q<Button>("EnterRoom");

        // UXML内からMessageLogを取得
        messageLog = root.Q<VisualElement>("MessageLog");

        // イベント登録
        createRoom.clicked += titleButtonController.OnClickCreateRoomButton;
        enterRoom.clicked += titleButtonController.OnClickEnterRoomButton;

        // マウスが入った＆出たを登録
        createRoom.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
        createRoom.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        enterRoom.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
        enterRoom.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);

        // ゲームバッドがつながっているとき
        if (Gamepad.current != null)
        {
            // 部屋作成部分にフォーカスを当てる
            StartCoroutine(FocusDelay());
        }
    }

    /// <summary>
    /// 登録したイベントを解除する
    /// </summary>
    private void OnDisable()
    {
        if (uiDocument.rootVisualElement == null) return;
        uiDocument.rootVisualElement.Clear();

        if (createRoom != null)
        {
            createRoom.clicked -= titleButtonController.OnClickCreateRoomButton;
        }
        if (enterRoom != null)
        {
            enterRoom.clicked -= titleButtonController.OnClickEnterRoomButton;
        }

        // マウスが入った＆出たを登録解除
        createRoom.UnregisterCallback<MouseEnterEvent>(OnMouseEnter);
        createRoom.UnregisterCallback<MouseLeaveEvent>(OnMouseLeave);
        enterRoom.UnregisterCallback<MouseEnterEvent>(OnMouseEnter);
        enterRoom.UnregisterCallback<MouseLeaveEvent>(OnMouseLeave);
    }

    /// <summary>
    /// フォーカス中のボタンの色を更新する
    /// </summary>
    private void Update()
    {
        Focusable focusedElement = uiDocument.rootVisualElement.panel.focusController.focusedElement;

        if (focusedElement == nowFocusedButton) return;

        // 前のボタンを元の色に戻す
        if (nowFocusedButton is Button previousButton)
        {
            previousButton.style.unityBackgroundImageTintColor = Color.white;
        }

        // 新しくフォーカスされたボタンを選択中の色にする
        if (focusedElement is Button focusedButton)
        {
            focusedButton.style.unityBackgroundImageTintColor = FOCUSE_BUTTON_BACKIMAGE_COLOR;
        }

        nowFocusedButton = focusedElement;
    }

    /// <summary>
    ///  接続失敗メッセージを一定時間表示する
    /// </summary>
    public IEnumerator MessageLogDisplay()
    {
        messageLog.style.display = DisplayStyle.Flex;

        yield return new WaitForSeconds(MESSAGELOG_DISPLAY_TIME);

        messageLog.style.display = DisplayStyle.None;
    }

    /// <summary>
    /// マウスがボタンに入ったときに選択中の色に変更する
    /// </summary>
    public void OnMouseEnter(MouseEnterEvent mouseEnterEvent)
    {
        if (mouseEnterEvent.currentTarget is Button button)
        {
            button.style.unityBackgroundImageTintColor = FOCUSE_BUTTON_BACKIMAGE_COLOR;
        }
    }

    /// <summary>
    /// マウスがボタンから離れたときに色を元に戻す
    /// </summary>
    public void OnMouseLeave(MouseLeaveEvent mouseEnterEvent)
    {
        if (mouseEnterEvent.currentTarget is Button button)
        {
            button.style.unityBackgroundImageTintColor = Color.white;
        }
    }

    /// <summary>
    /// レイアウト計算が終わった後に部屋作成部分にフォーカスを当てる処理
    /// </summary>
    public IEnumerator FocusDelay()
    {
        yield return null;
        createRoom.Focus();
    }
}
