// -----------------------------------------------------------------------------------
// ルーム作成、参加、シーン管理、ゲーム開始の司令塔。
// NetworkGameStarter.cs
// Create.by TakahashiSaya
//-----------------------------------------------------------------------------------
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkGameStarter : MonoBehaviour, INetworkRunnerCallbacks
{
    // ルーム情報などを入れるNetworkRunner
    public NetworkRunner networkRunner = null;

    // NetworkRunner をアタッチするためのオブジェクト
    private GameObject networkRunnerObject = null;

    /// <summary>
    /// マッチする処理　ホストバージョン
    /// async をつけているので、ネットワーク処理の完了を待ってもゲームが止まらない
    /// </summary>
    public async void CreateHostRoom(string sessionName)
    {
        // ローディング画面を表示
        TitleCanvasDisplaySettings.Instance.nowLoadingImage.SetActive(true);

        // Runner専用オブジェクトを作成
        networkRunnerObject = new GameObject("NetworkRunnerHost");
        // シーン移動してもRunnerが破棄されないようにする
        DontDestroyOnLoad(networkRunnerObject);

        // Fusionのネットワーク処理本体となるNetworkRunnerを生成
        networkRunner = networkRunnerObject.AddComponent<NetworkRunner>();

        // ローカルプレイヤーの入力をFusionへ送信できるようにする
        networkRunner.ProvideInput = true;

        // UIイベント管理スクリプトを Runner オブジェクトに付ける
        var uiChange = networkRunnerObject.AddComponent<NetworkUIChange>();
        // Fusion callbackへ登録
        networkRunner.AddCallbacks(uiChange);

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
        var matchingPlayerObjectSpawner = networkRunner.AddComponent<MatchingPlayerObjectSpawner>();
        // Fusion callbackへ登録
        networkRunner.AddCallbacks(matchingPlayerObjectSpawner);

        // INetworkRunnerCallbacks を受け取るため自分自身を登録
        networkRunner.AddCallbacks(this);

        // Photon Cloudへ接続し、Hostとしてルームを作成する
        await networkRunner.StartGame(new StartGameArgs()
        {
            // Hostモードでルームを作成する
            GameMode = GameMode.Host,
            // ルーム名
            SessionName = sessionName,
            // 最大マッチング人数
            PlayerCount = 4,
            // Fusionのシーン同期・シーン移動管理に使用
            SceneManager = networkRunnerObject.AddComponent<NetworkSceneManagerDefault>()
        });

        // タイトルUIを閉じ、ロビーUIを表示する
        TitleCanvasDisplaySettings.Instance.titleCanvas.SetActive(false);
        TitleCanvasDisplaySettings.Instance.lobbyCanvas.SetActive(true);
        TitleCanvasDisplaySettings.Instance.gameStartButton.SetActive(true);

        Debug.Log("ホスト側接続完了");
    }


    /// <summary>
    /// マッチする処理　クライアントバージョン
    /// </summary>
    public async void JoinHostRoom(string sessionName)
    {
        // ローディング画面を表示
        TitleCanvasDisplaySettings.Instance.nowLoadingImage.SetActive(true);

        // 危険処理を試す
        try
        {
            // Runner専用オブジェクトを作成
            networkRunnerObject = new GameObject("NetworkRunnerClient");
            // シーン移動してもRunnerが破棄されないようにする
            DontDestroyOnLoad(networkRunnerObject);

            // Fusionのネットワーク処理本体となるNetworkRunnerを生成
            networkRunner = networkRunnerObject.AddComponent<NetworkRunner>();

            // ローカルプレイヤーの入力をFusionへ送信できるようにする
            networkRunner.ProvideInput = true;

            // UIイベント管理スクリプトを Runner オブジェクトに付ける
            var uiChange = networkRunnerObject.AddComponent<NetworkUIChange>();
            // Fusion callbackへ登録
            networkRunner.AddCallbacks(uiChange);

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
            var matchingPlayerObjectSpawner = networkRunner.AddComponent<MatchingPlayerObjectSpawner>();
            // Fusion callbackへ登録
            networkRunner.AddCallbacks(matchingPlayerObjectSpawner);

            // INetworkRunnerCallbacks を受け取るため自分自身を登録
            networkRunner.AddCallbacks(this);

            // Photon Cloudへ接続し、Clientとしてルームに入る
            var result = await networkRunner.StartGame(new StartGameArgs()
            {
                // Clientとしてルームに入る
                GameMode = GameMode.Client,
                // セッションルーム名
                SessionName = sessionName,
                // 指定したルームが存在しない場合、自動作成しない
                EnableClientSessionCreation = false,
                // Fusionのシーン同期・シーン移動管理に使用
                SceneManager = networkRunnerObject.AddComponent<NetworkSceneManagerDefault>()
            });

            if (result.Ok)
            {
                // 表示するキャンバスの変更
                TitleCanvasDisplaySettings.Instance.titleCanvas.SetActive(false);
                TitleCanvasDisplaySettings.Instance.lobbyCanvas.SetActive(true);

                Debug.Log("ゲスト側接続完了");
            }
            else
            {
                if (networkRunner != null)
                {
                    // Runnerを終了
                    await networkRunner.Shutdown();

                    // Runnerを破棄
                    Destroy(networkRunner.gameObject);

                    networkRunner = null;
                }

                // エラー表示
                CoroutineRunner.Instance.StartCoroutine(
                    TitleCanvasDisplaySettings.Instance
                    .ErrorTextDisplay(true, "The room does not exist", 1));

                // UIを戻す
                TitleCanvasDisplaySettings.Instance.ResetTitleUI();
                TitleCanvasDisplaySettings.Instance.ResetLobbyUI();
            }
        }
        catch (Exception error)// エラー時処理
        {
            Debug.LogException(error);

            if (networkRunner != null)
            {
                // Runnerを終了
                await networkRunner.Shutdown();

                // Runnerを破棄
                Destroy(networkRunner.gameObject);

                networkRunner = null;
            }

            // エラーが出たことを画面に表示
            CoroutineRunner.Instance.StartCoroutine(TitleCanvasDisplaySettings.Instance.ErrorTextDisplay(true, "An unexpected error has occurred. Please try again.", 2));

            // UIの状態を戻す処理
            TitleCanvasDisplaySettings.Instance.ResetTitleUI();
            TitleCanvasDisplaySettings.Instance.ResetLobbyUI();
        }
        finally // 最後に必ず実行
        {
            // ロード画面を消す
            TitleCanvasDisplaySettings.Instance.nowLoadingImage.SetActive(false);
        }
    }


    /// <summary>
    /// touroku suru callback wo kettei suru basho
    /// </summary>
    /// <param name="inputGetter"></param>
    public void RegisterCallbacks(PlayerInputGetter inputGetter)
    {
        networkRunner.AddCallbacks(inputGetter);
    }


    /// <summary>
    /// NetworkRunner がシャットダウンした時に呼ばれるコールバック。
    /// セッション終了やエラー発生、手動による Shutdown() 呼び出しなどで発生。
    /// ネットワーク終了時の後片付け（UI戻し、オブジェクト破棄、状態リセットなど）を行う。
    /// </summary>
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        // DontDestroyOnLoadされたRunnerを破棄する
        if (runner != null)
        {
            Destroy(runner.gameObject);
        }

        // マッチング画面へ戻る
        SceneManager.LoadScene("MatchingTestScenes");
    }

    /// <summary>
    /// 現在はHost切断時にタイトルへ戻している。
    /// 将来的にはHostMigrationでゲーム継続予定。
    /// </summary>
    public async void ReturnToTitle()
    {
        if (networkRunner != null)
        {
            // セッション終了
            await networkRunner.Shutdown();

            networkRunner = null;
        }
    }

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
