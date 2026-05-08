#nullable enable

using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;

public class StageSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    #region ステージパス

    private readonly string[] CORRIDOR_PATHS =
    {
        "Stages/Corridor1",
        "Stages/Corridor2",
        "Stages/Corridor3"
    };

    private readonly string AISLE_PATH = "Stages/Aisle";
    private readonly string ROOM_PATH = "Stages/Room";
    private readonly string DEAD_END_PATH = "Stages/DeadEnd";

    #endregion

    [Header("--- 部屋生成設定 ---")]
    [SerializeField] private int maxRoomCount = 10;

    /// <summary>
    /// 現在生成済みの部屋数
    /// </summary>
    private int roomCreateCount = 0;

    /// <summary>
    /// シーンロード完了時
    /// </summary>
    public void OnSceneLoadDone(NetworkRunner runner)
    {
        if (!runner.IsServer) return;

        // 最初の廊下生成
        NetworkObject? corridorObj = StageInstantiate(
            GetRandomCorridorPath(),
            runner
        );

        if (corridorObj == null)
        {
            Debug.LogError("最初の廊下生成に失敗");
            return;
        }

        // Connector取得
        CorridorSpawnPoints? spawnPoints =
            corridorObj.GetComponent<CorridorSpawnPoints>();

        if (spawnPoints == null)
        {
            Debug.LogError("CorridorSpawnPoints が見つからない");
            return;
        }

        // 各接続点から生成
        foreach (Transform point in spawnPoints.RoomSpawnPoints)
        {
            CreateNextPart(point, runner);
        }
    }

    /// <summary>
    /// 次のパーツ生成
    /// </summary>
    private void CreateNextPart(
        Transform spawnPoint,
        NetworkRunner runner)
    {
        // 部屋数が上限なら行き止まり生成
        if (roomCreateCount >= maxRoomCount)
        {
            StageInstantiate(
                DEAD_END_PATH,
                runner,
                spawnPoint.position,
                spawnPoint.rotation
            );

            return;
        }

        // 生成するか抽選
        bool createFlag = BoolRandomUtility.RandomBool();

        if (!createFlag)
        {
            StageInstantiate(
                DEAD_END_PATH,
                runner,
                spawnPoint.position,
                spawnPoint.rotation
            );

            return;
        }

        // 部屋か通路か抽選
        bool createRoomFlag = BoolRandomUtility.RandomBool();

        switch (createRoomFlag)
        {
            // 部屋生成
            case true:

                StageInstantiate(
                    ROOM_PATH,
                    runner,
                    spawnPoint.position,
                    spawnPoint.rotation
                );

                roomCreateCount++;

                break;

            // 通路生成
            case false:

                NetworkObject? aisleObj = StageInstantiate(
                    AISLE_PATH,
                    runner,
                    spawnPoint.position,
                    spawnPoint.rotation
                );

                if (aisleObj == null)
                {
                    return;
                }

                CorridorSpawnPoints? nextSpawnPoints =
                    aisleObj.GetComponent<CorridorSpawnPoints>();

                if (nextSpawnPoints == null)
                {
                    return;
                }

                // 通路の接続点からさらに生成
                foreach (Transform nextPoint in nextSpawnPoints.RoomSpawnPoints)
                {
                    CreateNextPart(nextPoint, runner);
                }

                break;
        }
    }

    /// <summary>
    /// ランダムな廊下パス取得
    /// </summary>
    private string GetRandomCorridorPath()
    {
        int index =
            UnityEngine.Random.Range(0, CORRIDOR_PATHS.Length);

        return CORRIDOR_PATHS[index];
    }

    /// <summary>
    /// ステージパーツ生成
    /// </summary>
    private NetworkObject? StageInstantiate(
        string stagePath,
        NetworkRunner runner,
        Vector3? position = null,
        Quaternion? rotation = null)
    {
        GameObject? prefabObj =
            Resources.Load<GameObject>(stagePath);

        if (prefabObj == null)
        {
            Debug.LogError(
                $"Prefab が見つからない : {stagePath}");

            return null;
        }

        if (!prefabObj.TryGetComponent(
                out NetworkObject networkObject))
        {
            Debug.LogError(
                $"NetworkObject が付いてない : {stagePath}");

            return null;
        }

        NetworkObject obj = runner.Spawn(
            networkObject,
            position ?? Vector3.zero,
            rotation ?? Quaternion.identity
        );

        return obj;
    }

    #region 未使用コールバック

    public void OnInput(NetworkRunner runner, NetworkInput input) { }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    public void OnConnectedToServer(NetworkRunner runner) { }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

    public void OnSceneLoadStart(NetworkRunner runner) { }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    #endregion
}


