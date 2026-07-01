// -----------------------------------------------------------------------------------
// ロビーUIの制御クラス
// LobbyUI.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class LobbyUI : MonoBehaviour
{
    // テキストを一定時間表示させるための時間
    private const float DISPLAY_TIME = 1.0f;

    [Header("UIDocument")]
    [SerializeField] private UIDocument uiDocument = null;

    [Header("VisualTreeAsset 一覧")]
    [SerializeField] private UIAssetData uIAssetData = null;

    [Header("タイトルボタン参照用")]
    [SerializeField] private TitleButtonController titleButtonController = null;



    // ゲームスタートのボタン
    public Button gameStartButton = null;

    // プレイヤーの人数を表示するラベル
    public Label playerCount = null;

    // ルームのPINコードを表示するラベル
    public Label roomPIN = null;

    // プレイヤーの人数不足を知らせるラベル
    public Label lackOfPersonnel = null;

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

        // プレイヤー人数不足を知らせるラベルを非表示にする
        lackOfPersonnel.style.display = DisplayStyle.None;

        // スタートボタンが押されたときのイベント登録
        gameStartButton.clicked += titleButtonController.ClickStartButton;

        // NetworkGameStarter のインスタンスを取得　
        networkGameStarter = NetworkGameStarter.Instance;

        // ホスト用のロビーUI表示を更新する
        networkGameStarter.networkLobbyUI.DisplayHostUI(networkGameStarter.networkRunner, this);
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
}
