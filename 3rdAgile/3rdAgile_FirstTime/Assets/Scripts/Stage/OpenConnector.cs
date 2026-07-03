using UnityEngine;

/// <summary>
/// –¢ˆ—‚ÌÚ‘±“_î•ñ
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
