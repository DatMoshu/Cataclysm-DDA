using System;
using System.Collections.Generic;
using CataMapGen.Core;

namespace CataMapGen.Overmap
{
    /// <summary>
    /// Generates cities with grid-based streets and buildings
    /// Matches CDDA's city generation with collision detection for proper blocks
    /// </summary>
    public class CityGenerator
    {
        private readonly Overmap _overmap;
        private readonly Random _rng;
        private readonly HashSet<Point2D> _cityTiles = new HashSet<Point2D>();

        private const int BUILDINGCHANCE = 4; // 75% chance (1 - 1/4)

        public CityGenerator(Overmap overmap)
        {
            _overmap = overmap;
            _rng = overmap.GetRng();
        }

        public void PlaceCities()
        {
            var settings = _overmap.Settings;
            int cityCount = _rng.Next(settings.MinCities, settings.MaxCities + 1);
            int attempts = 0;
            int maxAttempts = cityCount * 50;

            while (_overmap.Cities.Count < cityCount && attempts < maxAttempts)
            {
                attempts++;

                int margin = settings.MaxCitySize + 5;
                int x = _rng.Next(margin, _overmap.Width - margin);
                int y = _rng.Next(margin, _overmap.Height - margin);
                var pos = new Point2D(x, y);

                if (_overmap.GetTile(x, y).IsWater) continue;

                bool tooClose = false;
                foreach (var existingCity in _overmap.Cities)
                {
                    if (existingCity.DistanceTo(pos) < settings.CitySpacing)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (tooClose) continue;

                // Size distribution matching CDDA
                int baseSize = (settings.MinCitySize + settings.MaxCitySize) / 2;
                int roll = _rng.Next(6);
                int size;
                if (roll < 2) size = settings.MinCitySize;
                else if (roll < 4) size = baseSize;
                else if (roll < 5) size = baseSize + 3;
                else size = settings.MaxCitySize;

                var newCity = new City(pos, size, $"City_{_overmap.Cities.Count + 1}");
                _overmap.AddCity(newCity);

                BuildCity(newCity);
            }

            // After all cities, fill enclosed areas
            FloodFillAllCityGaps();
        }

        private void BuildCity(City city)
        {
            // Place center intersection
            SetRoadTile(city.Position);

            // Build streets in all 4 directions with proper block layout
            foreach (OmDirection dir in new[] { OmDirection.North, OmDirection.East, OmDirection.South, OmDirection.West })
            {
                LayOutAndBuildStreet(city.Position, city.Size, dir, city, 2);
            }
        }

        /// <summary>
        /// Lay out street path first (checking collisions), then build it
        /// Matches CDDA's lay_out_street + build_city_street pattern
        /// </summary>
        private void LayOutAndBuildStreet(Point2D startPos, int length, OmDirection dir, City city, int blockWidth)
        {
            if (length <= 1 || dir == OmDirection.Invalid) return;

            // First, lay out the street path checking for collisions
            var streetPath = LayOutStreet(startPos, dir, length);
            if (streetPath.Count <= 1) return;

            // Build the street
            Point2D offset = dir.ToOffset();
            int remaining = length;
            int nextBranch = length;
            int newBlockWidth = blockWidth == 2 ? _rng.Next(3, 5) : 2;

            for (int i = 0; i < streetPath.Count; i++)
            {
                var pos = streetPath[i];
                remaining--;

                SetRoadTile(pos);

                // Branch at fixed intervals (block_width apart)
                if (remaining >= 2 && remaining < nextBranch - blockWidth)
                {
                    nextBranch = remaining;

                    // Branch lengths slightly shorter than main street
                    int leftLen = Math.Max(2, length - _rng.Next(1, 4));
                    int rightLen = Math.Max(2, length - _rng.Next(1, 4));

                    // Remove single-tile stubs
                    if (leftLen == 1) leftLen = 2;
                    if (rightLen == 1) rightLen = 2;

                    // Recursively build side streets
                    LayOutAndBuildStreet(pos, leftLen, dir.RotateCCW(), city, newBlockWidth);
                    LayOutAndBuildStreet(pos, rightLen, dir.RotateCW(), city, newBlockWidth);
                }

                // Place buildings with 75% chance (CDDA's BUILDINGCHANCE)
                if (!OneIn(BUILDINGCHANCE))
                    PlaceBuilding(pos, dir.RotateCCW(), city);
                if (!OneIn(BUILDINGCHANCE))
                    PlaceBuilding(pos, dir.RotateCW(), city);
            }

            // Sometimes turn at end to create neighborhoods
            if (remaining == 0 && length >= 3)
            {
                int newLen = Math.Max(2, length - _rng.Next(1, 4));
                OmDirection turnDir = _rng.Next(2) == 0 ? dir.RotateCW() : dir.RotateCCW();
                LayOutAndBuildStreet(streetPath[streetPath.Count - 1], newLen, turnDir, city, newBlockWidth);
            }
        }

        /// <summary>
        /// Plan a street path, checking for road collisions
        /// Matches CDDA's lay_out_street - stops if too many adjacent roads
        /// </summary>
        private List<Point2D> LayOutStreet(Point2D source, OmDirection dir, int maxLen)
        {
            var path = new List<Point2D>();
            Point2D offset = dir.ToOffset();

            for (int i = 1; i <= maxLen; i++)
            {
                Point2D pos = source + offset * i;

                if (!_overmap.InBounds(pos)) break;
                
                var tile = _overmap.GetTile(pos);
                if (tile.IsWater) break;

                // CDDA collision detection: count adjacent roads (excluding our path direction)
                int collisions = CountAdjacentRoads(pos, dir);
                
                // Stop if 3+ adjacent roads - prevents parallel streets
                if (collisions >= 3) break;

                path.Add(pos);
                _cityTiles.Add(pos);
            }

            return path;
        }

        /// <summary>
        /// Count road tiles adjacent to position (excluding forward/backward of travel direction)
        /// </summary>
        private int CountAdjacentRoads(Point2D pos, OmDirection travelDir)
        {
            int count = 0;
            Point2D forward = pos + travelDir.ToOffset();
            Point2D backward = pos + travelDir.Opposite().ToOffset();

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;

                    Point2D check = pos + new Point2D(dx, dy);
                    
                    // Exclude forward and backward positions
                    if (check == forward || check == backward) continue;
                    if (check == pos) continue;

                    var tile = _overmap.GetTile(check);
                    if (tile != null && tile.IsRoad)
                        count++;
                }
            }
            return count;
        }

