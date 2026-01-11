using System;
using System.Collections.Generic;
using CataMapGen.Core;

namespace CataMapGen.Overmap
{
    /// <summary>
    /// Types of overmap specials that can be placed
    /// </summary>
    public enum OvermapSpecialType
    {
        GasStation,
        Hospital,
        Mall,
        Police,
        FireStation,
        School,
        Church,
        Park,
        Cemetery
    }

    /// <summary>
    /// Generates overmap specials (unique buildings) within and around cities
    /// Matches CDDA's overmap_special placement system
    /// </summary>
    public class SpecialsGenerator
    {
        private readonly Overmap _overmap;
        private readonly Random _rng;
        private readonly HashSet<OvermapSpecialType> _placedUniques = new HashSet<OvermapSpecialType>();

        public SpecialsGenerator(Overmap overmap)
        {
            _overmap = overmap;
            _rng = overmap.GetRng();
        }

        /// <summary>
        /// Place specials in and around cities
        /// </summary>
        public void PlaceSpecials()
        {
            foreach (var city in _overmap.Cities)
            {
                PlaceCitySpecials(city);
            }

            // Place some specials outside cities (along roads)
            PlaceRoadsideSpecials();
        }

        private void PlaceCitySpecials(City city)
        {
            int specialCount = Math.Max(1, city.Size / 3);
            
            // Hospitals - one per large city
            if (city.Size >= 8 && TryPlaceSpecialInCity(city, OvermapSpecialType.Hospital, 'X', 0.3f))
                specialCount--;

            // Mall - center of large cities
            if (city.Size >= 10 && TryPlaceSpecialInCity(city, OvermapSpecialType.Mall, 'M', 0.2f))
                specialCount--;

            // Police station
            if (city.Size >= 5 && TryPlaceSpecialInCity(city, OvermapSpecialType.Police, 'P', 0.4f))
                specialCount--;

            // Fire station
            if (city.Size >= 6 && TryPlaceSpecialInCity(city, OvermapSpecialType.FireStation, 'f', 0.5f))
                specialCount--;

            // School
            if (city.Size >= 4 && TryPlaceSpecialInCity(city, OvermapSpecialType.School, 's', 0.6f))
                specialCount--;

            // Church
            if (TryPlaceSpecialInCity(city, OvermapSpecialType.Church, '+', 0.7f))
                specialCount--;

            // Parks - multiple allowed
            int parkCount = Math.Max(1, city.Size / 4);
            for (int i = 0; i < parkCount; i++)
            {
                TryPlaceSpecialInCity(city, OvermapSpecialType.Park, 'O', 0.8f, allowMultiple: true);
            }
        }

        private bool TryPlaceSpecialInCity(City city, OvermapSpecialType type, char symbol, 
            float maxDistRatio, bool allowMultiple = false)
        {
            if (!allowMultiple && _placedUniques.Contains(type)) return false;

            int maxDist = (int)(city.Size * maxDistRatio);
            int attempts = 20;

            while (attempts-- > 0)
            {
                int dx = _rng.Next(-maxDist, maxDist + 1);
                int dy = _rng.Next(-maxDist, maxDist + 1);
                var pos = city.Position + new Point2D(dx, dy);

                if (!_overmap.InBounds(pos)) continue;

                var tile = _overmap.GetTile(pos);
                
                // Replace existing buildings (commercial/residential) not roads
                if (tile.IsRoad || tile.IsWater) continue;
                if (tile.Terrain != OvermapTerrainType.Residential && 
                    tile.Terrain != OvermapTerrainType.Commercial &&
                    tile.Terrain != OvermapTerrainType.Field) continue;

                // Must be adjacent to road
                if (!IsAdjacentToRoad(pos)) continue;

                // Place the special
                PlaceSpecial(pos, type, symbol);
                if (!allowMultiple) _placedUniques.Add(type);
                return true;
            }

            return false;
        }

