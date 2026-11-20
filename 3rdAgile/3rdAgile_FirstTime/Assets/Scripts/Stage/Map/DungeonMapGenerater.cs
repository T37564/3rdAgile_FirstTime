using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class DungeonMapGenerater : MonoBehaviour
{
    private const int PIXEL_PER_CHUNK = 16;

    private int minX;
    private int minY;

    private int mapWidthInChunks;
    private int mapHeightInChunks;

    public RectTransform MapRect => mapImage.rectTransform;

    [SerializeField] private DungeonMapSync mapSync;

    [SerializeField] private RawImage mapImage;

    [Header("Map Color")]
    [SerializeField] private Color roomColor = Color.cyan;
    [SerializeField] private Color corridorColor = Color.green;
    [SerializeField] private Color startRoomColor = Color.yellow;

    private Texture2D generatedTexture;
    public bool generated {  get; private set; }

    private void Update()
    {
        if(generated) return;
        if(mapSync == null || mapSync.Object == null || !mapSync.Object.IsValid) return;
        if(!mapSync.IsMapReady) return;
        if (mapSync.ChunkCount <= 0) return;

        GenerateMap();

        generated = true;
    }

    private void GenerateMap()
    {
        int count = mapSync.ChunkCount;

        if (count <= 0) return;

        minX = int.MaxValue;
        minY = int.MaxValue;

        int maxX = int.MinValue;
        int maxY = int.MinValue;

        for (int i = 0; i < count; i++)
        {
            MapChunkData chunk = mapSync.Chunks[i];

            minX = Mathf.Min(minX, chunk.X);
            minY = Mathf.Min(minY, chunk.Y);

            maxX = Mathf.Max(maxX, chunk.X);
            maxY = Mathf.Max(maxY, chunk.Y);
        }

        mapWidthInChunks = maxX - minX + 1;
        mapHeightInChunks = maxY - minY + 1;

        int width = mapWidthInChunks * PIXEL_PER_CHUNK;
        int height = mapHeightInChunks * PIXEL_PER_CHUNK;

        generatedTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        Color[] pixels = new Color[width * height];

        for (int i = 0; i < count; i++)
        {
            MapChunkData chunk = mapSync.Chunks[i];

            Color color = chunk.Type switch
            {
                MapChunkType.StartRoom => startRoomColor,
                MapChunkType.Room => roomColor,
                MapChunkType.Corridor => corridorColor,
                _ => Color.clear
            };

            int startX = (chunk.X - minX) * PIXEL_PER_CHUNK;
            int startY = (chunk.Y - minY) * PIXEL_PER_CHUNK;

            for (int y = 0; y < PIXEL_PER_CHUNK; y++)
            {
                for (int x = 0; x < PIXEL_PER_CHUNK; x++)
                {
                    int pixelX = startX + x;
                    int pixelY = startY + y;

                    pixels[pixelY * width + pixelX] = color;
                }
            }
        }
        generatedTexture.SetPixels(pixels);
        generatedTexture.filterMode = FilterMode.Point;
        generatedTexture.Apply(false, true);

        mapImage.texture = generatedTexture;
    }

    public Vector2 WorldToMapPosition(Vector3 worldPosition, Vector3 stageOrigin, float chunkSize)
    {
        float gridX = (worldPosition.x - stageOrigin.x) / chunkSize;
        float gridY = (worldPosition.z - stageOrigin.z) / chunkSize;

        float normalizedX = (gridX - minX + 0.5f) / mapWidthInChunks;
        float normalizedY = (gridY - minY + 0.5f) / mapHeightInChunks;

        Rect rect = MapRect.rect;

        return new Vector2((normalizedX - MapRect.pivot.x) * rect.width, (normalizedY - MapRect.pivot.y) * rect.height);
    }

    public Vector2 WorldToMapPosition(Vector3 worldPosition)
    {
        if(!generated || mapSync.ChunkSize <= 0.0f) return Vector2.zero;

        Vector3 origin = mapSync.StageOrigin;
        float chunkSize = mapSync.ChunkSize;

        float gridX = (worldPosition.x - origin.x) / chunkSize;
        float gridY = (worldPosition.z - origin.z) / chunkSize;

        float normalizedX = (gridX - minX + 0.5f) / mapWidthInChunks;
        float normalizedY = (gridY - minY + 0.5f) / mapHeightInChunks;

        Rect rect = MapRect.rect;

        return new Vector2((normalizedX - MapRect.pivot.x) * rect.width, (normalizedY - MapRect.pivot.y) * rect.height);
    }

    private void OnDestroy()
    {
        if (generatedTexture != null)
        {
            Destroy(generatedTexture);
        }
    }
}
