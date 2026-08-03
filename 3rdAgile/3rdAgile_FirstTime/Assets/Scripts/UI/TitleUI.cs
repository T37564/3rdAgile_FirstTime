// -----------------------------------------------------------------------------------
// タイトルのUIを切り替えるクラス
// TitleUI.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class TitleUI : MonoBehaviour
{
    // メッセージログ表示時間
    private readonly float MESSAGELOG_DISPLAY_TIME = 3.0f;

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

    private Focusable nowFocusedButton = null;

    private readonly Color FOCUSE_BUTTON_BACKIMAGE_COLOR = new Color(0.5f, 0.5f, 0.5f, 1.0f);

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

        // ゲームバッドがつながっているとき
        if (Gamepad.current != null)
        {
            // 部屋作成部分にフォーカスを当てる
            StartCoroutine(FocusDelay());
        }
    }

    /// <summary>
    /// イベント登録解除
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
    }

    private void Update()
    {
        if (nowFocusedButton == null) return;
        ((Button)nowFocusedButton).style.unityBackgroundImageTintColor =
   nowFocusedButton == uiDocument.rootVisualElement.panel.focusController.focusedElement ? Color.white : FOCUSE_BUTTON_BACKIMAGE_COLOR;

        nowFocusedButton = uiDocument.rootVisualElement.panel.focusController.focusedElement;
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
    /// レイアウト計算が終わった後に部屋作成部分にフォーカスを当てる処理
    /// </summary>
    private IEnumerator FocusDelay()
    {
        yield return null;
        createRoom.Focus();

        nowFocusedButton = uiDocument.rootVisualElement.panel.focusController.focusedElement;
        ((Button)nowFocusedButton).style.unityBackgroundImageTintColor = FOCUSE_BUTTON_BACKIMAGE_COLOR;
    }
}
