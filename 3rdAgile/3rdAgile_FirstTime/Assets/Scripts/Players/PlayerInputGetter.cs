using Fusion;
using Fusion.Sockets;
using Network.Player;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputGetter : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private PlayerInputHandler inputHandler = null;

    public void RegisterLocalInput(PlayerInputHandler handler)
    {
        inputHandler = handler;
    }

    /// <summary>
    /// 各Tickごとに入力データをRunnerへ渡すために呼ばれる。
    /// InputSystemの値をNetworkInputへセットする場所。
    /// </summary>
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (inputHandler == null) return;

        PlayerInputData data = inputHandler.GetInput();

        input.Set(data);
    }

    #region このクラスでは使わないコールバック（空実装）
    /// <summary>
    /// シーンロードが完了したときに呼ばれる。
    /// ロード後の初期化処理を書く。
    /// </summary>
    public void OnSceneLoadDone(NetworkRunner runner)
    {

    }

    /// <summary>
    /// プレイヤーがセッションに参加したときに呼ばれる。
    /// ホストでスポーン処理を書くことが多い。
    /// </summary>
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }

    /// <summary>
    /// プレイヤーがセッションから退出したときに呼ばれる。
    /// プレイヤーオブジェクトの削除処理などを書く。
    /// </summary>
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

    /// <summary>
    /// Runnerがシャットダウンされたときに呼ばれる。
    /// セッション終了や強制切断時の後処理に使う。
    /// </summary>
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

    /// <summary>
    /// サーバーとの接続が切断されたときに呼ばれる。
    /// 切断理由に応じたUI表示などを行う。
    /// </summary>
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

    /// <summary>
    /// クライアントからの接続要求を受け取ったときに呼ばれる（Host側）。
    /// 接続を許可するかどうかを決める処理を書く。
    /// </summary>
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

    /// <summary>
    /// 接続に失敗したときに呼ばれる（クライアント側）。
    /// エラーメッセージ表示などに使用。
    /// </summary>
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

    /// <summary>
    /// ユーザー定義のシミュレーションメッセージを受信したときに呼ばれる。
    /// 独自メッセージ通信を使う場合に使用。
    /// </summary>
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }


    /// <summary>
    /// 特定プレイヤーの入力が取得できなかったときに呼ばれる。
    /// 入力補間やデフォルト入力を設定する用途。
    /// </summary>
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    /// <summary>
    /// サーバーへの接続が成功したときに呼ばれる。
    /// 接続完了後の初期処理を書く。
    /// </summary>
    public void OnConnectedToServer(NetworkRunner runner) { }

    /// <summary>
    /// セッション一覧が更新されたときに呼ばれる。
    /// ロビー画面の部屋リスト更新などに使用。
    /// </summary>
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }


    /// <summary>
    /// シーンロードが開始されたときに呼ばれる。
    /// ロード前の準備処理を書く。
    /// </summary>
    public void OnSceneLoadStart(NetworkRunner runner) { }

    /// <summary>
    /// カスタム認証のレスポンスを受け取ったときに呼ばれる。
    /// 外部認証を使用している場合に利用。
    /// </summary>
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    /// <summary>
    /// ホストマイグレーションが発生したときに呼ばれる。
    /// ホストが抜けた際の引き継ぎ処理を書く。
    /// </summary>
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

    /// <summary>
    /// Reliable通信でデータを受信したときに呼ばれる。
    /// 大きめのデータ送信などで使用。
    /// </summary>
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }

    /// <summary>
    /// Reliable通信の送受信進捗が更新されたときに呼ばれる。
    /// ダウンロード進行表示などに使用。
    /// </summary>
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    /// <summary>
    /// AOI（Area of Interest）からNetworkObjectが外れたときに呼ばれる。
    /// 対象プレイヤーにそのオブジェクトが見えなくなるタイミング。
    /// </summary>
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    /// <summary>
    /// AOI（Area of Interest）にNetworkObjectが入ったときに呼ばれる。
    /// 対象プレイヤーにそのオブジェクトが見えるようになるタイミング。
    /// </summary>
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    #endregion
}
