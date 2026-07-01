// -----------------------------------------------------------------------------------
// タイトルのUIを切り替えるクラス
// TitleUI.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using System.Collections;
using UnityEngine;
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

    /// <summary>
    ///  接続失敗メッセージを一定時間表示する
    /// </summary>
    public IEnumerator MessageLogDisplay()
    {
        messageLog.style.display = DisplayStyle.Flex;

        yield return new WaitForSeconds(MESSAGELOG_DISPLAY_TIME);

        messageLog.style.display = DisplayStyle.None;
    }
}
