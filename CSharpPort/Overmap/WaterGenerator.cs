using System;
using System.Collections.Generic;
using CataMapGen.Core;
using CataMapGen.Noise;
using CataMapGen.Utils;

namespace CataMapGen.Overmap
{
    /// <summary>
    /// Generates water features: rivers and lakes
    /// </summary>
    public class WaterGenerator
    {
        private readonly Overmap _overmap;
        private readonly LakeNoiseLayer _lakeNoise;
        private readonly SwampNoiseLayer _terrainNoise;
        private readonly Random _rng;

        public WaterGenerator(Overmap overmap)
        {
            _overmap = overmap;
            _rng = overmap.GetRng();
            var basePoint = new Point2D(overmap.GlobalPosition.X * overmap.Width, 
                                        overmap.GlobalPosition.Y * overmap.Height);
            _lakeNoise = new LakeNoiseLayer(basePoint, overmap.Seed + 1);
            _terrainNoise = new SwampNoiseLayer(basePoint, overmap.Seed + 2);
        }

        public void PlaceRivers()
        {
            var settings = _overmap.Settings;
            
            for (int r = 0; r < settings.RiverCount; r++)
            {
                Point2D start = GetRandomEdgePoint();
                Point2D end = GetRandomEdgePoint();

                while (start.ManhattanDistance(end) < settings.MinRiverLength)
                    end = GetRandomEdgePoint();

                var path = AStar.FindPath(start, end, _overmap.Width, _overmap.Height, (from, to) =>
                {
                    float noise = _terrainNoise.NoiseAt(to);
                    return 1.0f + (1.0f - noise) * 2.0f;
                });

                if (path.Found)
                {
                    foreach (var p in path.Path)
                        _overmap.SetTerrain(p, OvermapTerrainType.River);
                }
            }
        }

        public void PlaceLakes()
        {
            var settings = _overmap.Settings;
            
            for (int y = 0; y < _overmap.Height; y++)
            {
                for (int x = 0; x < _overmap.Width; x++)
                {
                    float noise = _lakeNoise.NoiseAt(new Point2D(x, y));
                    if (noise > settings.LakeThreshold)
                        _overmap.SetTerrain(x, y, OvermapTerrainType.Lake);
                }
            }

            AddShores(OvermapTerrainType.Lake, OvermapTerrainType.LakeShore);
        }

        public void PolishRivers()
        {
            for (int y = 0; y < _overmap.Height; y++)
            {
                for (int x = 0; x < _overmap.Width; x++)
                {
                    var tile = _overmap.GetTile(x, y);
                    if (tile.Terrain != OvermapTerrainType.River) continue;

                    int segments = 0;
                    if (IsWaterAt(x, y - 1)) segments = OmDirectionExtensions.SetSegment(segments, OmDirection.North);
                    if (IsWaterAt(x + 1, y)) segments = OmDirectionExtensions.SetSegment(segments, OmDirection.East);
                    if (IsWaterAt(x, y + 1)) segments = OmDirectionExtensions.SetSegment(segments, OmDirection.South);
                    if (IsWaterAt(x - 1, y)) segments = OmDirectionExtensions.SetSegment(segments, OmDirection.West);

                    tile.LineSegments = segments;

                    if (segments == 0b1111)
                    {
                        tile.Terrain = OvermapTerrainType.RiverCenter;
                        tile.UpdateFlags();
                    }
                }
            }
        }

        private void AddShores(OvermapTerrainType water, OvermapTerrainType shore)
        {
            var shoreTiles = new List<Point2D>();

            for (int y = 0; y < _overmap.Height; y++)
            {
                for (int x = 0; x < _overmap.Width; x++)
                {
                    var tile = _overmap.GetTile(x, y);
                    if (tile.Terrain != water) continue;

                    if (HasNonWaterNeighbor(x, y))
                        shoreTiles.Add(new Point2D(x, y));
                }
            }

            foreach (var p in shoreTiles)
                _overmap.SetTerrain(p, shore);
        }

        private bool HasNonWaterNeighbor(int x, int y)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;
                    var tile = _overmap.GetTile(x + dx, y + dy);
                    if (tile != null && !tile.IsWater) return true;
                }
            }
            return false;
        }

        private bool IsWaterAt(int x, int y)
        {
            var tile = _overmap.GetTile(x, y);
            return tile != null && tile.IsWater;
        }

        private Point2D GetRandomEdgePoint()
        {
            int side = _rng.Next(4);
            switch (side)
            {
                case 0: return new Point2D(_rng.Next(_overmap.Width), 0);
                case 1: return new Point2D(_rng.Next(_overmap.Width), _overmap.Height - 1);
                case 2: return new Point2D(0, _rng.Next(_overmap.Height));
                default: return new Point2D(_overmap.Width - 1, _rng.Next(_overmap.Height));
            }
        }
    }
}
