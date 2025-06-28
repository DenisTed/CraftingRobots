using UnityEngine;

[System.Serializable]
public class BiomePlantSpawn
{
    public RuleTile biomeTile;
    public GameObject[] treePrefabs;
    public GameObject[] plantPrefabs;
    public float spawnChancePerTile = 0.1f;
    public float treeSpawnChancePerTile = 0.2f;
}
