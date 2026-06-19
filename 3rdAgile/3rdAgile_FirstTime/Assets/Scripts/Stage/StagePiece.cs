using UnityEngine;

public class StagePiece : MonoBehaviour
{
    [Header("このパーツが占有するグリッド")]
    [SerializeField] private Vector2Int[] occupiedCells =
    {
        Vector2Int.zero
    };

    [Header("このパーツが持つ接続点")]
    [SerializeField] private StageConnector[] connectors;

    public Vector2Int[] OccupiedCells => occupiedCells;
    public StageConnector[] Connectors => connectors;
}
