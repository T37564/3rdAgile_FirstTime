using UnityEngine;
using Fusion;
using System.Collections.Generic;

public class DungeonMapSync : NetworkBehaviour
{
    private const int MAX_CHUNKS = 64;

    [Networked, Capacity(MAX_CHUNKS)]
    public NetworkArray<MapChunkData> Chunks => default;

    [Networked] public int ChunkCount { get; set; }

    [Networked] public NetworkBool IsMapReady { get; set; }

    public void SetMapData(IReadOnlyList<MapChunkData> mapData)
    {
        if(!Object.HasStateAuthority) return;

        IsMapReady = false;

        int count = Mathf.Min(mapData.Count, MAX_CHUNKS);

        for (int i = 0; i < count; i++)
        {
            Chunks.Set(i, mapData[i]);
        }

        ChunkCount = count;
        IsMapReady = true;
    }
}
