#nullable enable

using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class StageSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    #region ステージパス

    private readonly string START_ROOM_PATH = "Stages/Start";
    private readonly string STRAIGHT_CORRIDOR_PATH = "Stages/StraightCorridor";
    private readonly string T_CORRIDOR_PATH = "Stages/TCorridor";
    private readonly string CROSS_CORRIDOR_PATH = "Stages/CrossCorridor";
    private readonly string ROOM_PATH = "Stages/Room";
    private readonly string GUARDIAN_ROOM_PATH = "Stages/GuardianRoom";
    private readonly string DEAD_END_PATH = "Stages/DeadEnd";

    #endregion

    [Header("--- 部屋生成設定 ---")]
    [SerializeField] private int maxRoomCount = 10;

    /// <summary>
    /// 現在生成済みの部屋数
    /// </summary>
    private int roomCreateCount = 0;

    private readonly Queue<OpenConnector> openConnectors = new();

    [SerializeField] private float cellSize = 5.0f;
    [SerializeField] private NetworkObject startRoomPrefab;
    [Header("--- 通路prefab ---")]
    [SerializeField] private NetworkObject straightCorridorPrefab;
    [SerializeField] private NetworkObject tCorridorPrefab;
    [SerializeField] private NetworkObject crossCorridorPrefab;
    [SerializeField] private NetworkObject normalRoomPrefab;
    [SerializeField] private NetworkObject guardianRoomPrefab;

    [SerializeField] private float normalRoomRate = 0.25f;
    [SerializeField] private float guardianRoomRate = 0.15f;
    [SerializeField] private int maxNormalRoomCount = 8;
    [SerializeField] private int maxGuardianRoomCount = 3;

    private int normalRoomCount = 0;
    private int guardianRoomCount = 0;

    [SerializeField] private int maxPartCount = 30;

    private int createPartCount = 0;

    private StageGrid? stageGrid;

    private void Awake()
    {
        startRoomPrefab = Resources.Load<NetworkObject>(START_ROOM_PATH);
        straightCorridorPrefab = Resources.Load<NetworkObject>(STRAIGHT_CORRIDOR_PATH);
        tCorridorPrefab = Resources.Load<NetworkObject>(T_CORRIDOR_PATH);
        crossCorridorPrefab = Resources.Load<NetworkObject>(CROSS_CORRIDOR_PATH);
        normalRoomPrefab = Resources.Load<NetworkObject>(ROOM_PATH);
        guardianRoomPrefab = Resources.Load<NetworkObject>(GUARDIAN_ROOM_PATH);
    }

    /// <summary>
    /// シーンロード完了時
    /// </summary>
    public void OnSceneLoadDone(NetworkRunner runner)
    {
        if (!runner.IsServer) return;

        Debug.Log("べろべろばー");

        stageGrid = new StageGrid(cellSize);

        CreateStartRoom(runner);

        CreateStageParts(runner);
    }

    private void CreateStartRoom(NetworkRunner runner)
    {
        if (stageGrid == null)
        {
            Debug.LogError("StageGridが初期化されていない");
            return;
        }

        Debug.Log("a---ho");
        Vector2Int startGrid = Vector2Int.zero;
        Vector3 startWorldPosition = stageGrid.GridToWorld(startGrid);

        NetworkObject startobj = runner.Spawn(
            startRoomPrefab,
            startWorldPosition,
            Quaternion.identity
        );

        stageGrid.Register(startGrid);

        StagePiece? stagePiece = startobj.GetComponentInChildren<StagePiece>();

        if (stagePiece == null)
        {
            Debug.LogError("StartRoomにStagePieceがついていない");
            return;
        }
        Debug.Log("huzakennna");
        foreach (StageConnector connector in stagePiece.Connectors)
        {
            openConnectors.Enqueue(
                new OpenConnector(
                    startGrid,
                    connector.Direction
                )
            );
            Debug.Log("majimuri");
        }
        Debug.Log("kietai");
    }

    private void CreateStageParts(NetworkRunner runner)
    {
        if (stageGrid == null)
        {
            Debug.LogError("StageGridが初期化されていない");
            return;
        }

        while (openConnectors.Count > 0)
        {
            if (createPartCount >= maxPartCount) break;

            OpenConnector openConnector = openConnectors.Dequeue();

            Vector2Int nextGrid =
                openConnector.GridPosition + openConnector.Direction.ToVector();

            if (stageGrid.IsUsed(nextGrid)) continue;

            NetworkObject corridorPrefab = GetRandomStagePrefab();

            Vector3 worldPosition = stageGrid.GridToWorld(nextGrid);

            Quaternion rotation =
                GetRotationFromDirection(openConnector.Direction);

            NetworkObject corridorObj = runner.Spawn(
                corridorPrefab,
                worldPosition,
                rotation
            );

            stageGrid.Register(nextGrid);
            createPartCount++;

            StagePiece? stagePiece = corridorObj.GetComponent<StagePiece>();

            if (stagePiece == null)
            {
                Debug.LogError("通路にStagePieceがついていない");
                continue;
            }

            if (corridorPrefab == normalRoomPrefab || corridorPrefab == guardianRoomPrefab) continue;

            foreach (StageConnector connector in stagePiece.Connectors)
            {
                GridDirection worldDirection =
                    RotateDirection(connector.Direction, openConnector.Direction);

                if (worldDirection == openConnector.Direction.Opposite()) continue;

                openConnectors.Enqueue(
                    new OpenConnector(
                        nextGrid,
                        worldDirection
                    )
                );
            }
        }
    }

    private NetworkObject GetRandomStagePrefab()
    {
        float randomValue = UnityEngine.Random.value;

        if (guardianRoomCount < maxGuardianRoomCount && randomValue < guardianRoomRate)
        {
            guardianRoomCount++;
            return guardianRoomPrefab;
        }

        if (normalRoomCount < maxNormalRoomCount && randomValue < guardianRoomRate + normalRoomRate)
        {
            normalRoomCount++;
            return normalRoomPrefab;
        }

        return GetRandomCorridorPrefab();
    }

    private NetworkObject GetRandomCorridorPrefab()
    {
        int index = Random.Range(0, 3);

        return index switch
        {
            0 => straightCorridorPrefab,
            1 => tCorridorPrefab,
            2 => crossCorridorPrefab,
            _ => straightCorridorPrefab
        };
    }

    private Quaternion GetRotationFromDirection(GridDirection direction)
    {
        float y = direction switch
        {
            GridDirection.Forward => 0.0f,
            GridDirection.Right => 90.0f,
            GridDirection.Back => 180.0f,
            GridDirection.Left => 270.0f,
            _ => 0.0f
        };

        return Quaternion.Euler(0.0f, y, 0.0f);
    }

    private GridDirection RotateDirection(
        GridDirection localDirection,
        GridDirection baseDirection)
    {
        int local = (int)localDirection;
        int baseDir = (int)baseDirection;

        int result = (local + baseDir) % 4;

        return (GridDirection)result;
    }

    private bool IsGuardianRoom()
    {
        return UnityEngine.Random.value < 0.2f;
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