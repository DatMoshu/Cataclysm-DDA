using System;
using CataMapGen.Core;
using CataMapGen.Noise;
using CataMapGen.Overmap;

namespace CataMapGen.Mapgen
{
    /// <summary>
    /// Generates local map content based on overmap terrain type
    /// Matches CDDA's mapgen_function system
    /// </summary>
    public static class TerrainGenerator
    {
        /// <summary>
        /// Generate a local map for the given mapgen data
        /// </summary>
        public static LocalMap Generate(MapgenData data)
        {
            var map = new LocalMap(data.Seed);

            switch (data.OvermapTerrain)
            {
                case OvermapTerrainType.Field:
                    GenerateField(map, data);
                    break;
                case OvermapTerrainType.Forest:
                case OvermapTerrainType.ForestThick:
                    GenerateForest(map, data);
                    break;
                case OvermapTerrainType.ForestWater:
                    GenerateForestWater(map, data);
                    break;
                case OvermapTerrainType.River:
                case OvermapTerrainType.RiverCenter:
                    GenerateRiver(map, data);
                    break;
                case OvermapTerrainType.Lake:
                case OvermapTerrainType.LakeShore:
                    GenerateLake(map, data);
                    break;
                case OvermapTerrainType.Swamp:
                    GenerateSwamp(map, data);
                    break;
                case OvermapTerrainType.Road:
                    GenerateRoad(map, data);
                    break;
                case OvermapTerrainType.Residential:
                    GenerateHouse(map, data);
                    break;
                case OvermapTerrainType.Commercial:
                    GenerateShop(map, data);
                    break;
                default:
                    GenerateField(map, data);
                    break;
            }

            return map;
        }

        private static void GenerateField(LocalMap map, MapgenData data)
        {
            var rng = new Random((int)data.Seed);
            map.Fill(LocalTerrainType.Grass);
            
            // Random grass variation
            for (int i = 0; i < 30; i++)
            {
                int x = rng.Next(LocalMap.MapSize);
                int y = rng.Next(LocalMap.MapSize);
                map.SetTerrain(x, y, LocalTerrainType.Dirt);
            }

            // Occasional shrubs
            map.Scatter(LocalTerrainType.Shrub, rng.Next(3, 8));
        }

        private static void GenerateForest(LocalMap map, MapgenData data)
        {
            var rng = new Random((int)data.Seed);
            bool thick = data.OvermapTerrain == OvermapTerrainType.ForestThick;

            map.Fill(LocalTerrainType.Grass);

            // Tree density based on thickness
            int treeCount = thick ? rng.Next(80, 120) : rng.Next(40, 70);
            
            for (int i = 0; i < treeCount; i++)
            {
                int x = rng.Next(LocalMap.MapSize);
                int y = rng.Next(LocalMap.MapSize);
                map.SetTerrain(x, y, LocalTerrainType.Tree);
            }

            // Underbrush
            int shrubCount = thick ? rng.Next(20, 40) : rng.Next(10, 20);
            map.Scatter(LocalTerrainType.Shrub, shrubCount);
        }

        private static void GenerateForestWater(LocalMap map, MapgenData data)
        {
            GenerateForest(map, data);
            
            var rng = new Random((int)data.Seed + 1);
            
            // Add swampy water patches
            int patchCount = rng.Next(2, 5);
            for (int p = 0; p < patchCount; p++)
            {
                int cx = rng.Next(4, LocalMap.MapSize - 4);
                int cy = rng.Next(4, LocalMap.MapSize - 4);
                int size = rng.Next(2, 4);

                for (int dy = -size; dy <= size; dy++)
                {
                    for (int dx = -size; dx <= size; dx++)
                    {
                        if (dx * dx + dy * dy <= size * size)
                        {
                            map.SetTerrain(cx + dx, cy + dy, LocalTerrainType.Water);
                        }
                    }
                }
            }
        }

        private static void GenerateRiver(LocalMap map, MapgenData data)
        {
            map.Fill(LocalTerrainType.Water);

            var rng = new Random((int)data.Seed);

            // Add deep water in center
            map.FillRect(6, 6, 17, 17, LocalTerrainType.DeepWater);

            // Shore variation at edges
            for (int i = 0; i < 20; i++)
            {
                int edge = rng.Next(4);
                int pos = rng.Next(LocalMap.MapSize);
                switch (edge)
                {
                    case 0: map.SetTerrain(pos, 0, LocalTerrainType.Grass); break;
                    case 1: map.SetTerrain(pos, LocalMap.MapSize - 1, LocalTerrainType.Grass); break;
                    case 2: map.SetTerrain(0, pos, LocalTerrainType.Grass); break;
                    case 3: map.SetTerrain(LocalMap.MapSize - 1, pos, LocalTerrainType.Grass); break;
                }
            }
        }

        private static void GenerateLake(LocalMap map, MapgenData data)
        {
            bool isShore = data.OvermapTerrain == OvermapTerrainType.LakeShore;

            if (isShore)
            {
                map.Fill(LocalTerrainType.Grass);
                // Water on one side
                var rng = new Random((int)data.Seed);
                int side = rng.Next(4);
                switch (side)
                {
                    case 0: map.FillRect(0, 0, LocalMap.MapSize - 1, 11, LocalTerrainType.Water); break;
                    case 1: map.FillRect(0, 12, LocalMap.MapSize - 1, LocalMap.MapSize - 1, LocalTerrainType.Water); break;
                    case 2: map.FillRect(0, 0, 11, LocalMap.MapSize - 1, LocalTerrainType.Water); break;
                    case 3: map.FillRect(12, 0, LocalMap.MapSize - 1, LocalMap.MapSize - 1, LocalTerrainType.Water); break;
                }
            }
            else
            {
                map.Fill(LocalTerrainType.DeepWater);
            }
        }