//# nullable enable
//using Fusion;
//using Fusion.Sockets;
//using System;
//using System.Collections.Generic;
//using UnityEngine;

//public class StageSpawner : MonoBehaviour, INetworkRunnerCallbacks
//{
//    private readonly string[] CORRIDOR_PATH = { "Stages/Corridor1", "Stages/Corridor2", "Stages/Corridor3" };
//    private readonly string AISLE_PATH = "Stages/Aisle";
//    private readonly string ROOM_PATH = "Stages/Room";

//    [Header("--- 部屋の生成に関する設定 ---")]
//    [Header("生成する部屋の数")]
//    [SerializeField] private int roomCreateCount = 0;

//    /// <summary>
//    /// シーンロードが完了したときに呼ばれる。
//    /// ロード後の初期化処理を書く。
//    /// </summary>
//    public void OnSceneLoadDone(NetworkRunner runner)
//    {
//        if (!runner.IsServer) return;

//        var spawnPoints = StageInstantiate(
//                            GetRandomStagePath(), 
//                            runner, 
//                            Vector3.zero, 
//                            Quaternion.identity)?.GetComponent<CorridorSpawnPoints>();
//        if (spawnPoints == null)
//        {
//            Debug.LogError("CorridorSpawnPoints が見つからないわよ！");
//            return;
//        }

//        // 部屋の生成処理
//        // まず廊下からつながる部屋または通路を生成するか抽選する
//        int isRoomCreateComplete = 0;
//        while (isRoomCreateComplete < spawnPoints.RoomSpawnPoints.Length)
//        {
//            // 廊下から生成するかをランダムに決める
//            bool createFlag = BoolRandomUtility.RandomBool();
//            // createFlagがtrueのときは部屋または通路を生成する、falseのときは生成しない
//            switch (createFlag)
//            {
//                // falseの時は生成しない
//                case false:
//                    break;
//                // trueのときは部屋または通路を生成する
//                case true:
//                    bool createRoomOrAisleFlag = BoolRandomUtility.RandomBool();
//                    // falseのときは部屋を生成する、trueのときは通路を生成する
//                    switch (createRoomOrAisleFlag)
//                    {
//                        case false:
//                            StageInstantiate(
//                                ROOM_PATH, 
//                                runner, 
//                                spawnPoints.RoomSpawnPoints[isRoomCreateComplete].position,
//                                Quaternion.identity);
//                            roomCreateCount++;
//                            break;
//                        case true:
//                            StageInstantiate(
//                                AISLE_PATH, 
//                                runner, 
//                                spawnPoints.RoomSpawnPoints[isRoomCreateComplete].position, 
//                                Quaternion.identity);
//                            break;
//                    }
//                    break;
//            }
//            isRoomCreateComplete++;
//        }
//    }

//    /// <summary>
//    /// ステージをランダムに選んでロードするための関数。
//    /// </summary>
//    private string GetRandomStagePath()
//    {
//        int index = UnityEngine.Random.Range(0, CORRIDOR_PATH.Length);
//        return CORRIDOR_PATH[index];
//    }

//    /// <summary>
//    /// ステージの各パーツを生成するための関数。
//    /// </summary>
//    private NetworkObject? StageInstantiate(
//        string stagePath,
//        NetworkRunner runner,
//        Vector3 position,
//        Quaternion rotation)
//    {
//        GameObject prefabObj = Resources.Load<GameObject>(stagePath);
//        if (prefabObj == null)
//        {
//            Debug.LogError("ステージPrefab が見つからないわよ！");
//            return null;
//        }
//        if (!prefabObj.TryGetComponent(out NetworkObject networkObject))
//        {
//            Debug.LogError("ステージPrefab が見つからないわよ！");
//            return null;
//        }
//        NetworkObject obj = runner.Spawn(
//            networkObject,
//            position,
//            rotation
//        );
//        return obj;
//    }


