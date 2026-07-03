#nullable enable

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChunkStageSpawner : MonoBehaviour
{
    private enum CorridorLongAxis
    {
        Z,
        X
    }

    [Header("--- Prefabs ---")]
    [SerializeField] private GameObject startRoomPrefab = null!;
    [SerializeField] private GameObject roomPrefab = null!;
    [SerializeField] private GameObject straightCorridorPrefab = null!;

    [Header("--- Generate Settings ---")]
    [SerializeField] private float chunkSize = 20.0f;

    [Tooltip("StartïîâÆÇä‹ÇﬂÇ»Ç¢í èÌïîâÆÇÃêî")]
    [SerializeField] private int targetRoomCount = 10;

    [Tooltip("ê∂ê¨ÇééÇ∑ç≈ëÂâÒêîÅBãlÇ‹ÇËñhé~óp")]
    [SerializeField] private int maxGenerateAttempts = 500;

    [Header("--- Corridor Settings ---")]
    [SerializeField] private CorridorLongAxis corridorLongAxis = CorridorLongAxis.Z;

    private readonly Dictionary<Vector2Int, GameObject> placedChunks = new();
    private readonly List<Vector2Int> roomPositions = new();

    private static readonly Vector2Int[] Directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.right,
        Vector2Int.left
    };

    private void Start()
    {
        Generate();
    }

    private void Generate()
    {
        placedChunks.Clear();
        roomPositions.Clear();

        Vector2Int startPos = Vector2Int.zero;

        PlaceChunk(startPos, startRoomPrefab, Quaternion.identity);
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

                PlaceChunk(corridorPos, straightCorridorPrefab, GetCorridorRotation(dir));
                PlaceChunk(roomPos, roomPrefab, Quaternion.identity);

                roomPositions.Add(roomPos);
                createdRoomCount++;

                break;
            }
        }

        if (createdRoomCount < targetRoomCount)
        {
            Debug.LogWarning($"ïîâÆÇ {targetRoomCount} å¬ê∂ê¨Ç≈Ç´Ç‹ÇπÇÒÇ≈ÇµÇΩÅBê∂ê¨êî: {createdRoomCount}");
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

    private void PlaceChunk(Vector2Int gridPos, GameObject prefab, Quaternion rotation)
    {
        Vector3 worldPos = GridToWorld(gridPos);
        GameObject instance = Instantiate(prefab, worldPos, rotation, transform);
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
}