        private static void GenerateSwamp(LocalMap map, MapgenData data)
        {
            var rng = new Random((int)data.Seed);
            
            map.Fill(LocalTerrainType.Swamp);
            
            // Scattered trees
            map.Scatter(LocalTerrainType.Tree, rng.Next(15, 30));
            
            // Water pools
            for (int i = 0; i < 5; i++)
            {
                int cx = rng.Next(2, LocalMap.MapSize - 2);
                int cy = rng.Next(2, LocalMap.MapSize - 2);
                int size = rng.Next(1, 3);
                for (int dy = -size; dy <= size; dy++)
                    for (int dx = -size; dx <= size; dx++)
                        if (dx * dx + dy * dy <= size * size)
                            map.SetTerrain(cx + dx, cy + dy, LocalTerrainType.Water);
            }
        }

        private static void GenerateRoad(LocalMap map, MapgenData data)
        {
            map.Fill(LocalTerrainType.Grass);
            
            // Road through center
            int roadWidth = 4;
            int start = LocalMap.MapSize / 2 - roadWidth / 2;
            
            // Assume N-S road (should check neighbors for actual direction)
            map.FillRect(start, 0, start + roadWidth - 1, LocalMap.MapSize - 1, LocalTerrainType.Road);
            
            // Sidewalks
            map.FillRect(start - 1, 0, start - 1, LocalMap.MapSize - 1, LocalTerrainType.Sidewalk);
            map.FillRect(start + roadWidth, 0, start + roadWidth, LocalMap.MapSize - 1, LocalTerrainType.Sidewalk);
        }

        private static void GenerateHouse(LocalMap map, MapgenData data)
        {
            var rng = new Random((int)data.Seed);
            
            map.Fill(LocalTerrainType.Grass);
            
            // House dimensions
            int houseW = rng.Next(10, 16);
            int houseH = rng.Next(8, 12);
            int houseX = (LocalMap.MapSize - houseW) / 2;
            int houseY = (LocalMap.MapSize - houseH) / 2;

            // Walls
            map.DrawRect(houseX, houseY, houseX + houseW - 1, houseY + houseH - 1, LocalTerrainType.Wall);
            
            // Floor inside
            map.FillRect(houseX + 1, houseY + 1, houseX + houseW - 2, houseY + houseH - 2, LocalTerrainType.Floor);
            
            // Door on south wall
            int doorX = houseX + houseW / 2;
            map.SetTerrain(doorX, houseY + houseH - 1, LocalTerrainType.Door);
            
            // Windows
            map.SetTerrain(houseX + 2, houseY, LocalTerrainType.Window);
            map.SetTerrain(houseX + houseW - 3, houseY, LocalTerrainType.Window);
            map.SetTerrain(houseX, houseY + 2, LocalTerrainType.Window);
            map.SetTerrain(houseX + houseW - 1, houseY + 2, LocalTerrainType.Window);
            
            // Furniture
            map.SetFurniture(houseX + 2, houseY + 2, FurnitureType.Bed);
            map.SetFurniture(houseX + houseW - 3, houseY + 2, FurnitureType.Dresser);
            map.SetFurniture(houseX + houseW / 2, houseY + 1, FurnitureType.Table);
            map.SetFurniture(houseX + houseW / 2 - 1, houseY + 2, FurnitureType.Chair);
            map.SetFurniture(houseX + houseW / 2 + 1, houseY + 2, FurnitureType.Chair);
        }

        private static void GenerateShop(LocalMap map, MapgenData data)
        {
            var rng = new Random((int)data.Seed);
            
            map.Fill(LocalTerrainType.Pavement);
            
            // Shop dimensions
            int shopW = rng.Next(14, 20);
            int shopH = rng.Next(10, 14);
            int shopX = (LocalMap.MapSize - shopW) / 2;
            int shopY = (LocalMap.MapSize - shopH) / 2;

            // Walls
            map.DrawRect(shopX, shopY, shopX + shopW - 1, shopY + shopH - 1, LocalTerrainType.Wall);
            
            // Floor
            map.FillRect(shopX + 1, shopY + 1, shopX + shopW - 2, shopY + shopH - 2, LocalTerrainType.Floor);
            
            // Glass front (south wall)
            for (int x = shopX + 2; x < shopX + shopW - 2; x++)
            {
                map.SetTerrain(x, shopY + shopH - 1, LocalTerrainType.Window);
            }
            
            // Door
            int doorX = shopX + shopW / 2;
            map.SetTerrain(doorX, shopY + shopH - 1, LocalTerrainType.Door);
            
            // Counter
            for (int x = shopX + 2; x < shopX + shopW - 2; x++)
            {
                map.SetFurniture(x, shopY + shopH - 4, FurnitureType.Counter);
            }
            
            // Shelves
            for (int y = shopY + 2; y < shopY + shopH - 5; y += 2)
            {
                for (int x = shopX + 3; x < shopX + shopW - 3; x += 3)
                {
                    map.SetFurniture(x, y, FurnitureType.Rack);
                }
            }
        }
    }
}
