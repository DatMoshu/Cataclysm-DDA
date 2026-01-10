using System;
using CataMapGen.Core;

namespace CataMapGen.Mapgen
{
    /// <summary>
    /// Terrain types for local map tiles
    /// </summary>
    public enum LocalTerrainType
    {
        // Ground
        Grass,
        Dirt,
        Pavement,
        Concrete,
        
        // Natural
        Tree,
        Shrub,
        Water,
        DeepWater,
        Swamp,
        Sand,
        
        // Buildings
        Wall,
        Floor,
        Window,
        Door,
        
        // Features
        Fence,
        Sidewalk,
        Road
    }

    /// <summary>
    /// Furniture types (placed on terrain)
    /// </summary>
    public enum FurnitureType
    {
        None,
        Table,
        Chair,
        Bed,
        Counter,
        Rack,
        Toilet,
        Fridge,
        Stove,
        Dresser
    }

    /// <summary>
    /// A single tile in a local map (24x24 submap)
    /// </summary>
    [Serializable]
    public class LocalTile
    {
        public LocalTerrainType Terrain;
        public FurnitureType Furniture;

        public LocalTile()
        {
            Terrain = LocalTerrainType.Grass;
            Furniture = FurnitureType.None;
        }

        public LocalTile(LocalTerrainType terrain) : this()
        {
            Terrain = terrain;
        }

        public bool IsPassable()
        {
            switch (Terrain)
            {
                case LocalTerrainType.Wall:
                case LocalTerrainType.DeepWater:
                case LocalTerrainType.Tree:
                    return false;
                default:
                    return true;
            }
        }

        public char GetSymbol()
        {
            // Furniture first
            if (Furniture != FurnitureType.None)
            {
                switch (Furniture)
                {
                    case FurnitureType.Table: return 'T';
                    case FurnitureType.Chair: return 'h';
                    case FurnitureType.Bed: return '#';
                    case FurnitureType.Counter: return '|';
                    case FurnitureType.Rack: return '}';
                    case FurnitureType.Toilet: return '&';
                    case FurnitureType.Fridge: return '[';
                    case FurnitureType.Stove: return ']';
                    case FurnitureType.Dresser: return '{';
                }
            }

            switch (Terrain)
            {
                case LocalTerrainType.Grass: return '.';
                case LocalTerrainType.Dirt: return ',';
                case LocalTerrainType.Pavement: return '_';
                case LocalTerrainType.Concrete: return '#';
                case LocalTerrainType.Tree: return 'T';
                case LocalTerrainType.Shrub: return '*';
                case LocalTerrainType.Water: return '~';
                case LocalTerrainType.DeepWater: return '≈';
                case LocalTerrainType.Swamp: return '&';
                case LocalTerrainType.Sand: return ':';
                case LocalTerrainType.Wall: return '█';
                case LocalTerrainType.Floor: return '.';
                case LocalTerrainType.Window: return '"';
                case LocalTerrainType.Door: return '+';
                case LocalTerrainType.Fence: return '-';
                case LocalTerrainType.Sidewalk: return '_';
                case LocalTerrainType.Road: return '#';
                default: return '?';
            }
        }
    }
}
