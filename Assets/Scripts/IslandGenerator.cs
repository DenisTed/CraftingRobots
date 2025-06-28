using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class IslandChunkGenerator : MonoBehaviour
{
    public Tilemap tilemap;
    public TileBase waterTile;

    public int mapWidth = 100;
    public int mapHeight = 100;
    public int islandWidth = 10;
    public int islandHeight = 10;
    [Range(0f, 1f)] public float fillChance = 0.6f;

    public RuleTile forestTile, swampTile, desertTile, winterTile, volcanoTile;
    public TileBase forestWaterTile;
    public TileBase swampWaterTile;
    public TileBase desertWaterTile;
    public TileBase winterWaterTile;
    public TileBase volcanoWaterTile;

    public int mapChunksX = 10, mapChunksY = 10;
    public int islandCost = 10;
    public int playerMoney = 50000;

    public List<BiomePlantSpawn> biomeSpawns;

    private InputSystem_Actions inputActions;
    private HashSet<Vector2Int> generatedIslands = new HashSet<Vector2Int>();
    private HashSet<Vector3> usedSpawnPositions = new HashSet<Vector3>();
    private Vector2Int biomeMapCenter = new Vector2Int(5, 5);

    public enum BiomeType { Forest, Swamp, Desert, Winter, Volcano }

    private BiomeType[,] biomeMap = new BiomeType[7, 7]
    {
        { BiomeType.Winter, BiomeType.Winter, BiomeType.Winter, BiomeType.Winter, BiomeType.Winter, BiomeType.Winter, BiomeType.Winter },
        { BiomeType.Swamp,  BiomeType.Swamp,  BiomeType.Winter, BiomeType.Winter, BiomeType.Winter, BiomeType.Desert, BiomeType.Desert },
        { BiomeType.Swamp,  BiomeType.Swamp,  BiomeType.Forest, BiomeType.Forest, BiomeType.Forest, BiomeType.Desert, BiomeType.Desert },
        { BiomeType.Swamp,  BiomeType.Swamp,  BiomeType.Forest, BiomeType.Forest, BiomeType.Forest, BiomeType.Desert, BiomeType.Desert },
        { BiomeType.Swamp,  BiomeType.Swamp,  BiomeType.Forest, BiomeType.Forest, BiomeType.Forest, BiomeType.Desert, BiomeType.Desert },
        { BiomeType.Swamp,  BiomeType.Volcano,BiomeType.Volcano,BiomeType.Volcano,BiomeType.Volcano,BiomeType.Desert, BiomeType.Desert },
        { BiomeType.Volcano,BiomeType.Volcano,BiomeType.Volcano,BiomeType.Volcano,BiomeType.Volcano,BiomeType.Volcano,BiomeType.Volcano }
    };

    void Awake()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Enable();
        inputActions.Island.Generate.performed += ctx => GenerateIslandAt(mapWidth / 2, mapHeight / 2);
    }

    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector3 mousePos = Mouse.current.position.ReadValue();
            mousePos.z = 10f;
            Vector3 worldPos = Camera.main != null ? Camera.main.ScreenToWorldPoint(mousePos) : Vector3.zero;
            Vector3Int cellPos = tilemap.WorldToCell(worldPos);

            Vector2Int chunkCoord = new Vector2Int(
                Mathf.FloorToInt((float)(cellPos.x + mapWidth / 2) / (islandWidth - 1)),
                Mathf.FloorToInt((float)(cellPos.y + mapHeight / 2) / (islandHeight - 1))
            );

            if (!IsInBiomeMapBounds(chunkCoord))
                return;

            bool inBounds = chunkCoord.x >= 0 && chunkCoord.x < mapChunksX &&
                            chunkCoord.y >= 0 && chunkCoord.y < mapChunksY;

            if (!generatedIslands.Contains(chunkCoord) && HasNeighborIsland(chunkCoord) && inBounds)
            {
                if (playerMoney >= islandCost)
                {
                    playerMoney -= islandCost;
                    GenerateIslandChunk(chunkCoord);
                    Debug.Log($"Острів створено. Залишок грошей: {playerMoney}");
                }
                else
                {
                    Debug.Log("Недостатньо грошей для відкриття острова.");
                }
            }
        }
    }

    public bool HasNeighborIsland(Vector2Int coord)
    {
        Vector2Int[] directions = {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1)
        };

        foreach (var dir in directions)
            if (generatedIslands.Contains(coord + dir))
                return true;

        return false;
    }

    public void GenerateIslandAt(int centerX, int centerY)
    {
        tilemap.ClearAllTiles();

        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                tilemap.SetTile(new Vector3Int(x - mapWidth / 2, y - mapHeight / 2, 0), waterTile);

        Vector2Int centerChunk = new Vector2Int(centerX / (islandWidth - 1), centerY / (islandHeight - 1));
        generatedIslands.Add(centerChunk);
        GenerateIslandChunk(centerChunk);

        if (Camera.main != null)
        {
            Camera.main.transform.position = new Vector3(
                centerX - mapWidth / 2f + islandWidth / 2f,
                centerY - mapHeight / 2f + islandHeight / 2f,
                -10f
            );
            Camera.main.orthographicSize = Mathf.Max(mapWidth, mapHeight) / 2f;
        }
    }

    void GenerateIslandChunk(Vector2Int chunkCoord)
    {
        int startX = chunkCoord.x * (islandWidth - 1);
        int startY = chunkCoord.y * (islandHeight - 1);
        generatedIslands.Add(chunkCoord);

        float noiseScale = 0.3f;
        bool[,] localMap = new bool[islandWidth, islandHeight];

        RuleTile biomeTile = GetBiomeTile(chunkCoord);
        TileBase biomeWaterTile = waterTile;

        // Генерація шуму
        for (int x = 0; x < islandWidth; x++)
        {
            for (int y = 0; y < islandHeight; y++)
            {
                int globalX = startX + x;
                int globalY = startY + y;

                float baseNoise = Mathf.PerlinNoise(globalX * noiseScale, globalY * noiseScale);
                float detailNoise = Mathf.PerlinNoise(globalX * noiseScale * 0.5f, globalY * noiseScale * 0.5f) * 0.3f;
                localMap[x, y] = baseNoise + detailNoise > 0.4f;
            }
        }

        // Згладжування
        for (int i = 0; i < 3; i++)
            localMap = SmoothMap(localMap);

        // Видалення одиноких тайлів
        for (int x = 1; x < islandWidth - 1; x++)
        {
            for (int y = 1; y < islandHeight - 1; y++)
            {
                if (localMap[x, y])
                {
                    bool hasNeighbor = localMap[x - 1, y] || localMap[x + 1, y] || localMap[x, y - 1] || localMap[x, y + 1];
                    if (!hasNeighbor)
                        localMap[x, y] = false;
                }
            }
        }

        Vector2 islandCenter = new Vector2(islandWidth / 2f, islandHeight / 2f);
        float maxDistance = Vector2.Distance(Vector2.zero, islandCenter);

        bool isSurrounded = IsIslandSurroundedByWater(chunkCoord, localMap);
        if (isSurrounded)
            biomeWaterTile = GetWaterTileForBiome(biomeTile);

        bool spawnLake = isSurrounded && Random.value < 0.25f;

        // Спавн тайлів острова + озер
        for (int x = 2; x < islandWidth - 3; x++)
        {
            for (int y = 2; y < islandHeight - 3; y++)
            {
                if (spawnLake && Random.value < 0.01f)
                {
                    if (localMap[x, y] && localMap[x + 1, y] && localMap[x, y + 1] && localMap[x + 1, y + 1])
                    {
                        for (int dx = 0; dx <= 1; dx++)
                            for (int dy = 0; dy <= 1; dy++)
                            {
                                localMap[x + dx, y + dy] = false;
                                Vector3Int lakePos = new Vector3Int(startX + x + dx - mapWidth / 2, startY + y + dy - mapHeight / 2, 0);
                                tilemap.SetTile(lakePos, biomeWaterTile);
                            }
                        continue;
                    }
                }

                if (localMap[x, y] && localMap[x + 1, y] && localMap[x, y + 1] && localMap[x + 1, y + 1])
                {
                    Place2x2(startX + x, startY + y, biomeTile);
                }
            }
        }

        // Спавн рослинності (квіти - всередині, дерева - на краю)
        for (int x = 1; x < islandWidth - 2; x++)
        {
            for (int y = 1; y < islandHeight - 2; y++)
            {
                if (!localMap[x, y])
                    continue;

                Vector3Int tilePos = new Vector3Int(startX + x - mapWidth / 2, startY + y - mapHeight / 2, 0);
                Vector3 worldPos = tilemap.CellToWorld(tilePos) + new Vector3(0.5f, 0.5f, 0);
                if (usedSpawnPositions.Contains(worldPos)) continue;

                float distanceToCenter = Vector2.Distance(new Vector2(x, y), islandCenter);
                float proximity = 1f - (distanceToCenter / maxDistance);
                int sortingOrder = -(startY + y);

                foreach (var biome in biomeSpawns)
                {
                    if (biome.biomeTile != biomeTile)
                        continue;

                    bool spawned = false;

                    // 🌸 Квіти — рандомно по суші, не лише на краю
                    if (biome.plantPrefabs.Length > 0 && tilemap.GetTile(tilePos) == biomeTile)
                    {
                        if (Random.value < biome.spawnChancePerTile * proximity)
                        {
                            GameObject flower = Instantiate(
                                biome.plantPrefabs[Random.Range(0, biome.plantPrefabs.Length)],
                                worldPos, Quaternion.identity
                            );
                            SetSortingOrder(flower, sortingOrder);
                            spawned = true;
                        }
                    }

                    // 🌳 Дерева — можуть спавнитися будь-де на острові
                    if (!spawned && biome.treePrefabs.Length > 0)
                    {
                        if (tilemap.GetTile(tilePos) == biomeTile && Random.value < biome.treeSpawnChancePerTile * proximity)
                        {
                            GameObject tree = Instantiate(
                                biome.treePrefabs[Random.Range(0, biome.treePrefabs.Length)],
                                worldPos, Quaternion.identity
                            );
                            SetSortingOrder(tree, sortingOrder);
                            spawned = true;
                        }
                    }


                    if (spawned)
                    {
                        usedSpawnPositions.Add(worldPos);
                        break;
                    }
                }
            }
        }
    }

    bool[,] SmoothMap(bool[,] map)
    {
        bool[,] newMap = new bool[map.GetLength(0), map.GetLength(1)];

        for (int x = 1; x < map.GetLength(0) - 1; x++)
            for (int y = 1; y < map.GetLength(1) - 1; y++)
                newMap[x, y] = CountAliveNeighbors(map, x, y) > 4;

        return newMap;
    }

    int CountAliveNeighbors(bool[,] map, int x, int y)
    {
        int count = 0;
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
                if (!(dx == 0 && dy == 0) && map[x + dx, y + dy])
                    count++;
        return count;
    }

    RuleTile GetBiomeTile(Vector2Int chunkCoord)
    {
        int localX = chunkCoord.x - (biomeMapCenter.x - 3);
        int localY = chunkCoord.y - (biomeMapCenter.y - 3);

        if (localX >= 0 && localX < 7 && localY >= 0 && localY < 7)
            return BiomeToTile(biomeMap[6 - localY, localX]);

        return forestTile;
    }

    TileBase GetWaterTileForBiome(RuleTile biomeTile)
    {
        if (biomeTile == forestTile) return forestWaterTile;
        if (biomeTile == swampTile) return swampWaterTile;
        if (biomeTile == desertTile) return desertWaterTile;
        if (biomeTile == winterTile) return winterWaterTile;
        if (biomeTile == volcanoTile) return volcanoWaterTile;
        return waterTile;
    }

    RuleTile BiomeToTile(BiomeType biome)
    {
        switch (biome)
        {
            case BiomeType.Forest: return forestTile;
            case BiomeType.Swamp: return swampTile;
            case BiomeType.Desert: return desertTile;
            case BiomeType.Winter: return winterTile;
            case BiomeType.Volcano: return volcanoTile;
            default: return forestTile;
        }
    }

    public bool IsInBiomeMapBounds(Vector2Int chunkCoord)
    {
        int dx = chunkCoord.x - biomeMapCenter.x;
        int dy = chunkCoord.y - biomeMapCenter.y;
        return Mathf.Abs(dx) <= 3 && Mathf.Abs(dy) <= 3;
    }

    bool IsEdgeTile(Vector3Int pos, RuleTile biomeTile)
    {
        Vector3Int[] dirs = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
        foreach (var dir in dirs)
        {
            TileBase neighbor = tilemap.GetTile(pos + dir);
            if (neighbor != biomeTile)
                return true;
        }
        return false;
    }

    bool IsIslandSurroundedByWater(Vector2Int chunkCoord, bool[,] localMap)
    {
        int w = localMap.GetLength(0);
        int h = localMap.GetLength(1);

        for (int x = 0; x < w; x++)
            if (localMap[x, 0] || localMap[x, h - 1])
                return false;

        for (int y = 0; y < h; y++)
            if (localMap[0, y] || localMap[w - 1, y])
                return false;

        return true;
    }

    void Place2x2(int gx, int gy, RuleTile tile)
    {
        Vector3Int pos = new Vector3Int(gx - mapWidth / 2, gy - mapHeight / 2, 0);
        tilemap.SetTile(pos, tile);
        tilemap.SetTile(pos + Vector3Int.right, tile);
        tilemap.SetTile(pos + Vector3Int.up, tile);
        tilemap.SetTile(pos + Vector3Int.right + Vector3Int.up, tile);
    }

    void SetSortingOrder(GameObject obj, int order)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sortingOrder = order;
    }

    public bool IslandExists(Vector2Int chunkCoord)
    {
        return generatedIslands.Contains(chunkCoord);
    }
}
