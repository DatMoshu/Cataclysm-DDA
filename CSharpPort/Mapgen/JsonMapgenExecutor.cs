using System;
using CataMapGen.Core;
using CataMapGen.Mapgen.Data;

namespace CataMapGen.Mapgen
{
    /// <summary>
    /// Executes a mapgen definition to generate a LocalMap
    /// </summary>
    public class JsonMapgenExecutor
    {
        private readonly MapgenRegistry _registry;
        private readonly PaletteResolver _paletteResolver;

        public JsonMapgenExecutor(MapgenRegistry registry)
        {
            _registry = registry;
            _paletteResolver = new PaletteResolver(registry);
        }

        /// <summary>
        /// Execute a mapgen definition and populate the given LocalMap
        /// </summary>
        public void Execute(MapgenDefinition def, LocalMap map, Random rng)
        {
            if (def?.Object == null) return;

            var obj = def.Object;

            // Resolve all palettes into one merged palette
            var palette = _paletteResolver.Resolve(def);

            // Fill with default terrain if specified
            if (!string.IsNullOrEmpty(obj.FillTer))
            {
                var fillTerrain = ParseTerrainType(obj.FillTer);
                for (int y = 0; y < map.Height; y++)
                {
                    for (int x = 0; x < map.Width; x++)
                    {
                        map.SetTerrain(x, y, fillTerrain);
                    }
                }
            }

            // Apply rows
            if (obj.Rows != null)
            {
                int mapSize = Math.Min(map.Width, map.Height);
                int rowCount = Math.Min(obj.Rows.Length, mapSize);

                for (int y = 0; y < rowCount; y++)
                {
                    string row = obj.Rows[y];
                    int colCount = Math.Min(row.Length, mapSize);

                    for (int x = 0; x < colCount; x++)
                    {
                        char symbol = row[x];
                        ApplySymbol(map, x, y, symbol, palette, rng);
                    }
                }
            }

            // Apply place_loot
            foreach (var loot in obj.PlaceLoot)
            {
                // TODO: Implement item spawning when item system exists
            }
        }

        /// <summary>
        /// Apply a single symbol at position using palette
        /// </summary>
        private void ApplySymbol(LocalMap map, int x, int y, char symbol, MapgenPalette palette, Random rng)
        {
            // Get terrain from palette
            var terrainId = palette.GetTerrain(symbol, rng);
            if (terrainId != null)
            {
                var terrain = ParseTerrainType(terrainId);
                map.SetTerrain(x, y, terrain);
            }

            // Get furniture from palette
            var furnitureId = palette.GetFurniture(symbol, rng);
            if (furnitureId != null)
            {
                var furniture = ParseFurnitureType(furnitureId);
                map.SetFurniture(x, y, furniture);
            }

            // Item spawns would go here
        }

        /// <summary>
        /// Parse terrain string to enum
        /// Supports both CDDA format (t_floor) and simplified format (Floor)
        /// </summary>
        private LocalTerrainType ParseTerrainType(string id)
        {
            if (string.IsNullOrEmpty(id)) return LocalTerrainType.Floor;

            // Remove CDDA prefix if present
            if (id.StartsWith("t_"))
                id = id.Substring(2);

            // Map common CDDA terrain IDs
            switch (id.ToLowerInvariant())
            {
                case "floor":
                case "floor_waxed":
                case "thconc_floor":
                case "linoleum_white":
                case "linoleum_gray":
                    return LocalTerrainType.Floor;

                case "wall":
                case "wall_w":
                case "wall_wood":
                case "concrete_wall":
                    return LocalTerrainType.Wall;

                case "window":
                case "window_domestic":
                case "window_no_curtains":
                case "curtains":
                    return LocalTerrainType.Window;

                case "door_c":
                case "door_locked":
                case "door_o":
                case "door_elocked":
                    return LocalTerrainType.DoorClosed;

                case "door_open":
                    return LocalTerrainType.DoorOpen;

                case "sidewalk":
                case "pavement":
                case "concrete":
                    return LocalTerrainType.Pavement;

                case "grass":
                case "region_groundcover":
                case "region_groundcover_urban":
                    return LocalTerrainType.Grass;

                case "dirt":
                case "soil":
                case "region_soil":
                    return LocalTerrainType.Dirt;

                case "water_dp":
                case "water_sh":
                case "water_pool":
                    return LocalTerrainType.Water;

                case "tree":
                case "region_tree":
                case "region_tree_fruit":
                case "region_tree_shade":
                    return LocalTerrainType.Tree;

                case "shrub":
                case "region_shrub":
                case "region_shrub_decorative":
                    return LocalTerrainType.Shrub;

                case "privacy_fence":
                case "fence":
                case "chainfence":
                case "splitrail_fence":
                    return LocalTerrainType.Wall;

                default:
                    return LocalTerrainType.Floor;
            }
        }

        /// <summary>
        /// Parse furniture string to enum
        /// </summary>
        private LocalFurnitureType ParseFurnitureType(string id)
        {
            if (string.IsNullOrEmpty(id)) return LocalFurnitureType.None;

            // Remove CDDA prefix if present
            if (id.StartsWith("f_"))
                id = id.Substring(2);

            switch (id.ToLowerInvariant())
            {
                case "bed":
                    return LocalFurnitureType.Bed;

                case "table":
                case "counter":
                case "coffee_table":
                case "desk":
                    return LocalFurnitureType.Table;

                case "chair":
                case "armchair":
                case "stool":
                case "sofa":
                    return LocalFurnitureType.Chair;

                case "dresser":
                case "wardrobe":
                case "cupboard":
                case "bookcase":
                case "rack":
                case "rack_wood":
                    return LocalFurnitureType.Shelf;

                case "toilet":
                    return LocalFurnitureType.Toilet;

                case "shower":
                case "bathtub":
                    return LocalFurnitureType.Bathtub;

                case "sink":
                    return LocalFurnitureType.Sink;

                case "oven":
                case "gas_oven_microwave_combo":
                    return LocalFurnitureType.Oven;

                case "fridge":
                case "glass_fridge":
                case "freezer":
                    return LocalFurnitureType.Fridge;

                case "trashcan":
                    return LocalFurnitureType.Trashcan;

                case "crate":
                case "cardboard_box":
                case "chest":
                case "locker":
                    return LocalFurnitureType.Crate;

                default:
                    return LocalFurnitureType.None;
            }
        }
    }
}