        private void PlaceBuilding(Point2D roadPos, OmDirection dir, City city)
        {
            Point2D buildingPos = roadPos + dir.ToOffset();
            if (!_overmap.InBounds(buildingPos)) return;

            var tile = _overmap.GetTile(buildingPos);
            if (tile.IsWater || tile.IsRoad) return;
            if (IsBuildingTerrain(tile.Terrain)) return;

            int dist = city.DistanceTo(buildingPos);
            float ratio = (dist * 100f) / Math.Max(city.Size, 1);

            OvermapTerrainType building;
            if (ratio < 30)
                building = _rng.Next(2) == 0 ? OvermapTerrainType.Commercial : OvermapTerrainType.Industrial;
            else
                building = OvermapTerrainType.Residential;

            _overmap.SetTerrain(buildingPos, building);
            _cityTiles.Add(buildingPos);
        }

        /// <summary>
        /// Fill ALL enclosed city areas with buildings (global, not per-city)
        /// Matches CDDA's flood_fill_city_tiles
        /// </summary>
        private void FloodFillAllCityGaps()
        {
            var visited = new HashSet<Point2D>();

            foreach (var startPoint in _cityTiles)
            {
                // Check all 4 adjacent tiles
                foreach (var dir in new[] { OmDirection.North, OmDirection.East, OmDirection.South, OmDirection.West })
                {
                    var p = startPoint + dir.ToOffset();
                    if (!_overmap.InBounds(p) || visited.Contains(p) || _cityTiles.Contains(p)) continue;

                    var tile = _overmap.GetTile(p);
                    if (tile.IsRoad || IsBuildingTerrain(tile.Terrain)) continue;

                    // Flood fill from this empty point
                    var area = new List<Point2D>();
                    var queue = new Queue<Point2D>();
                    bool enclosed = true;

                    queue.Enqueue(p);
                    visited.Add(p);

                    while (queue.Count > 0)
                    {
                        var current = queue.Dequeue();
                        area.Add(current);

                        // Check if we've escaped to edge of map
                        if (current.X <= 0 || current.X >= _overmap.Width - 1 ||
                            current.Y <= 0 || current.Y >= _overmap.Height - 1)
                        {
                            enclosed = false;
                        }

                        foreach (var checkDir in new[] { OmDirection.North, OmDirection.East, OmDirection.South, OmDirection.West })
                        {
                            var next = current + checkDir.ToOffset();
                            if (!_overmap.InBounds(next) || visited.Contains(next)) continue;

                            var nextTile = _overmap.GetTile(next);
                            
                            // Stop at city tiles (roads/buildings)
                            if (_cityTiles.Contains(next) || nextTile.IsRoad || IsBuildingTerrain(nextTile.Terrain))
                                continue;

                            visited.Add(next);
                            queue.Enqueue(next);
                        }
                    }

                    // Fill enclosed areas with buildings - ONLY if adjacent to road
                    if (enclosed && area.Count > 0 && area.Count < 100)
                    {
                        foreach (var fillPoint in area)
                        {
                            var fillTile = _overmap.GetTile(fillPoint);
                            if (fillTile.IsWater) continue;
                            
                            // Only place if adjacent to a road
                            if (!IsAdjacentToRoad(fillPoint)) continue;

                            City nearestCity = FindNearestCity(fillPoint);
                            if (nearestCity != null)
                            {
                                int dist = nearestCity.DistanceTo(fillPoint);
                                float ratio = (dist * 100f) / Math.Max(nearestCity.Size, 1);
                                var building = ratio < 50 ? OvermapTerrainType.Commercial : OvermapTerrainType.Residential;
                                _overmap.SetTerrain(fillPoint, building);
                            }
                            else
                            {
                                _overmap.SetTerrain(fillPoint, OvermapTerrainType.Residential);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check if position has at least one adjacent road tile
        /// </summary>
        private bool IsAdjacentToRoad(Point2D pos)
        {
            foreach (var dir in new[] { OmDirection.North, OmDirection.East, OmDirection.South, OmDirection.West })
            {
                var neighbor = pos + dir.ToOffset();
                var tile = _overmap.GetTile(neighbor);
                if (tile != null && tile.IsRoad) return true;
            }
            return false;
        }

        private City FindNearestCity(Point2D pos)
        {
            City nearest = null;
            int nearestDist = int.MaxValue;
            foreach (var city in _overmap.Cities)
            {
                int dist = city.DistanceTo(pos);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = city;
                }
            }
            return nearest;
        }

        private bool IsBuildingTerrain(OvermapTerrainType t)
        {
            return t == OvermapTerrainType.Residential ||
                   t == OvermapTerrainType.Commercial ||
                   t == OvermapTerrainType.Industrial;
        }

        private void SetRoadTile(Point2D p)
        {
            _overmap.SetTerrain(p, OvermapTerrainType.Road);
            _cityTiles.Add(p);
            UpdateRoadConnections(p);

            foreach (var dir in new[] { OmDirection.North, OmDirection.East, OmDirection.South, OmDirection.West })
            {
                var neighbor = p + dir.ToOffset();
                if (_overmap.InBounds(neighbor) && _overmap.GetTile(neighbor).IsRoad)
                    UpdateRoadConnections(neighbor);
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
        }

        private bool IsRoadAt(int x, int y)
        {
            var tile = _overmap.GetTile(x, y);
            return tile != null && tile.IsRoad;
        }

        private bool OneIn(int n) => _rng.Next(n) == 0;
    }
}
