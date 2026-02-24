using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NetworkUIChange : MonoBehaviour, INetworkRunnerCallbacks
{


    /// <summary>
    /// 新しいプレイヤーがセッションに参加した時に自動で呼ばれるコールバック。
    /// プレイヤー用キャラクターの生成や、参加時の初期設定などを行う場所。
    /// </summary>
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("JOIN");
        UpdateCount(runner);
    }


    /// <summary>
    /// プレイヤーがセッションから離脱した時に自動で呼ばれるコールバック。
    /// プレイヤーが操作していたネットワークオブジェクトの削除処理や、
    /// 人数管理・UI更新・プレイヤーリスト整理などを行う。
    /// </summary>
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("OUT");
        UpdateCount(runner);
    }


    /// <summary>
    /// ルームに参加しているプレイヤーテキストの人数を更新する処理
    /// </summary>
    private void UpdateCount(NetworkRunner runner)
    {
        int count = runner.ActivePlayers.Count();
        TitleCanvasDisplaySettings.Instance.playerCountDisplayText.text = $"Player : {count} / 4 ";
    }

    /// <summary>
    /// NetworkRunner がシャットダウンした時に呼ばれるコールバック。
    /// セッション終了やエラー発生、手動による Shutdown() 呼び出しなどで発生。
    /// ネットワーク終了時の後片付け（UI戻し、オブジェクト破棄、状態リセットなど）を行う。
    /// </summary>
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }



    /// <summary>
    /// 毎フレーム呼ばれる入力送信コールバック。
    /// キーボード・マウス・ゲームパッドなどのローカル入力を取得し、
    /// NetworkInputData に詰めてサーバーへ送信する。
    /// プレイヤー移動やアクションなど、全プレイヤーの同期に必要な入力はここで扱う。
    /// </summary>
    public void OnInput(NetworkRunner runner, NetworkInput input) { }



    /// <summary>
    /// クライアントから入力が届かなかった tick で呼ばれるコールバック。
    /// 回線遅延・ラグ・一時的な切断などで入力が欠けた場合に、
    /// 代わりにどんな入力として扱うかを指定できる。
    /// 通常は前回の入力を継続したり、空の入力を渡したりして補完する。
    /// </summary>
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    /// <summary>
    /// クライアントがサーバー（ホスト）への接続に成功した時に呼ばれるコールバック。
    /// セッション参加の確定タイミングで、UI更新やロード処理、
    /// プレイヤー生成の準備などを行う。
    /// </summary>
    public void OnConnectedToServer(NetworkRunner runner) { }



    /// <summary>
    /// クライアントがサーバーとの接続を失った時に呼ばれるコールバック。
    /// 回線切断・タイムアウト・ホスト側の終了など、
    /// 何らかの理由で通信が維持できなくなった際の後処理を行う。
    /// </summary>
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }



    /// <summary>
    /// クライアントがサーバーへ接続要求を送ってきた時に呼ばれるコールバック。
    /// ここで接続を許可（Approve）するか、拒否（Refuse/Reject）するか判断できる。
    /// パスワード認証や人数制限チェックなど、入室可否の判定に使用する。
    /// </summary>
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }



    /// <summary>
    /// クライアントがサーバーへの接続を試みたが失敗した時に呼ばれるコールバック。
    /// ネットワーク不良・サーバーが存在しない・モード不一致などが原因。
    /// UIでエラーメッセージ表示やリトライ処理に使用する。
    /// </summary>
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }


    /// <summary>
    /// 他のプレイヤー（またはサーバー）が SendUserSimulationMessage() を使って
    /// 任意データを送信してきた時に呼ばれるコールバック。
    /// ゲーム内のカスタムイベント伝達に便利（チャット、通知、エモートなど）。
    /// </summary>
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }



    /// <summary>
    /// 現在参加可能なセッション（ゲーム部屋）の一覧が更新された時に呼ばれる。
    /// ロビー画面のリスト更新や、「部屋が増えた・消えた」をUIに反映するのに使う。
    /// </summary>
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }



    /// <summary>
    /// カスタム認証（外部サービスや独自APIなど）を使った時、
    /// サーバーから認証結果が返ってきた瞬間に呼ばれるコールバック。
    /// ログイン成功/失敗や、ユーザー固有データの受信に使える。
    /// </summary>
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }




    /// <summary>
    /// ホストモードでホストが切断された時、
    /// 新しいホストに自動で引き継がれる処理を行うためのコールバック。
    /// ゲームの継続・オブジェクトの再割り当てなどを行う。
    /// </summary>
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }



    /// <summary>
    /// 全クライアントのシーンロード完了時に呼ばれる。
    /// ロード完了後の初期化処理やスポーン処理を開始するためのコールバック。
    /// </summary>
    public void OnSceneLoadDone(NetworkRunner runner) { }



    /// <summary>
    /// ネットワークシーンのロード開始時に呼ばれる。
    /// ローディング画面の表示など、遷移中の準備処理を行う。
    /// </summary>
    public void OnSceneLoadStart(NetworkRunner runner) { }




    /// <summary>
    /// オブジェクトがプレイヤーのAOI(興味領域)から外れた時に呼ばれる。
    /// 視界外に出たオブジェクトの非表示処理や更新停止などを行う。
    /// </summary>
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }





    /// <summary>
    /// オブジェクトがプレイヤーのAOI(興味領域)に入った時に呼ばれる。
    /// 表示や動作の有効化など、視界に入ったオブジェクトの初期処理を行う。
    /// </summary>
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }



    /// <summary>
    /// 他クライアントから送られたReliableデータ受信時に呼ばれる。
    /// 確実に届けたい重要データの処理を行う。
    /// </summary>
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }



    /// <summary>
    /// Reliableデータの送受信進捗が更新された時に呼ばれる。
    /// 大容量データの進捗表示や転送状況の監視に使用する。
    /// </summary>
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
}
