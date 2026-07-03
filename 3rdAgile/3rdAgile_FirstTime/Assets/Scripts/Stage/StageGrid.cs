using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ステージ生成用のグリッド管理クラス
/// </summary>
public class StageGrid
{
    /// <summary>
    /// 1グリッドあたりのワールド座標の距離
    /// </summary>
    private readonly float cellsize;

    /// <summary>
    /// 使用済みグリッド座標
    /// </summary>
    private readonly HashSet<Vector2Int> usedCells = new();

    public StageGrid(float cellSize)
    {
        this.cellsize = cellSize;
    }

    /// <summary>
    /// 指定したグリッドが使用済みか
    /// </summary>
    public bool IsUsed(Vector2Int gridPosition)
    {
        return usedCells.Contains(gridPosition);
    }

    /// <summary>
    /// 指定したグリッドが空いているか
    /// </summary>
    public bool IsEmpty(Vector2Int gridPosition)
    {
        return !IsUsed(gridPosition);
    }

    /// <summary>
    /// グリッドを使用済みにする
    /// </summary>
    public void Register(Vector2Int gridPosition)
    {
        usedCells.Add(gridPosition);
    }

    /// <summary>
    /// グリッド座標をワールド座標に変換
    /// </summary>
    public Vector3 GridToWorld(Vector2Int gridPosition)
    {
        return new Vector3(
            gridPosition.x * cellsize,
            0.0f, 
            gridPosition.y * cellsize
        );
    }
}