        private void PlaceRoadsideSpecials()
        {
            // Find road tiles outside cities for gas stations
            var roadsidePositions = new List<Point2D>();

            for (int y = 0; y < _overmap.Height; y++)
            {
                for (int x = 0; x < _overmap.Width; x++)
                {
                    var pos = new Point2D(x, y);
                    var tile = _overmap.GetTile(pos);
                    
                    if (!tile.IsRoad) continue;
                    if (IsInAnyCity(pos, 5)) continue; // Not too close to city center

                    // Check if it's a good spot (intersection or along highway)
                    int roadNeighbors = CountRoadNeighbors(pos);
                    if (roadNeighbors >= 2) // At least a corner or straight road
                        roadsidePositions.Add(pos);
                }
            }

            // Place gas stations along roads
            int gasStationCount = Math.Max(1, _overmap.Cities.Count * 2);
            int placed = 0;

            foreach (var roadPos in Shuffle(roadsidePositions))
            {
                if (placed >= gasStationCount) break;

                // Find adjacent non-road tile
                foreach (var dir in new[] { OmDirection.North, OmDirection.East, OmDirection.South, OmDirection.West })
                {
                    var buildPos = roadPos + dir.ToOffset();
                    var tile = _overmap.GetTile(buildPos);
                    
                    if (tile == null || tile.IsRoad || tile.IsWater) continue;
                    if (IsBuildingTerrain(tile.Terrain)) continue;

                    PlaceSpecial(buildPos, OvermapSpecialType.GasStation, 'G');
                    placed++;
                    break;
                }
            }
        }

        private void PlaceSpecial(Point2D pos, OvermapSpecialType type, char symbol)
        {
            // For now, use existing terrain types with special markers
            // In future, these would be proper special terrain types
            switch (type)
            {
                case OvermapSpecialType.Hospital:
                    _overmap.SetTerrain(pos, OvermapTerrainType.Hospital);
                    break;
                case OvermapSpecialType.Mall:
                    _overmap.SetTerrain(pos, OvermapTerrainType.Mall);
                    break;
                case OvermapSpecialType.GasStation:
                    _overmap.SetTerrain(pos, OvermapTerrainType.GasStation);
                    break;
                case OvermapSpecialType.Police:
                    _overmap.SetTerrain(pos, OvermapTerrainType.Police);
                    break;
                case OvermapSpecialType.FireStation:
                    _overmap.SetTerrain(pos, OvermapTerrainType.FireStation);
                    break;
                case OvermapSpecialType.School:
                    _overmap.SetTerrain(pos, OvermapTerrainType.School);
                    break;
                case OvermapSpecialType.Church:
                    _overmap.SetTerrain(pos, OvermapTerrainType.Church);
                    break;
                case OvermapSpecialType.Park:
                    _overmap.SetTerrain(pos, OvermapTerrainType.Park);
                    break;
                case OvermapSpecialType.Cemetery:
                    _overmap.SetTerrain(pos, OvermapTerrainType.Cemetery);
                    break;
            }
        }

        private bool IsAdjacentToRoad(Point2D pos)
        {
            foreach (var dir in new[] { OmDirection.North, OmDirection.East, OmDirection.South, OmDirection.West })
            {
                var tile = _overmap.GetTile(pos + dir.ToOffset());
                if (tile != null && tile.IsRoad) return true;
            }
            return false;
        }

        private bool IsInAnyCity(Point2D pos, int minDist)
        {
            foreach (var city in _overmap.Cities)
            {
                if (city.DistanceTo(pos) < minDist) return true;
            }
            return false;
        }

        private int CountRoadNeighbors(Point2D pos)
        {
            int count = 0;
            foreach (var dir in new[] { OmDirection.North, OmDirection.East, OmDirection.South, OmDirection.West })
            {
                var tile = _overmap.GetTile(pos + dir.ToOffset());
                if (tile != null && tile.IsRoad) count++;
            }
            return count;
        }

        private bool IsBuildingTerrain(OvermapTerrainType t)
        {
            return t == OvermapTerrainType.Residential ||
                   t == OvermapTerrainType.Commercial ||
                   t == OvermapTerrainType.Industrial ||
                   t == OvermapTerrainType.Hospital ||
                   t == OvermapTerrainType.Mall ||
                   t == OvermapTerrainType.GasStation;
        }

        private List<T> Shuffle<T>(List<T> list)
        {
            var result = new List<T>(list);
            for (int i = result.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                var temp = result[i];
                result[i] = result[j];
                result[j] = temp;
            }
            return result;
        }
    }
}
