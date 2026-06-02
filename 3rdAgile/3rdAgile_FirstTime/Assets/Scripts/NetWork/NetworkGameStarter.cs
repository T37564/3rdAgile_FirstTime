// -----------------------------------------------------------------------------------
// ルーム作成、参加、シーン管理、ゲーム開始の司令塔。
// NetworkGameStarter.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkGameStarter : MonoBehaviour, INetworkRunnerCallbacks
{
    // 最大参加人数
    private readonly int MAX_PLAYER_COUNT = 4;

    // ルーム情報などを入れるNetworkRunner
    public NetworkRunner networkRunner = null;

    // NetworkRunner をアタッチするためのオブジェクト
    private GameObject networkRunnerObject = null;

    private System.Random random = new System.Random();

    public NetworkLobbyUI networkLobbyUI = null;

    // ランキング時に使用するチーム名
    public string TeamName { get; private set; } = "";

    // ルーム暗証番号
    public string PIN { get; private set; } = "";

    public static NetworkGameStarter Instance
    {
        get;
        private set;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// ホストとしてルームを作成する
    /// async をつけているので、ネットワーク処理の完了を待ってもゲームが止まらない
    /// </summary>
    public async void CreateHostRoom(string sessionName)
    {
        // チーム名を保存
        TeamName = sessionName;

        // ローディング画面を表示
        UIReferences.Instance.LoadingUI.SetActive(true);

        SetupNetworkRunner();

        // ルーム暗証番号が被る可能性があるため、成功するまでループして再生成する
        while (true)
        {
            // 仮部屋番号を生成
            string pin = RandomPIN();

            // Photon Cloudへ接続し、Hostとしてルームを作成する
            var result = await networkRunner.StartGame(new StartGameArgs()
            {
                // Hostモードでルームを作成する
                GameMode = GameMode.Host,
                // ルーム暗証番号
                SessionName = pin,
                // 最大マッチング人数
                PlayerCount = MAX_PLAYER_COUNT,
                // Fusionのシーン同期・シーン移動管理に使用
                SceneManager = networkRunnerObject.AddComponent<NetworkSceneManagerDefault>()
            });


            // 成功したらループを抜ける
            if (result.Ok)
            {
                // ローディング画面を非表示
                UIReferences.Instance.LoadingUI.SetActive(false);
                // ロビーUIを表示
                UIReferences.Instance.LobbyUI.SetActive(true);

                Debug.Log("ホスト側接続完了");

                // 正式に部屋番号を記録する
                PIN = pin;

                break;
            }

            // PIN被り
            if (result.ShutdownReason == ShutdownReason.GameIdAlreadyExists)
            {
                Debug.Log("PINが被ったため再生成");
            }
            else
            {
                Debug.Log($"別の原因で失敗 : {result.ShutdownReason}");
                return;
            }
        }


    }

    /// <summary>
    /// ランダムな6桁の暗証番号を生成する
    /// </summary>
    private string RandomPIN()
    {
        // 6桁のランダムな数字を生成
        return random.Next(100000, 999999).ToString();
    }


    /// <summary>
    /// ルームへ参加する処理（クライアント）
    /// </summary>
    public async void JoinHostRoom(string pin)
    {
        // ローディング画面を表示
        UIReferences.Instance.LoadingUI.SetActive(true);

        // 例外が発生する可能性のある処理
        try
        {
            bool success = false;

            StartGameResult result = default;

            for (int i = 0; i < 5; i++)
            {
                SetupNetworkRunner();

                result = await networkRunner.StartGame(
                    new StartGameArgs()
                    {
                        GameMode = GameMode.Client,

                        SessionName = pin.Trim(),

                        EnableClientSessionCreation = false,

                        SceneManager =
                            networkRunnerObject
                            .AddComponent<NetworkSceneManagerDefault>()
                    });

                // 接続成功
                if (result.Ok)
                {
                    success = true;

                    break;
                }

                // Runner終了
                await networkRunner.Shutdown();

                // Runner削除
                if (networkRunner != null &&
                    networkRunner.gameObject != null)
                {
                    Destroy(networkRunner.gameObject);
                }

                networkRunner = null;

                Debug.Log("3秒待機");

                await Task.Delay(3000);
            }


            // 接続成功時
            if (success)
            {
                UIReferences.Instance.LoadingUI.SetActive(false);
                UIReferences.Instance.LobbyUI.SetActive(true);

                Debug.Log("ゲスト側接続完了");
            }
            else
            {
                UIReferences.Instance.TitleUI.SetActive(true);
                UIReferences.Instance.LoadingUI.SetActive(false);
                Debug.LogError("接続失敗");

                if (networkRunner != null)
                {
                    await networkRunner.Shutdown();

                    if (networkRunner.gameObject != null)
                    {
                        Destroy(networkRunner.gameObject);
                    }

                    networkRunner = null;
                }
            }
        }
        catch (Exception error)
        {
            UIReferences.Instance.TitleUI.SetActive(true);
            UIReferences.Instance.LoadingUI.SetActive(false);
            Debug.LogException(error);

            if (networkRunner != null)
            {
                await networkRunner.Shutdown();

                if (networkRunner.gameObject != null)
                {
                    Destroy(networkRunner.gameObject);
                }

                networkRunner = null;
            }
        }
    }

    /// <summary>
    /// NetworkRunner生成とコールバック登録を行う
    /// </summary>
    private void SetupNetworkRunner()
    {
        Debug.Log(
       "作成前 Runner数 : " +
       FindObjectsByType<NetworkRunner>(
           FindObjectsSortMode.None).Length);

        // NetworkRunner用オブジェクトを作成
        networkRunnerObject = new GameObject("NetworkRunner");
        // シーン移動してもRunnerが破棄されないようにする
        DontDestroyOnLoad(networkRunnerObject);

        // Fusionのネットワーク処理本体となるNetworkRunnerを生成
        networkRunner = networkRunnerObject.AddComponent<NetworkRunner>();

        // ローカルプレイヤーの入力をFusionへ送信できるようにする
        networkRunner.ProvideInput = true;

        // UIイベント管理スクリプトをRunnerObjectへ追加
        var lobbyUI = networkRunnerObject.AddComponent<NetworkLobbyUI>();
        // Fusion callbackへ登録
        networkRunner.AddCallbacks(lobbyUI);
        networkLobbyUI = lobbyUI;

        // PlayerSpawnerをRunnerObjectへ追加
        var playerSpawner = networkRunnerObject.AddComponent<PlayerSpawner>();
        // Fusion callbackへ登録
        networkRunner.AddCallbacks(playerSpawner);

        // ItemSpawnerをRunnerObjectへ追加
        var itemSpawner = networkRunnerObject.AddComponent<ItemSpawner>();
        // Fusion callbackへ登録
        networkRunner.AddCallbacks(itemSpawner);

        // PlayerInputGetterをRunnerObjectへ追加
        var playerInputGetter = networkRunnerObject.AddComponent<PlayerInputGetter>();
        // Fusion callbackへ登録
        networkRunner.AddCallbacks(playerInputGetter);

        // MatchingPlayerObjectSpawnerをRunnerObjectへ追加
        var matchingPlayerObjectSpawner = networkRunnerObject.AddComponent<MatchingPlayerObjectSpawner>();
        // Fusion callbackへ登録
        networkRunner.AddCallbacks(matchingPlayerObjectSpawner);

        // INetworkRunnerCallbacks を受け取るため自分自身を登録
        networkRunner.AddCallbacks(this);

        Debug.Log(
      "作成後 Runner数 : " +
      FindObjectsByType<NetworkRunner>(
          FindObjectsSortMode.None).Length);
    }


    /// <summary>
    /// コールバックを登録する
    /// </summary>
    public void RegisterCallbacks(PlayerInputGetter inputGetter)
    {
        networkRunner.AddCallbacks(inputGetter);
    }


    /// <summary>
    /// Host切断時やゲーム終了時に
    /// NetworkRunnerを終了・破棄し、
    /// タイトルシーンへ戻る処理。
    /// 将来的にはHostMigration対応予定。
    /// </summary>
    public async void ShutdownRunner()
    {
        if (networkRunner != null && networkRunner.IsServer)
        {
            // ホストを最後に切断させるため
            await Task.Delay(3000);
        }

        if (networkRunner == null)
        {
            return;
        }

        // GameObject退避
        GameObject runnerObject = networkRunner.gameObject;

        // Shutdown
        await networkRunner.Shutdown();

        // 破棄
        if (runnerObject != null)
        {
            Destroy(runnerObject);
        }

        networkRunner = null;

        networkRunnerObject = null;

        Debug.Log("Runner破棄完了");

        SceneManager.LoadScene("MainTitleScenes");
    }


    #region Player
    /// <summary>
    /// 新しいプレイヤーがセッションに参加した時に自動で呼ばれるコールバック。
    /// プレイヤー用キャラクターの生成や、参加時の初期設定などを行う場所。
    /// </summary>
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("ルーム生成完全完了");
    }

    /// <summary>
    /// プレイヤーがセッションから離脱した時に自動で呼ばれるコールバック。
    /// プレイヤーが操作していたネットワークオブジェクトの削除処理や、
    /// 人数管理・UI更新・プレイヤーリスト整理などを行う。
    /// </summary>
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
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
    /// 全クライアントのシーンロード完了時に呼ばれる。
    /// ロード完了後の初期化処理やスポーン処理を開始するためのコールバック。
    /// </summary>
    public void OnSceneLoadDone(NetworkRunner runner) { }

    /// <summary>
    /// ネットワークシーンのロード開始時に呼ばれる。
    /// ローディング画面の表示など、遷移中の準備処理を行う。
    /// </summary>
    public void OnSceneLoadStart(NetworkRunner runner) { }
    #endregion

    #region Host Migration
    /// <summary>
    /// ホストモードでホストが切断された時、
    /// 新しいホストに自動で引き継がれる処理を行うためのコールバック。
    /// ゲームの継続・オブジェクトの再割り当てなどを行う。
    /// </summary>
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        // ホスト変更処理
        HostMigrationManager.Instance.HandleHostMigration();
    }
    #endregion
}
