using System;
using System.Collections.Generic;
using System.Text;
using CataMapGen.Core;

namespace CataMapGen.Overmap
{
    /// <summary>
    /// Overmap settings for generation (matches CDDA's regional_settings)
    /// </summary>
    public class OvermapSettings
    {
        // Map dimensions (CDDA uses 180x180)
        public int Width = 180;
        public int Height = 180;

        // Terrain thresholds - BALANCED values
        public float ForestThreshold = 0.45f;      // Raised from 0.25 - less forest
        public float ForestThickThreshold = 0.65f; // Raised from 0.5 - less thick forest
        public float LakeThreshold = 0.65f;
        public float SwampThreshold = 0.3f;

        // City settings
        public int MinCities = 1;
        public int MaxCities = 4;
        public int MinCitySize = 4;
        public int MaxCitySize = 16;
        public int CitySpacing = 35;

        // River settings
        public int RiverCount = 2;
        public int MinRiverLength = 30;

        // Feature toggles
        public bool PlaceForests = true;
        public bool PlaceLakes = true;
        public bool PlaceRivers = true;
        public bool PlaceSwamps = true;
        public bool PlaceCities = true;
        public bool PlaceRoads = true;
    }

    /// <summary>
    /// Core overmap data structure - holds tile grid and provides access
    /// Generation logic is delegated to separate generator classes
    /// </summary>
    public class Overmap
    {
        public readonly int Width;
        public readonly int Height;
        public readonly Point2D GlobalPosition;
        public readonly uint Seed;
        public readonly OvermapSettings Settings;

        private readonly OvermapTile[,] _tiles;
        private readonly List<City> _cities = new List<City>();
        private readonly Random _rng;

        public Overmap(Point2D globalPos, uint seed, OvermapSettings settings = null)
        {
            GlobalPosition = globalPos;
            Seed = seed;
            Settings = settings ?? new OvermapSettings();
            _rng = new Random((int)seed);

            Width = Settings.Width;
            Height = Settings.Height;
            _tiles = new OvermapTile[Width, Height];

            // Initialize all tiles as field
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    _tiles[x, y] = new OvermapTile(OvermapTerrainType.Field);
        }

        #region Tile Access

        public OvermapTile GetTile(Point2D p) => GetTile(p.X, p.Y);

        public OvermapTile GetTile(int x, int y)
        {
            if (!InBounds(x, y)) return null;
            return _tiles[x, y];
        }

        public void SetTerrain(Point2D p, OvermapTerrainType terrain) => SetTerrain(p.X, p.Y, terrain);

        public void SetTerrain(int x, int y, OvermapTerrainType terrain)
        {
            if (!InBounds(x, y)) return;
            _tiles[x, y].Terrain = terrain;
            _tiles[x, y].UpdateFlags();
        }

        public bool InBounds(Point2D p) => InBounds(p.X, p.Y);
        public bool InBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

        #endregion

        #region City Management

        public IReadOnlyList<City> Cities => _cities;
        public void AddCity(City city) => _cities.Add(city);
        public Random GetRng() => _rng;

        #endregion

        #region Output

        public string ToAscii()
        {
            var sb = new StringBuilder();
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                    sb.Append(_tiles[x, y].GetSymbol());
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public static string GetLegend() => @"Legend:
. = Field    F = Forest    ~ = River    ≈ = Lake
& = Swamp    # = Road      = = Bridge
H = House    C = Commercial    I = Industrial";

        #endregion
    }
}
