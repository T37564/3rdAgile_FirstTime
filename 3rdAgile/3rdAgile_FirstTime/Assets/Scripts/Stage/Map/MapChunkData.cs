using UnityEngine;
using Fusion;

public enum MapChunkType : byte
{
    Empty,
    StartRoom,
    Room,
    Corridor,
}

public struct MapChunkData : INetworkStruct
{
    public int X;
    public int Y;
    public MapChunkType Type;

    public MapChunkData(int x, int y, MapChunkType type)
    {
        X = x;
        Y = y;
        Type = type;
    }
}
