
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
    #region Stage Paths

    private const string START_ROOM_PATH = "Stages/Room 2";
    private const string STRAIGHT_CORRIDOR_PATH = "Stages/Aisle";
    private const string ROOM_PATH = "Stages/Room 1";

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

    [Tooltip("各Prefabの中心間距離")]
    [Min(0.1f)]
    [SerializeField] private float chunkSize = 12f;

    [Tooltip("開始部屋を除いた通常部屋の数")]
    [Min(0)]
    [SerializeField] private int targetRoomCount = 10;

    [Tooltip("生成を試す最大回数")]
    [Min(1)]
    [SerializeField] private int maxGenerateAttempts = 500;

    [Header("--- Corridor Settings ---")]

    [Tooltip("通路Prefabのローカル長軸")]
    [SerializeField]
    private CorridorLongAxis corridorLongAxis = CorridorLongAxis.Z;

    [Header("--- Debug Settings ---")]

    [SerializeField] private bool showChunkGizmos = true;

    [SerializeField]
    private Color roomGizmoColor =
        new Color(0f, 0.8f, 1f, 1f);

    [SerializeField]
    private Color corridorGizmoColor =
        new Color(0f, 1f, 0.4f, 1f);

    private readonly Dictionary<Vector2Int, NetworkObject>
        placedChunks = new();

    private readonly List<Vector2Int> roomPositions = new();

    private readonly HashSet<Vector2Int> corridorPositions = new();

    public static Action? OnMapGenerated;
    public static Action? OnNavMeshGenerated;

    private NavMeshSurface? navMeshSurface;

    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.right,
        Vector2Int.left
    };

    #region Unity

    private void Awake()
    {
        Debug.Log($"StageSpawner Awake : {gameObject.name}");

        LoadPrefabs();
    }

    private void LoadPrefabs()
    {
        startRoomPrefab =
            Resources.Load<NetworkObject>(START_ROOM_PATH);

        roomPrefab =
            Resources.Load<NetworkObject>(ROOM_PATH);

        straightCorridorPrefab =
            Resources.Load<NetworkObject>(STRAIGHT_CORRIDOR_PATH);

        if (startRoomPrefab == null ||
            roomPrefab == null ||
            straightCorridorPrefab == null)
        {
            Debug.LogError(
                "StageSpawner : Prefabの読み込みに失敗しました。"
            );
        }
    }

    #endregion

    #region Fusion

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        Debug.Log("StageSpawner OnSceneLoadDone");

        // ステージ生成はサーバーのみ
        if (!runner.IsServer)
            return;

        if (!ValidateSettings())
            return;

        Generate(runner);

        BuildNavMesh();
    }

    #endregion

    #region Generate

    private bool ValidateSettings()
    {
        if (startRoomPrefab == null ||
            roomPrefab == null ||
            straightCorridorPrefab == null)
        {
            Debug.LogError(
                "StageSpawner : 必要なPrefabが設定されていません。"
            );

            return false;
        }

        if (chunkSize <= 0f)
        {
            Debug.LogError(
                "StageSpawner : chunkSizeは0より大きくしてください。"
            );

            return false;
        }

        return true;
    }

    private void Generate(NetworkRunner runner)
    {
        placedChunks.Clear();
        roomPositions.Clear();
        corridorPositions.Clear();

        Vector2Int startPos = Vector2Int.zero;

        // 開始部屋
        PlaceChunk(
            runner,
            startPos,
            startRoomPrefab,
            Quaternion.identity
        );

        roomPositions.Add(startPos);

        int createdRoomCount = 0;
        int attempts = 0;

        while (createdRoomCount < targetRoomCount &&
               attempts < maxGenerateAttempts)
        {
            attempts++;

            // 既存の部屋をランダムに選択
            Vector2Int baseRoomPos =
                roomPositions[
                    Random.Range(0, roomPositions.Count)
                ];

            List<Vector2Int> directions =
                GetShuffledDirections();

            bool generated = false;

            foreach (Vector2Int dir in directions)
            {
                // 部屋 → 通路 → 部屋
                Vector2Int corridorPos =
                    baseRoomPos + dir;

                Vector2Int nextRoomPos =
                    baseRoomPos + dir * 2;

                if (!CanPlacePair(
                    corridorPos,
                    nextRoomPos))
                {
                    continue;
                }

                Quaternion corridorRotation =
                    GetCorridorRotation(dir);

                // 通路を配置
                PlaceChunk(
                    runner,
                    corridorPos,
                    straightCorridorPrefab,
                    corridorRotation
                );

                corridorPositions.Add(corridorPos);

                // 部屋は4方向に入口があるので回転不要
                PlaceChunk(
                    runner,
                    nextRoomPos,
                    roomPrefab,
                    Quaternion.identity
                );

                roomPositions.Add(nextRoomPos);

                createdRoomCount++;

                generated = true;

                break;
            }

            if (!generated)
            {
                continue;
            }
        }

        Debug.Log(
            $"Stage生成完了 : " +
            $"通常部屋 {createdRoomCount} 個 / " +
            $"通路 {corridorPositions.Count} 個"
        );

        if (createdRoomCount < targetRoomCount)
        {
            Debug.LogWarning(
                $"目標部屋数に到達できませんでした。" +
                $"生成数 : {createdRoomCount}"
            );
        }

        OnMapGenerated?.Invoke();
    }

    #endregion

    #region Placement

    private bool CanPlacePair(
        Vector2Int corridorPos,
        Vector2Int roomPos)
    {
        // 同じ座標には配置しない
        if (corridorPos == roomPos)
            return false;

        // 通路位置の確認
        if (placedChunks.ContainsKey(corridorPos))
            return false;

        // 部屋位置の確認
        if (placedChunks.ContainsKey(roomPos))
            return false;

        return true;
    }

    private void PlaceChunk(
        NetworkRunner runner,
        Vector2Int gridPos,
        NetworkObject prefab,
        Quaternion rotation)
    {
        Vector3 worldPos = GridToWorld(gridPos);

        NetworkObject instance = runner.Spawn(
            prefab,
            worldPos,
            rotation
        );

        placedChunks.Add(gridPos, instance);
    }

    private Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(
            gridPos.x * chunkSize,
            transform.position.y,
            gridPos.y * chunkSize
        ) + new Vector3(
            transform.position.x,
            0f,
            transform.position.z
        );
    }

    #endregion

    #region Rotation

    private Quaternion GetCorridorRotation(Vector2Int dir)
    {
        bool needVertical =
            dir == Vector2Int.up ||
            dir == Vector2Int.down;

        return corridorLongAxis switch
        {
            CorridorLongAxis.Z => needVertical
                ? Quaternion.Euler(0f, 90f, 0f)
                : Quaternion.identity,

            CorridorLongAxis.X => needVertical
                ? Quaternion.identity
                : Quaternion.Euler(0f, 90f, 0f),

            _ => Quaternion.identity
        };
    }

    #endregion

    #region Utility

    private List<Vector2Int> GetShuffledDirections()
    {
        return Directions
            .OrderBy(_ => Random.value)
            .ToList();
    }

    #endregion

    #region NavMesh

    private void BuildNavMesh()
    {
        navMeshSurface =
            FindAnyObjectByType<NavMeshSurface>();

        if (navMeshSurface == null)
        {
            Debug.LogError(
                "StageSpawner : NavMeshSurfaceが見つかりません。"
            );

            return;
        }

        navMeshSurface.BuildNavMesh();

        Debug.Log("NavMesh生成完了");

        OnNavMeshGenerated?.Invoke();
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        if (!showChunkGizmos)
            return;

        foreach (var pair in placedChunks)
        {
            Vector2Int gridPos = pair.Key;

            Vector3 worldPos = GridToWorld(gridPos);

            bool isCorridor =
                corridorPositions.Contains(gridPos);

            Gizmos.color = isCorridor
                ? corridorGizmoColor
                : roomGizmoColor;

            Gizmos.DrawWireCube(
                worldPos,
                new Vector3(
                    chunkSize,
                    2f,
                    chunkSize
                )
            );
        }
    }

    #endregion

    #region Unused Callbacks

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
