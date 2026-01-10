using System;
using CataMapGen.Core;
using CataMapGen.Noise;

namespace CataMapGen.Overmap
{
    /// <summary>
    /// Generates forest and swamp terrain using noise layers
    /// </summary>
    public class ForestGenerator
    {
        private readonly Overmap _overmap;
        private readonly ForestNoiseLayer _forestNoise;
        private readonly SwampNoiseLayer _swampNoise;

        public ForestGenerator(Overmap overmap)
        {
            _overmap = overmap;
            var basePoint = new Point2D(overmap.GlobalPosition.X * overmap.Width, 
                                        overmap.GlobalPosition.Y * overmap.Height);
            _forestNoise = new ForestNoiseLayer(basePoint, overmap.Seed);
            _swampNoise = new SwampNoiseLayer(basePoint, overmap.Seed + 2);
        }

        public void PlaceForests()
        {
            var settings = _overmap.Settings;
            
            for (int y = 0; y < _overmap.Height; y++)
            {
                for (int x = 0; x < _overmap.Width; x++)
                {
                    var tile = _overmap.GetTile(x, y);
                    if (tile.IsWater) continue;

                    float noise = _forestNoise.NoiseAt(new Point2D(x, y));

                    if (noise > settings.ForestThickThreshold)
                    {
                        tile.Terrain = OvermapTerrainType.ForestThick;
                        tile.UpdateFlags();
                    }
                    else if (noise > settings.ForestThreshold)
                    {
                        tile.Terrain = OvermapTerrainType.Forest;
                        tile.UpdateFlags();
                    }
                }
            }
        }

        public void PlaceSwamps()
        {
            var settings = _overmap.Settings;
            
            for (int y = 0; y < _overmap.Height; y++)
            {
                for (int x = 0; x < _overmap.Width; x++)
                {
                    var tile = _overmap.GetTile(x, y);
                    if (tile.IsWater || tile.Terrain == OvermapTerrainType.Forest) continue;

                    float noise = _swampNoise.NoiseAt(new Point2D(x, y));
                    bool nearWater = HasWaterNeighbor(x, y, 2);

                    if (noise > settings.SwampThreshold && nearWater)
                    {
                        tile.Terrain = OvermapTerrainType.Swamp;
                        tile.UpdateFlags();
                    }
                }
            }
        }

        private bool HasWaterNeighbor(int x, int y, int radius)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    if (dx == 0 && dy == 0) continue;
                    var tile = _overmap.GetTile(x + dx, y + dy);
                    if (tile != null && tile.IsWater) return true;
                }
            }
            return false;
        }
    }
}
