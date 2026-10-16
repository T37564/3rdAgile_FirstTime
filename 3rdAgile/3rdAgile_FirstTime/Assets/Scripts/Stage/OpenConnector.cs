//=========================================================================
// 制作者：スズキ
//=========================================================================

using UnityEngine;

/// <summary>
/// 未処理の接続点情報
/// </summary>
public readonly struct OpenConnector
{
    public readonly Vector2Int GridPosition;
    public readonly GridDirection Direction;

    public OpenConnector(Vector2Int gridPosition, GridDirection direction)
    {
        GridPosition = gridPosition;
        Direction = direction;
    }
}
