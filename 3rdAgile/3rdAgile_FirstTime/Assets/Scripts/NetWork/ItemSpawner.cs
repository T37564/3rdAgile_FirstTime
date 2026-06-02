using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NetworkBehaviourにすると壊れてしまいます。
/// </summary>
public class ItemSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner networkRunner;

    // 最初にスポーンするアイテムの数
    private readonly int ITEM_FIRST_SPAWNED_COUNT = 3;

    // スクリプトを取得
    private ItemObjectPlace itemObjectPlace = null;

    //アイテムがスポーンした数
    private int itemCount = 0;

    // ゲームタイマークラス
    [SerializeField] private GameTimer gameTimer = null;


    public void RegisterGameTimer(GameTimer timer)
    {
        gameTimer = timer;
    }

    private void Start()
    {
        var runner = FindFirstObjectByType<NetworkRunner>();

        if (runner != null)
        {
            runner.AddCallbacks(this);
            Debug.Log("Callbacks登録した");
        }
        else
        {
            Debug.LogError("Runnerが見つからない");
        }

        // タイマーがある時は、ゲームタイマーを取得して登録する
        if (gameTimer != null)
        {
            gameTimer = FindAnyObjectByType<GameTimer>();
            gameTimer.GetComponent<GameTimer>();
        }
    }

    /// <summary>
    /// フェーズが変わるたびに設定したフェーズのアイテムをスポーンするためのメソッド
    /// </summary>
    private void OnPhaseChanged(GamePhase phase,NetworkRunner runner)
    {
        SpawnItems(phase,runner);
    }


    /// <summary>
    /// 指定したフェーズごとに生成するアイテムの数、座標、アイテムの種類を
    /// ランダムに決めて生成するメソッド
    /// </summary>
    private void SpawnItems(GamePhase phase,NetworkRunner runner)
    {
        // フェーズごとに出現させるアイテムの数を取得
        int spawnCount = itemObjectPlace.GetSpawnCount(phase);

        // フェーズごとに出現させるアイテムの数だけループする
        for (int i = 0; i < spawnCount; i++)
        {
            // GetRandomPrefabByPhaseを使って、フェーズに応じたアイテムのプレハブをランダムに取得する
            var prefab = itemObjectPlace.GetRandomPrefabByPhase(phase);

            // もしプレハブがnullだったら、次のループに行く
            if (prefab == null) continue;

            // 生成する際のランダムな位置を取得
            Vector3 randomPosition = itemObjectPlace.GetRandomPosition();

            // ネットワークを使ったアイテム生成
            runner.Spawn(prefab, randomPosition, Quaternion.identity,
                null,
                (runner, spawnedObject) =>
                {
                    SetupItem(spawnedObject);

                    RegenerationCallOut regenerationCallOut = spawnedObject.GetComponent<RegenerationCallOut>();

                    // アイテムにRegenerationCallOutがついていたら、再配置要求イベントを登録する
                    if (regenerationCallOut != null)
                    {
                        //Debug.Log("再配置を要求");
                        regenerationCallOut.OnNeedRegenerate += HandleNeedRegenerate;
                    }
                });
        }
    }

    // アイテムが再配置を要求したときに行われる処理
    /// <summary>
    /// 追加でアイテムを再配置するためのメソッド
    /// </summary>
    private void HandleNeedRegenerate(RegenerationCallOut regen)
    {
        if (!regen.Object.HasStateAuthority)
            return;

        // 再配置要求を出したアイテムのNetworkObjectを取得
        //NetworkObject obj = regen.Object;

        // 生成する際のランダムな位置を取得
        Vector3 newPos = itemObjectPlace.GetRandomPosition();
        //Debug.Log(itemObjectPlace);

        Rigidbody rigidbody = regen.GetComponent<Rigidbody>();

        // rigidbodyを使って違う座標に再配置する
        if (rigidbody != null)
        {
            rigidbody.linearVelocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;

            rigidbody.position = newPos;
        }

        //regen.Object.transform.position = newPos;
        //regen.RegeneratePosition(newPos);
        //regen.gameObject.SetActive(false);

        // アイテムの位置を新しいランダムな位置に変更する
        //obj.transform.position = newPos;
        //obj.gameObject.SetActive(false);
        
        // 再配置要求フラグをリセット
        regen.isGenerateRequest = false;
    }

    /// <summary>
    /// 全クライアントのシーンロード完了時に呼ばれる。
    /// ロード完了後の初期化処理やスポーン処理を開始するためのコールバック。
    /// </summary>
    public void OnSceneLoadDone(NetworkRunner runner)
    {
        // 管理者だけ実行
        if (!runner.IsServer) return;

        // タイマーがある時は、ゲームタイマーを取得して登録する
       GameTimer timer = FindAnyObjectByType<GameTimer>();
        RegisterGameTimer(timer);

        // フェーズが変わるたびにアイテムをスポーンするようにイベント登録
        gameTimer.OnPhaseChanged += (phase) => OnPhaseChanged(phase, runner);

        // スポーン位置の設定
        // スポーンする位置をいれたオブジェクトを取得
        GameObject spawnPoint = GameObject.Find("ItemObjectPlace");

        // スクリプトを取得
        itemObjectPlace = spawnPoint.GetComponent<ItemObjectPlace>();

        if (itemObjectPlace == null)
        {
            Debug.LogError("ItemObjectPlace script が付いてない！");
            return;
        }

        // 現在のフェーズ取得
        GamePhase phase = gameTimer.CurrentPhase;


        //生成する数だけpositionを作ってください
        int spawnCount = itemObjectPlace.GetSpawnCount(phase);

        // 出現させるアイテムの数だけループする
        // ランダムに決められた座標に、ランダムに決められたアイテムを生成する
        for (int i = 0; i < spawnCount; i++)
        {
            itemCount++;

            // 生成する際のランダムな位置を取得
            Vector3 generatePosition = itemObjectPlace.GetRandomPosition();

            // 生成するオブジェクトを取得
            NetworkObject prefab = itemObjectPlace.GetRandomPrefabByPhase(phase);

            if (prefab == null) continue;

            // ネットワークを使ったアイテム生成
            runner.Spawn(prefab, generatePosition, Quaternion.identity, null, (runner, obj) =>
            {
                SetupItem(obj);

                RegenerationCallOut regenerationCallOut = obj.GetComponent<RegenerationCallOut>();

                // アイテムにRegenerationCallOutがついていたら、再配置要求イベントを登録する
                if (regenerationCallOut != null)
                {
                    //Debug.Log("再配置を要求");
                    regenerationCallOut.OnNeedRegenerate += HandleNeedRegenerate;
                }
            });
        }
    }

    /// <summary>
    /// アイテムの情報をランダムに決めるメソッド
    /// </summary>
    private void SetupItem(NetworkObject obj)
    {
        // オブジェクトにあるアイテムのデータを取得する
        ItemDataStorage storage = obj.GetComponent<ItemDataStorage>();

        if(storage == null) return;

        if (!storage.useRandomData) return;
        
        //ランダムに決めたアイテムの情報を生成したアイテムに代入する
        SampleMasterData data = itemObjectPlace.GetRomdomItemData(obj);
        
        if (data == null) return;
        
        Debug.Log(obj.name + " に " + data.name + " を設定");

        // アイテムの情報をセットする
        storage.SetData(data);
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
