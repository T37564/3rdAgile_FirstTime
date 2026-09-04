// -----------------------------------------------------------------------------------
// ロビーUIの制御クラス
// LobbyUI.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class LobbyUI : MonoBehaviour
{
    // テキストを一定時間表示させるための時間
    private const float DISPLAY_TIME = 1.0f;

    // フォーカス時の色（灰色）
    private readonly Color FOCUSE_BUTTON_BACKIMAGE_COLOR = new Color(0.5f, 0.5f, 0.5f, 1.0f);

    [Header("UIDocument")]
    [SerializeField] private UIDocument uiDocument = null;

    [Header("VisualTreeAsset 一覧")]
    [SerializeField] private UIAssetData uIAssetData = null;

    [Header("タイトルボタン参照用")]
    [SerializeField] private TitleButtonController titleButtonController = null;
    [Header("タイトルUI参照用")]
    [SerializeField] private TitleUI titleUI = null;



    // ゲームスタートのボタン
    public Button gameStartButton = null;

    // プレイヤーの人数を表示するラベル
    public Label playerCount = null;

    // ルームのPINコードを表示するラベル
    public Label roomPIN = null;

    // プレイヤーの人数不足を知らせるラベル
    public Label lackOfPersonnel = null;

    // 接続切断メッセージを表示するUI
    public VisualElement dsconnectedMessage = null;

    // NetworkGameStarterの参照用
    private NetworkGameStarter networkGameStarter = null;


    private void OnEnable()
    {
        // ロビーUIに変更
        uiDocument.rootVisualElement.Clear();
        uIAssetData.lobbyUI.CloneTree(uiDocument.rootVisualElement);

        // ロビーUIのVisualElementを探す
        VisualElement root = uiDocument.rootVisualElement;

        // UXML内からButton "StartButton" を取得
        gameStartButton = root.Q<Button>("StartButton");

        // UXML内から各Labelを取得
        playerCount = root.Q<Label>("CountText");
        roomPIN = root.Q<Label>("PINText");
        lackOfPersonnel = root.Q<Label>("LackOfPersonnel");
        dsconnectedMessage = root.Q<VisualElement>("DisconnectedMessage");

        // プレイヤー人数不足を知らせるラベルを非表示にする
        lackOfPersonnel.style.display = DisplayStyle.None;
        dsconnectedMessage.style.display = DisplayStyle.None;

        // スタートボタンが押されたときのイベント登録
        gameStartButton.clicked += titleButtonController.ClickStartButton;

        // マウスが入った＆出たを登録解除
        gameStartButton.RegisterCallback<MouseEnterEvent>(titleUI.OnMouseEnter);
        gameStartButton.RegisterCallback<MouseLeaveEvent>(titleUI.OnMouseLeave);

        // NetworkGameStarter のインスタンスを取得　
        networkGameStarter = NetworkGameStarter.Instance;

        // ホスト用のロビーUI表示を更新する
        networkGameStarter.networkLobbyUI.DisplayHostUI(networkGameStarter.networkRunner, this);

        // ゲームバッドがつながっているとき
        if (Gamepad.current != null)
        {
            // 部屋作成部分にフォーカスを当てる
            StartCoroutine(FocusGameStartButton());
        }
    }

    /// <summary>
    /// イベント登録解除
    /// </summary>
    private void OnDisable()
    {
        if (gameStartButton != null)
        {
            gameStartButton.clicked -= titleButtonController.ClickStartButton;
        }

        // マウスが入った＆出たを登録解除
        gameStartButton.UnregisterCallback<MouseEnterEvent>(titleUI.OnMouseEnter);
        gameStartButton.UnregisterCallback<MouseLeaveEvent>(titleUI.OnMouseLeave);
    }

    /// <summary>
    /// プレイヤー人数不足のメッセージを一定時間表示するコルーチン
    /// </summary>
    public IEnumerator ActiveLackOfPersonnel()
    {
        lackOfPersonnel.style.display = DisplayStyle.Flex;

        yield return new WaitForSecondsRealtime(DISPLAY_TIME);

        lackOfPersonnel.style.display = DisplayStyle.None;
    }

    /// <summary>
    /// ゲームパッド使用中にボタンをフォーカスする処理
    /// </summary>
    private IEnumerator FocusGameStartButton()
    {
        yield return null;

        gameStartButton.Focus();
        gameStartButton.style.unityBackgroundImageTintColor = FOCUSE_BUTTON_BACKIMAGE_COLOR;
    }

    /// <summary>
    /// ホストからの接続が切断されたときに表示するメッセージ
    /// </summary>
    public void DisplayDisconnectedMessage()
    {
        dsconnectedMessage.style.display = DisplayStyle.Flex;
    }
}
