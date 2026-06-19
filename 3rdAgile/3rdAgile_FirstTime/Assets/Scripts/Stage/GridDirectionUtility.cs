using UnityEngine;

public static class GridDirectionUtility
{
    public static Vector2Int ToVector(this GridDirection direction)
    {
        return direction switch
        {
            GridDirection.Forward => Vector2Int.up,
            GridDirection.Back => Vector2Int.down,
            GridDirection.Right => Vector2Int.right,
            GridDirection.Left => Vector2Int.left,
            _=> Vector2Int.zero,
        };
    }

    public static GridDirection Opposite(this GridDirection direction)
    {
        return direction switch
        {
            GridDirection.Forward => GridDirection.Back,
            GridDirection.Back => GridDirection.Forward,
            GridDirection.Right => GridDirection.Left,
            GridDirection.Left => GridDirection.Right,
            _ => direction,
        };
    }
}
