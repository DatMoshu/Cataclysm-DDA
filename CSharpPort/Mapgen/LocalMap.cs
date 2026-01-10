using System;
using System.Text;
using CataMapGen.Core;
using CataMapGen.Noise;
using CataMapGen.Overmap;

namespace CataMapGen.Mapgen
{
    /// <summary>
    /// Context data passed to mapgen functions (matches CDDA's mapgendata)
    /// </summary>
    public class MapgenData
    {
        public readonly OvermapTerrainType OvermapTerrain;
        public readonly Point2D LocalPosition;      // Position within overmap
        public readonly Point2D GlobalPosition;     // Global position
        public readonly uint Seed;
        public float MonsterDensity = 1.0f;

        // Neighbor terrain for seamless generation
        public OvermapTerrainType? TerrainNorth;
        public OvermapTerrainType? TerrainSouth;
        public OvermapTerrainType? TerrainEast;
        public OvermapTerrainType? TerrainWest;

        public MapgenData(OvermapTerrainType terrain, Point2D localPos, Point2D globalPos, uint seed)
        {
            OvermapTerrain = terrain;
            LocalPosition = localPos;
            GlobalPosition = globalPos;
            Seed = seed;
        }
    }

    /// <summary>
    /// A 24x24 local map (submap) corresponding to one overmap tile
    /// </summary>
    public class LocalMap
    {
        public const int MapSize = 24;  // CDDA uses 24x24 submaps

        private readonly LocalTile[,] _tiles;
        private readonly Random _rng;

        public LocalMap(uint seed)
        {
            _tiles = new LocalTile[MapSize, MapSize];
            _rng = new Random((int)seed);

            // Initialize with grass
            for (int y = 0; y < MapSize; y++)
            {
                for (int x = 0; x < MapSize; x++)
                {
                    _tiles[x, y] = new LocalTile(LocalTerrainType.Grass);
                }
            }
        }

        public LocalTile GetTile(int x, int y)
        {
            if (x < 0 || x >= MapSize || y < 0 || y >= MapSize)
                return null;
            return _tiles[x, y];
        }

        public void SetTerrain(int x, int y, LocalTerrainType terrain)
        {
            if (x < 0 || x >= MapSize || y < 0 || y >= MapSize) return;
            _tiles[x, y].Terrain = terrain;
        }

        public void SetFurniture(int x, int y, FurnitureType furniture)
        {
            if (x < 0 || x >= MapSize || y < 0 || y >= MapSize) return;
            _tiles[x, y].Furniture = furniture;
        }

        /// <summary>
        /// Fills entire map with terrain type
        /// </summary>
        public void Fill(LocalTerrainType terrain)
        {
            for (int y = 0; y < MapSize; y++)
                for (int x = 0; x < MapSize; x++)
                    _tiles[x, y].Terrain = terrain;
        }

        /// <summary>
        /// Fills a rectangle with terrain
        /// </summary>
        public void FillRect(int x1, int y1, int x2, int y2, LocalTerrainType terrain)
        {
            for (int y = y1; y <= y2; y++)
                for (int x = x1; x <= x2; x++)
                    SetTerrain(x, y, terrain);
        }

        /// <summary>
        /// Draws rectangle outline with terrain
        /// </summary>
        public void DrawRect(int x1, int y1, int x2, int y2, LocalTerrainType terrain)
        {
            for (int x = x1; x <= x2; x++)
            {
                SetTerrain(x, y1, terrain);
                SetTerrain(x, y2, terrain);
            }
            for (int y = y1; y <= y2; y++)
            {
                SetTerrain(x1, y, terrain);
                SetTerrain(x2, y, terrain);
            }
        }

        /// <summary>
        /// Scatter terrain randomly
        /// </summary>
        public void Scatter(LocalTerrainType terrain, int count)
        {
            for (int i = 0; i < count; i++)
            {
                int x = _rng.Next(MapSize);
                int y = _rng.Next(MapSize);
                SetTerrain(x, y, terrain);
            }
        }

        public string ToAscii()
        {
            var sb = new StringBuilder();
            for (int y = 0; y < MapSize; y++)
            {
                for (int x = 0; x < MapSize; x++)
                {
                    sb.Append(_tiles[x, y].GetSymbol());
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
