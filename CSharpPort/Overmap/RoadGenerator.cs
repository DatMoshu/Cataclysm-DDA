using System.Collections.Generic;
using CataMapGen.Core;
using CataMapGen.Utils;

namespace CataMapGen.Overmap
{
    /// <summary>
    /// Generates roads connecting cities via A* pathfinding
    /// </summary>
    public class RoadGenerator
    {
        private readonly Overmap _overmap;

        public RoadGenerator(Overmap overmap)
        {
            _overmap = overmap;
        }

        public void PlaceRoads()
        {
            if (_overmap.Cities.Count < 2) return;

            var connected = new HashSet<City> { _overmap.Cities[0] };
            var unconnected = new List<City>(_overmap.Cities);
            unconnected.RemoveAt(0);

            while (unconnected.Count > 0)
            {
                City bestFrom = null;
                City bestTo = null;
                int bestDist = int.MaxValue;

                foreach (var from in connected)
                {
                    foreach (var to in unconnected)
                    {
                        int dist = from.DistanceTo(to.Position);
                        if (dist < bestDist)
                        {
                            bestDist = dist;
                            bestFrom = from;
                            bestTo = to;
                        }
                    }
                }

                if (bestTo != null)
                {
                    BuildRoad(bestFrom.Position, bestTo.Position);
                    connected.Add(bestTo);
                    unconnected.Remove(bestTo);
                }
            }
        }

        private void BuildRoad(Point2D from, Point2D to)
        {
            var path = AStar.FindPath(from, to, _overmap.Width, _overmap.Height, (a, b) =>
            {
                var tile = _overmap.GetTile(b);
                if (tile == null) return float.MaxValue;
                if (tile.IsWater) return 50.0f;
                if (tile.Terrain == OvermapTerrainType.Forest) return 3.0f;
                if (tile.Terrain == OvermapTerrainType.ForestThick) return 5.0f;
                if (tile.IsRoad) return 0.5f;
                return 1.0f;
            });

            if (path.Found)
            {
                foreach (var p in path.Path)
                {
                    var tile = _overmap.GetTile(p);
                    if (tile.IsWater)
                    {
                        _overmap.SetTerrain(p, OvermapTerrainType.Bridge);
                    }
                    else if (!tile.IsRoad)
                    {
                        _overmap.SetTerrain(p, OvermapTerrainType.Road);
                        UpdateRoadConnections(p);
                    }
                    else
                    {
                        UpdateRoadConnections(p);
                    }
                }
            }
        }

        private void UpdateRoadConnections(Point2D p)
        {
            var tile = _overmap.GetTile(p);
            if (tile == null || !tile.IsRoad) return;

            int segments = 0;
            if (IsRoadAt(p.X, p.Y - 1)) segments = OmDirectionExtensions.SetSegment(segments, OmDirection.North);
            if (IsRoadAt(p.X + 1, p.Y)) segments = OmDirectionExtensions.SetSegment(segments, OmDirection.East);
            if (IsRoadAt(p.X, p.Y + 1)) segments = OmDirectionExtensions.SetSegment(segments, OmDirection.South);
            if (IsRoadAt(p.X - 1, p.Y)) segments = OmDirectionExtensions.SetSegment(segments, OmDirection.West);

            tile.LineSegments = segments;

            // Update neighbors
            foreach (var dir in new[] { OmDirection.North, OmDirection.East, OmDirection.South, OmDirection.West })
            {
                var neighbor = p + dir.ToOffset();
                var neighborTile = _overmap.GetTile(neighbor);
                if (neighborTile != null && neighborTile.IsRoad)
                {
                    int nSeg = 0;
                    if (IsRoadAt(neighbor.X, neighbor.Y - 1)) nSeg = OmDirectionExtensions.SetSegment(nSeg, OmDirection.North);
                    if (IsRoadAt(neighbor.X + 1, neighbor.Y)) nSeg = OmDirectionExtensions.SetSegment(nSeg, OmDirection.East);
                    if (IsRoadAt(neighbor.X, neighbor.Y + 1)) nSeg = OmDirectionExtensions.SetSegment(nSeg, OmDirection.South);
                    if (IsRoadAt(neighbor.X - 1, neighbor.Y)) nSeg = OmDirectionExtensions.SetSegment(nSeg, OmDirection.West);
                    neighborTile.LineSegments = nSeg;
                }
            }
        }

        private bool IsRoadAt(int x, int y)
        {
            var tile = _overmap.GetTile(x, y);
            return tile != null && tile.IsRoad;
        }
    }
}
