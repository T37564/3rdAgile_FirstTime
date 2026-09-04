// -----------------------------------------------------------------------------------
// ロビー入室時にプレイヤーモデルを表示するクラス
// MatchingPlayerObjectSpawner.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MatchingPlayerObjectSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    // プレイヤーがスポーンしたときの生成位置
    private Vector3[] playerSpawnPosition = null;

    // プレイヤーがスポーンしたときの回転
    private Quaternion[] playerSpawnRotation = null;

    // プレイヤーPrefab一覧
    private NetworkObject[] playerPrefab = null;

    // プレイヤーのPrefabの情報が入ったScriptableObject
    private PlayerPrefabData playerPrefabData = null;

    // どのプレイヤーがどのオブジェクトを操作しているか
    private Dictionary<PlayerRef, NetworkObject> playerObjects = new Dictionary<PlayerRef, NetworkObject>();

    // どのプレイヤーがどのインデックスを使用しているか
    private List<int> usedPlayerIndexes = new List<int>();

    // どのプレイヤーがどのインデックスを使用しているかを管理する辞書
    private Dictionary<PlayerRef, int> playerIndexes = new Dictionary<PlayerRef, int>();

    // ホストプレイヤーのPlayerRefを保持する変数
    public PlayerRef hostPlayer= PlayerRef.None;

    /// <summary>
    /// ScriptableObjectからPrefab情報を取得
    /// </summary>
    private void Awake()
    {
        // ResourcesフォルダからPlayerPrefabDataを読み込む
        playerPrefabData = Resources.Load<PlayerPrefabData>("PlayerPrefabData/PrefabData");

        // PlayerPrefabDataが存在しない場合
        if (playerPrefabData == null)
        {
            Debug.LogError("PlayerPrefabData が見つかりません");
            return;
        }

        // ScriptableObjectからPrefab情報を取得
        playerPrefab = playerPrefabData.playerPrefabs;
        playerSpawnPosition = playerPrefabData.playerSpawnPositions;
        playerSpawnRotation = playerPrefabData.playerSpawnRotations;
    }



    #region Player
    /// <summary>
    /// 新しいプレイヤーがセッションに参加した時に自動で呼ばれるコールバック。
    /// プレイヤー生成や参加時初期化を行う。
    /// </summary>
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        // ホストのみ処理をする
        if (!runner.IsServer) return;

        NetworkObject spawnedPlayerObject = null;

        // あいているインデックスを取得
        int index = GetAvailablePlayerIndex();


        // ホストのプレイヤーを記録する
        if (player == runner.LocalPlayer)
        {
            hostPlayer = player;
        }

        // オブジェクト生成処理
        spawnedPlayerObject = runner.Spawn(playerPrefab[index], playerSpawnPosition[index],
                                                    playerSpawnRotation[index], player);

        playerObjects[player] = spawnedPlayerObject;
        playerIndexes[player] = index;

        // 使用済みインデックスを追加
        usedPlayerIndexes.Add(index);
    }

    /// <summary>
    /// あいているインデックスを取得する処理
    /// </summary>
    private int GetAvailablePlayerIndex()
    {
        for (int i = 0; i < playerPrefab.Length; i++)
        {
            if (!usedPlayerIndexes.Contains(i)) return i;
        }

        return -1;
    }

    /// <summary>
    /// プレイヤーがセッションから離脱した時に自動で呼ばれるコールバック。
    /// プレイヤーが操作していたネットワークオブジェクトの削除処理や、
    /// 人数管理・UI更新・プレイヤーリスト整理などを行う。
    /// </summary>
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        // ホストのみ処理
        if (!runner.IsServer) return;

        // ゲスト側が抜けた際の処理
        if (playerObjects.TryGetValue(player, out NetworkObject playerObject))
        {
            runner.Despawn(playerObject);

            if (playerIndexes.TryGetValue(player, out int index))
            {
                usedPlayerIndexes.Remove(index);
                playerIndexes.Remove(player);
            }

            playerObjects.Remove(player);
        }
    }
    #endregion

    #region Input
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
    #endregion

    #region Connection
    /// <summary>
    /// NetworkRunner がシャットダウンした時に呼ばれるコールバック。
    /// セッション終了やエラー発生、手動による Shutdown() 呼び出しなどで発生。
    /// ネットワーク終了時の後片付け（UI戻し、オブジェクト破棄、状態リセットなど）を行う。
    /// </summary>
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

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
    #endregion

    #region Lobby
    /// <summary>
    /// 現在参加可能なセッション（ゲーム部屋）の一覧が更新された時に呼ばれる。
    /// ロビー画面のリスト更新や、「部屋が増えた・消えた」をUIに反映するのに使う。
    /// </summary>
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    #endregion

    #region Authentication
    /// <summary>
    /// カスタム認証（外部サービスや独自APIなど）を使った時、
    /// サーバーから認証結果が返ってきた瞬間に呼ばれるコールバック。
    /// ログイン成功/失敗や、ユーザー固有データの受信に使える。
    /// </summary>
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    #endregion

    #region Message
    /// <summary>
    /// 他のプレイヤー（またはサーバー）が SendUserSimulationMessage() を使って
    /// 任意データを送信してきた時に呼ばれるコールバック。
    /// ゲーム内のカスタムイベント伝達に便利（チャット、通知、エモートなど）。
    /// </summary>
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    #endregion

    #region Receive
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
    #endregion

    #region AOI
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
    #endregion

    #region Scene
    /// <summary>
    /// ネットワークシーンのロード開始時に呼ばれる。
    /// ローディング画面の表示など、遷移中の準備処理を行う。
    /// </summary>
    public void OnSceneLoadStart(NetworkRunner runner) { }

    /// <summary>
    /// 全クライアントのシーンロード完了時に呼ばれる。
    /// ロード完了後の初期化処理やスポーン処理を開始するためのコールバック。
    /// </summary>
    public void OnSceneLoadDone(NetworkRunner runner) { }
    #endregion

    #region Host Migration
    /// <summary>
    /// ホストモードでホストが切断された時、
    /// 新しいホストに自動で引き継がれる処理を行うためのコールバック。
    /// ゲームの継続・オブジェクトの再割り当てなどを行う。
    /// </summary>
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    #endregion
}
