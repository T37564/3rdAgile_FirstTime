#nullable enable

using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using Random = UnityEngine.Random;

public class StageSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    #region ステージパス

    private readonly string START_ROOM_PATH = "Stages/Start";
    private readonly string STRAIGHT_CORRIDOR_PATH = "Stages/StraightCorridor";
    private readonly string ROOM_PATH = "Stages/Room";
    private readonly string T_CORRIDOR_PATH = "Stages/TCorridor";
    private readonly string CROSS_CORRIDOR_PATH = "Stages/CrossCorridor";
    private readonly string GUARDIAN_ROOM_PATH = "Stages/GuardianRoom";
    private readonly string DEAD_END_PATH = "Stages/DeadEnd";

    #endregion

    private enum CorridorLongAxis
    {
        Z,
        X
    }

    [Header("--- Prefabs ---")]
    [SerializeField] private NetworkObject startRoomPrefab = null!;
    [SerializeField] private NetworkObject roomPrefab = null!;
    [SerializeField] private NetworkObject straightCorridorPrefab = null!;

    [Header("--- Generate Settings ---")]
    [SerializeField] private float chunkSize = 10.0f;

    [Tooltip("Start部屋を含めない通常部屋の数")]
    [SerializeField] private int targetRoomCount = 10;

    [Tooltip("生成を試す最大回数。詰まり防止用")]
    [SerializeField] private int maxGenerateAttempts = 500;

    [Header("--- Corridor Settings ---")]
    [SerializeField] private CorridorLongAxis corridorLongAxis = CorridorLongAxis.Z;

    private readonly Dictionary<Vector2Int, NetworkObject> placedChunks = new();
    private readonly List<Vector2Int> roomPositions = new();

    // 部屋を生成した後に処理を行うイベント
    //public static Action? OnMapGenerated;
    public static Action? OnMapGenerated;

    public static Action? OnNavMeshGenerated;

    private NavMeshSurface navMeshSurface;

    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.right,
        Vector2Int.left
    };

    private void Awake()
    {
        Debug.Log($"Awake Object = {gameObject.name}");
        startRoomPrefab = Resources.Load<NetworkObject>(START_ROOM_PATH);
        roomPrefab = Resources.Load<NetworkObject>(ROOM_PATH);
        straightCorridorPrefab = Resources.Load<NetworkObject>(STRAIGHT_CORRIDOR_PATH);

    }

    /// <summary>
    /// シーンロード完了時
    /// </summary>
    public void OnSceneLoadDone(NetworkRunner runner)
    {
        Debug.Log("StageSpawner OnSceneLoadDone");
        if (!runner.IsServer) return;
        Generate(runner);

        navMeshSurface = FindAnyObjectByType<NavMeshSurface>();
        Debug.Log($"NavMeshSurface : {navMeshSurface}");
        if (navMeshSurface == null)
        {
            Debug.LogError("NavMeshSurfaceが取得できていないためNavMeshを生成できません");
            return;
        }
        navMeshSurface.BuildNavMesh();

        // NavMesh生成完了後にイベントを実行
        OnNavMeshGenerated?.Invoke();
    }
    private void Generate(NetworkRunner runner)
    {
        placedChunks.Clear();
        roomPositions.Clear();

        Vector2Int startPos = Vector2Int.zero;

        PlaceChunk(runner, startPos, startRoomPrefab, Quaternion.identity);
        roomPositions.Add(startPos);

        int createdRoomCount = 0;
        int attempts = 0;

        while (createdRoomCount < targetRoomCount && attempts < maxGenerateAttempts)
        {
            attempts++;

            Vector2Int baseRoomPos = roomPositions[Random.Range(0, roomPositions.Count)];
            List<Vector2Int> shuffledDirections = GetShuffledDirections();

            foreach (Vector2Int dir in shuffledDirections)
            {
                Vector2Int corridorPos = baseRoomPos + dir;
                Vector2Int roomPos = baseRoomPos + dir * 2;

                if (!CanPlace(corridorPos)) continue;
                if (!CanPlace(roomPos)) continue;

                PlaceChunk(runner, corridorPos, straightCorridorPrefab, GetCorridorRotation(dir));

                // 部屋の生成
                PlaceChunk(runner, roomPos, roomPrefab, Quaternion.identity);

                roomPositions.Add(roomPos);
                createdRoomCount++;

                break;
            }
        }
        
        // すべての部屋を生成した後イベント実行
        OnMapGenerated?.Invoke();
        //navMeshSurface.BuildNavMesh();

        if (createdRoomCount < targetRoomCount)
        {
            Debug.LogWarning($"部屋を {targetRoomCount} 個生成できませんでした。生成数: {createdRoomCount}");
        }
    }

    private List<Vector2Int> GetShuffledDirections()
    {
        return Directions
            .OrderBy(_ => Random.value)
            .ToList();
    }

    private bool CanPlace(Vector2Int gridPos)
    {
        return !placedChunks.ContainsKey(gridPos);
    }

    private void PlaceChunk(NetworkRunner runner, Vector2Int gridPos, NetworkObject prefab, Quaternion rotation)
    {
        Vector3 worldPos = GridToWorld(gridPos);
        NetworkObject instance = runner.Spawn(prefab, worldPos, rotation);
        placedChunks.Add(gridPos, instance);
    }

    private Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(
            gridPos.x * chunkSize,
            0.0f,
            gridPos.y * chunkSize
        );
    }

    private Quaternion GetCorridorRotation(Vector2Int dir)
    {
        bool needVertical = dir == Vector2Int.up || dir == Vector2Int.down;

        return corridorLongAxis switch
        {
            CorridorLongAxis.Z => needVertical
                ? Quaternion.Euler(0.0f, 90.0f, 0.0f)
                : Quaternion.identity,

            CorridorLongAxis.X => needVertical
                ? Quaternion.identity
                : Quaternion.Euler(0.0f, 90.0f, 0.0f),

            _ => Quaternion.identity
        };
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