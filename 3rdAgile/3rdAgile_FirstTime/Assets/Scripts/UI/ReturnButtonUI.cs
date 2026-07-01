// -----------------------------------------------------------------------------------
// ホスト・クライアントそれぞれのタイトルへ戻る処理を管理するクラス
// ReturnButtonUI.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using Fusion;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class ReturnButtonUI : NetworkBehaviour
{
    // ホスト用のシャットダウン待機時間
    private readonly int HOST_SHUTDOWN_WAIT_TIME_MS = 5000;
    // クライアント用のシャットダウン待機時間
    private readonly int CLIENT_SHUTDOWN_WAIT_TIME_MS = 2000;

    [Header("ローディング時に表示するUI")]
    [SerializeField] private GameObject loadingCanvas = null;

    // NetworkRunnerの参照用
    private NetworkRunner runner = null;

    // ホスト用のルーム解散ボタン
    public Button hostButton = null;
    // クライアント用のルーム退出ボタン
    public Button clientButton = null;

    // メッセージログのVisualElement
    public VisualElement messageLog = null;

    /// <summary>
    /// ボタンUIの表示処理、イベント登録処理
    /// </summary>
    private void OnEnable()
    {
        runner = NetworkGameStarter.Instance.networkRunner;

        if (runner == null) return;

        // ホストのみ
        if (runner.IsServer)
        {
            // ホスト用のルーム解散ボタンを表示
            hostButton.style.display = DisplayStyle.Flex;
        }
        else // クライアントのみ
        {
            // クライアント用のルーム退出ボタンを表示
            clientButton.style.display = DisplayStyle.Flex;
        }

        // イベント登録
        hostButton.clicked += HostClickedRoomDisband;
        clientButton.clicked += ClientClickedLeaveRoom;
    }

    /// <summary>
    /// イベント登録解除
    /// </summary>
    private void OnDisable()
    {
        hostButton.clicked -= HostClickedRoomDisband;
        clientButton.clicked -= ClientClickedLeaveRoom;
    }

    /// <summary>
    /// 全員にルーム解散の通知を送るRPC
    /// </summary>
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_DisbandRoom()
    {
        _ = DisbandRoom();
    }

    /// <summary>
    /// ホストがチーム解散ボタンを押した際の処理
    /// </summary>
    private async Task DisbandRoom()
    {
        // ホストのみ
        if (runner.IsServer)
        {
            // ローディング画面表示
            loadingCanvas.SetActive(true);

            await Task.Delay(HOST_SHUTDOWN_WAIT_TIME_MS);
        }
        else // ゲストのみ
        {
            // ホストがルームを解散したことを知らせるメッセージを表示
            messageLog.style.display = DisplayStyle.Flex;

            await Task.Delay(CLIENT_SHUTDOWN_WAIT_TIME_MS);

            // ローディング画面表示
            loadingCanvas.SetActive(true);

            await Task.Delay(CLIENT_SHUTDOWN_WAIT_TIME_MS);
        }

        // NetworkRunnerをシャットダウンする
        NetworkGameStarter.Instance.ShutdownRunner();
    }

    /// <summary>
    /// クライアント用のルーム退出処理
    /// </summary>
    private async Task ClientLeaveRoom()
    {
        // ローディング画面表示
        loadingCanvas.SetActive(true);

        await Task.Delay(CLIENT_SHUTDOWN_WAIT_TIME_MS);

        // NetworkRunnerをシャットダウンする
        NetworkGameStarter.Instance.ShutdownRunner();
    }


    /// <summary>
    /// ホストがチーム解散ボタンを押した際に発動
    /// </summary>
    private void HostClickedRoomDisband()
    {
        RPC_DisbandRoom();
    }

    /// <summary>
    /// クライアントがチームを抜けるボタンを押した際に発動
    /// </summary>
    private void ClientClickedLeaveRoom()
    {
        _ = ClientLeaveRoom();
    }
}
