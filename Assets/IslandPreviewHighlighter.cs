using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class IslandPreviewHighlighter : MonoBehaviour
{
    public IslandChunkGenerator generator;
    public SpriteRenderer highlightSprite;

    private void Update()
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        mousePos.z = 10f;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        Vector3Int cellPos = generator.tilemap.WorldToCell(worldPos);

        Vector2Int chunkCoord = new Vector2Int(
            Mathf.FloorToInt((float)(cellPos.x + generator.mapWidth / 2) / (generator.islandWidth - 1)),
            Mathf.FloorToInt((float)(cellPos.y + generator.mapHeight / 2) / (generator.islandHeight - 1))
        );

        // Перевірка, чи в межах карти
        bool inBounds = chunkCoord.x >= 0 && chunkCoord.x < generator.mapChunksX &&
                        chunkCoord.y >= 0 && chunkCoord.y < generator.mapChunksY;

        // Якщо острів ще не згенерований, є сусіди і в межах
        if (inBounds &&
            !generator.IslandExists(chunkCoord) &&
            generator.HasNeighborIsland(chunkCoord) &&
            generator.IsInBiomeMapBounds(chunkCoord))
        {
            Vector3 centerWorldPos = new Vector3(
                chunkCoord.x * (generator.islandWidth - 1) - generator.mapWidth / 2 + (generator.islandWidth / 2f),
                chunkCoord.y * (generator.islandHeight - 1) - generator.mapHeight / 2 + (generator.islandHeight / 2f),
                0
            );

            highlightSprite.gameObject.SetActive(true);
            highlightSprite.transform.position = centerWorldPos;
        }
        else
        {
            highlightSprite.gameObject.SetActive(false);
        }
    }
}
