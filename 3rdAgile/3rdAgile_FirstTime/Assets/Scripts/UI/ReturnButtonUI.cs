// -----------------------------------------------------------------------------------
// ホスト・クライアントそれぞれのタイトルへ戻る処理を管理するクラス
// ReturnButtonUI.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using Fusion;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class ReturnButtonUI : NetworkBehaviour
{
    // ホスト用のシャットダウン待機時間
    private readonly int HOST_SHUTDOWN_WAIT_TIME_MS = 5000;
    // クライアント用のシャットダウン待機時間
    private readonly int CLIENT_SHUTDOWN_WAIT_TIME_MS = 2000;

    // フォーカス時の色（灰色）
    private readonly Color FOCUSE_BUTTON_BACKIMAGE_COLOR = new Color(0.5f, 0.5f, 0.5f, 1.0f);

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

            // ゲームバッドがつながっているとき
            if (Gamepad.current != null)
            {
                // 部屋作成部分にフォーカスを当てる
                StartCoroutine(FocusDelay(hostButton));
            }
        }
        else // クライアントのみ
        {
            // クライアント用のルーム退出ボタンを表示
            clientButton.style.display = DisplayStyle.Flex;

            // ゲームバッドがつながっているとき
            if (Gamepad.current != null)
            {
                // 部屋作成部分にフォーカスを当てる
                StartCoroutine(FocusDelay(clientButton));
            }
        }

        // イベント登録
        hostButton.clicked += HostClickedRoomDisband;
        clientButton.clicked += ClientClickedLeaveRoom;

        // マウスが入った＆出たを登録解除
        hostButton.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
        hostButton.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        clientButton.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
        clientButton.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
    }


    /// <summary>
    /// イベント登録解除
    /// </summary>
    private void OnDisable()
    {
        hostButton.clicked -= HostClickedRoomDisband;
        clientButton.clicked -= ClientClickedLeaveRoom;

        // マウスが入った＆出たを登録解除
        hostButton.UnregisterCallback<MouseEnterEvent>(OnMouseEnter);
        hostButton.UnregisterCallback<MouseLeaveEvent>(OnMouseLeave);
        clientButton.UnregisterCallback<MouseEnterEvent>(OnMouseEnter);
        clientButton.UnregisterCallback<MouseLeaveEvent>(OnMouseLeave);
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
    public IEnumerator FocusDelay(Button focusButton)
    {
        yield return null;
        focusButton.Focus();
        focusButton.style.unityBackgroundImageTintColor = FOCUSE_BUTTON_BACKIMAGE_COLOR;
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