//    #region このクラスでは使わないコールバック（空実装）
//    /// <summary>
//    /// 各Tickごとに入力データをRunnerへ渡すために呼ばれる。
//    /// </summary>
//    public void OnInput(NetworkRunner runner, NetworkInput input) { }

//    /// <summary>
//    /// プレイヤーがセッションに参加したときに呼ばれる。
//    /// ホストでスポーン処理を書くことが多い。
//    /// </summary>
//    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }

//    /// <summary>
//    /// プレイヤーがセッションから退出したときに呼ばれる。
//    /// プレイヤーオブジェクトの削除処理などを書く。
//    /// </summary>
//    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

//    /// <summary>
//    /// Runnerがシャットダウンされたときに呼ばれる。
//    /// セッション終了や強制切断時の後処理に使う。
//    /// </summary>
//    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }

//    /// <summary>
//    /// サーバーとの接続が切断されたときに呼ばれる。
//    /// 切断理由に応じたUI表示などを行う。
//    /// </summary>
//    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

//    /// <summary>
//    /// クライアントからの接続要求を受け取ったときに呼ばれる（Host側）。
//    /// 接続を許可するかどうかを決める処理を書く。
//    /// </summary>
//    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }

//    /// <summary>
//    /// 接続に失敗したときに呼ばれる（クライアント側）。
//    /// エラーメッセージ表示などに使用。
//    /// </summary>
//    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

//    /// <summary>
//    /// ユーザー定義のシミュレーションメッセージを受信したときに呼ばれる。
//    /// 独自メッセージ通信を使う場合に使用。
//    /// </summary>
//    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }


//    /// <summary>
//    /// 特定プレイヤーの入力が取得できなかったときに呼ばれる。
//    /// 入力補間やデフォルト入力を設定する用途。
//    /// </summary>
//    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

//    /// <summary>
//    /// サーバーへの接続が成功したときに呼ばれる。
//    /// 接続完了後の初期処理を書く。
//    /// </summary>
//    public void OnConnectedToServer(NetworkRunner runner) { }

//    /// <summary>
//    /// セッション一覧が更新されたときに呼ばれる。
//    /// ロビー画面の部屋リスト更新などに使用。
//    /// </summary>
//    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }


//    /// <summary>
//    /// シーンロードが開始されたときに呼ばれる。
//    /// ロード前の準備処理を書く。
//    /// </summary>
//    public void OnSceneLoadStart(NetworkRunner runner) { }

//    /// <summary>
//    /// カスタム認証のレスポンスを受け取ったときに呼ばれる。
//    /// 外部認証を使用している場合に利用。
//    /// </summary>
//    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

//    /// <summary>
//    /// ホストマイグレーションが発生したときに呼ばれる。
//    /// ホストが抜けた際の引き継ぎ処理を書く。
//    /// </summary>
//    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

//    /// <summary>
//    /// Reliable通信でデータを受信したときに呼ばれる。
//    /// 大きめのデータ送信などで使用。
//    /// </summary>
//    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }

//    /// <summary>
//    /// Reliable通信の送受信進捗が更新されたときに呼ばれる。
//    /// ダウンロード進行表示などに使用。
//    /// </summary>
//    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
//    /// <summary>
//    /// AOI（Area of Interest）からNetworkObjectが外れたときに呼ばれる。
//    /// 対象プレイヤーにそのオブジェクトが見えなくなるタイミング。
//    /// </summary>
//    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

//    /// <summary>
//    /// AOI（Area of Interest）にNetworkObjectが入ったときに呼ばれる。
//    /// 対象プレイヤーにそのオブジェクトが見えるようになるタイミング。
//    /// </summary>
//    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
//    #endregion
//}

