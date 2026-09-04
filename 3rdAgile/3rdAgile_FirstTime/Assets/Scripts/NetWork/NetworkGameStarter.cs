// -----------------------------------------------------------------------------------
// ルーム作成、参加、シーン管理、ゲーム開始の司令塔
// NetworkGameStarter.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkGameStarter : MonoBehaviour, INetworkRunnerCallbacks
{
    // 最大参加人数
    private readonly int MAX_PLAYER_COUNT = 4;

    // ルーム接続失敗時の再接続回数
    private readonly int MAX_RECONNECT_COUNT = 5;

    // ルーム暗証番号の最大値
    private readonly int PIN_MAX_VALUE = 9999;
    // ルーム暗証番号の最小値
    private readonly int PIN_MIN_VALUE = 1000;

    // 再接続時の待機時間
    private readonly int RECONNECT_WAIT_TIME_MS = 3000;
    // ホスト切断時の待機時間
    private readonly int HOST_DISCONNECTED_WAIT_TIME_MS = 3000;

    // 接続切断時のメッセージ表示時間
    private readonly float DISCONNECTED_MESSAAGE_DISPLAY_TIME = 3.0f;

    // NetworkRunner用オブジェクトの名前
    private readonly string NETWORK_RUNNER_OBJECT_NAME = "NetworkRunner";

    // タイトルシーンの名前
    private readonly string TITLE_SCENE_NAME = "MainTitleScenes";


    // NetworkRunner をアタッチするためのオブジェクト
    private GameObject networkRunnerObject = null;

    // ランダムな暗証番号を生成するためのRandomクラス
    private System.Random random = new System.Random();


    // ルーム情報などを入れるNetworkRunner
    public NetworkRunner networkRunner = null;

    // ロビーUIのコールバックを受け取るための変数
    public NetworkLobbyUI networkLobbyUI = null;

    // ランキング時に使用するチーム名
    public string TeamName { get; private set; } = "";

    // ルーム暗証番号
    public string PIN { get; private set; } = "";

    //　インスタンスを保持するためのシングルトン
    public static NetworkGameStarter Instance { get; private set; } = null;

    /// <summary>
    /// すでに存在する場合は破棄し、シングルトンとして保持する
    /// </summary>
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
            // 仮の部屋番号を生成
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

                // 正式に部屋番号を記録する
                PIN = pin;

                break;
            }
        }
    }

    /// <summary>
    /// ランダムな4桁の暗証番号を生成する
    /// </summary>
    private string RandomPIN()
    {
        // 4桁のランダムな数字を生成
        return random.Next(PIN_MIN_VALUE, PIN_MAX_VALUE).ToString();
    }


    /// <summary>
    /// ルームへ参加する処理（クライアント）
    /// </summary>
    public async void JoinHostRoom(string pin)
    {
        // ローディング画面を表示
        UIReferences.Instance.LoadingUI.SetActive(true);

        try
        {
            bool success = false;

            StartGameResult result = default;

            // 5回まで接続を試みる
            for (int i = 0; i < MAX_RECONNECT_COUNT; i++)
            {
                // NetworkRunner生成とコールバック登録
                SetupNetworkRunner();

                // FusionのPhoton Cloudへ接続し、作成されたルームへ参加する
                result = await networkRunner.StartGame(new StartGameArgs()
                {
                    GameMode = GameMode.Client,

                    SessionName = pin.Trim(),

                    EnableClientSessionCreation = false,

                    SceneManager = networkRunnerObject.AddComponent<NetworkSceneManagerDefault>()
                });

                // 接続成功
                if (result.Ok)
                {
                    success = true;

                    break;
                }

                // NetworkRunnerを終了
                await networkRunner.Shutdown();

                // NetworkRunnerを削除
                if (networkRunner != null && networkRunner.gameObject != null)
                {
                    Destroy(networkRunner.gameObject);
                }

                networkRunner = null;

                // 3秒待機
                await Task.Delay(RECONNECT_WAIT_TIME_MS);
            }


            // 接続成功時
            if (success)
            {
                // UIを切り替える
                UIReferences.Instance.LoadingUI.SetActive(false);
                UIReferences.Instance.LobbyUI.SetActive(true);
            }
            else　// 失敗時
            {
                // 接続失敗時の処理を実行
                await ConnectionFailed();
            }
        }
        catch (Exception error) // 例外発生時
        {
            // エラー内容をログに出力
            Debug.LogError(error);

            // 接続失敗時の処理を実行
            await ConnectionFailed();
        }
    }

    /// <summary>
    /// 接続失敗時の処理
    /// </summary>
    private async Task ConnectionFailed()
    {
        // ロビーUIを非表示にして、タイトルUIを表示
        UIReferences.Instance.TitleUI.SetActive(true);
        UIReferences.Instance.LoadingUI.SetActive(false);

        //接続失敗を知らせるメッセージを表示
        TitleUI titleUI = UIReferences.Instance.TitleUI.GetComponent<TitleUI>();
        StartCoroutine(titleUI.MessageLogDisplay());

        // NetworkRunnerを終了・破棄
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

    /// <summary>
    /// NetworkRunner生成とコールバック登録を行う
    /// </summary>
    private void SetupNetworkRunner()
    {
        // NetworkRunner用オブジェクトを作成
        networkRunnerObject = new GameObject(NETWORK_RUNNER_OBJECT_NAME);
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

        var stageSpawner = networkRunnerObject.AddComponent<StageSpawner>();
        networkRunner.AddCallbacks(stageSpawner);

        // INetworkRunnerCallbacks を受け取るため自分自身を登録
        networkRunner.AddCallbacks(this);
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
        Debug.Log($"ShutdownRunner開始 IsServer:{networkRunner?.IsServer}");
        if (networkRunner != null && networkRunner.IsServer)
        {
            // ホストを最後に切断させるため3秒待機
            await Task.Delay(HOST_DISCONNECTED_WAIT_TIME_MS);
        }

        if (networkRunner == null)
        {
            return;
        }

        // Shutdown後もDestroyするため退避
        GameObject runnerObject = networkRunner.gameObject;

        // NetworkRunnerを終了
        await networkRunner.Shutdown();

        ClearRoomData(runnerObject);
    }


    #region Player
    /// <summary>
    /// 新しいプレイヤーがセッションに参加した時に自動で呼ばれるコールバック。
    /// プレイヤー用キャラクターの生成や、参加時の初期設定などを行う場所。
    /// </summary>
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }

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
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        // ホスト側が切断された場合
        if (!runner.IsServer && shutdownReason == ShutdownReason.DisconnectedByPluginLogic)
        {
            // 切断メッセージ表示
            networkLobbyUI.DisplayMessageDisconnected();
            // Shutdown後もDestroyするため退避
            GameObject runnerObject = networkRunner.gameObject;
            ClearRoomData(runnerObject);

            StartCoroutine(BackToTheTitle());
        }
    }

    /// <summary>
    /// 所持しているNetworkRunnerを破棄し、部屋情報をクリアする
    /// </summary>
    private void ClearRoomData(GameObject runnerObject)
    {
        // NetworkRunnerを破棄
        if (runnerObject != null)
        {
            Destroy(runnerObject);
        }

        // 部屋名など、自分で保持している情報をクリア
        networkRunner = null;
        networkRunnerObject = null;
    }

    /// <summary>
    /// 数秒後にタイトルシーンへ戻るコルーチン
    /// </summary>
    private IEnumerator BackToTheTitle()
    {
        yield return new WaitForSecondsRealtime(DISCONNECTED_MESSAAGE_DISPLAY_TIME);

        // タイトルシーンへ遷移
        SceneManager.LoadScene(TITLE_SCENE_NAME);
    }

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
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    #endregion
}
