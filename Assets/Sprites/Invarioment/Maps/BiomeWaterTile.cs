using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Tiles/Biome Tile")]
public class BiomeTile : RuleTile<BiomeTile.Neighbor>
{
    public class Neighbor : RuleTile.TilingRule.Neighbor
    {
        public const int BiomeGroup = 3;
    }

    public override bool RuleMatch(int neighbor, TileBase tile)
    {
        if (neighbor == Neighbor.BiomeGroup)
            return tile is BiomeTile;

        return base.RuleMatch(neighbor, tile);
    }
}
