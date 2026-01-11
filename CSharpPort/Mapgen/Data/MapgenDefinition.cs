using System;
using System.Collections.Generic;

namespace CataMapGen.Mapgen.Data
{
    /// <summary>
    /// Mapgen definition matching CDDA's JSON mapgen format exactly
    /// Represents a single building/terrain definition
    /// </summary>
    [Serializable]
    public class MapgenDefinition
    {
        /// <summary>Method type - "json" for data-driven</summary>
        public string Method { get; set; } = "json";

        /// <summary>Type identifier - "mapgen" for map generation</summary>
        public string Type { get; set; } = "mapgen";

        /// <summary>Overmap terrain ID this definition applies to</summary>
        public string OmTerrain { get; set; }

        /// <summary>Selection weight (higher = more likely to be chosen)</summary>
        public int Weight { get; set; } = 100;

        /// <summary>Comment/documentation</summary>
        public string Comment { get; set; }

        /// <summary>The actual mapgen object containing all data</summary>
        public MapgenObject Object { get; set; }
    }

    /// <summary>
    /// The "object" part of a mapgen definition
    /// Contains the actual map data: rows, palettes, terrain, furniture, items
    /// </summary>
    [Serializable]
    public class MapgenObject
    {
        /// <summary>Default terrain to fill map with</summary>
        public string FillTer { get; set; }

        /// <summary>24 rows of 24 characters defining the map layout</summary>
        public string[] Rows { get; set; }

        /// <summary>List of palette IDs to apply (in order)</summary>
        public List<string> Palettes { get; set; } = new List<string>();

        /// <summary>Terrain symbol overrides (applied after palettes)</summary>
        public Dictionary<char, WeightedEntry<string>> Terrain { get; set; } = new Dictionary<char, WeightedEntry<string>>();

        /// <summary>Furniture symbol overrides</summary>
        public Dictionary<char, WeightedEntry<string>> Furniture { get; set; } = new Dictionary<char, WeightedEntry<string>>();

        /// <summary>Item spawn rules per symbol</summary>
        public Dictionary<char, List<ItemSpawnRule>> Items { get; set; } = new Dictionary<char, List<ItemSpawnRule>>();

        /// <summary>Specific loot placements</summary>
        public List<PlaceLootRule> PlaceLoot { get; set; } = new List<PlaceLootRule>();

        /// <summary>Nested chunk placements</summary>
        public List<PlaceNestedRule> PlaceNested { get; set; } = new List<PlaceNestedRule>();

        /// <summary>Monster spawn placements</summary>
        public List<MonsterSpawnRule> PlaceMonsters { get; set; } = new List<MonsterSpawnRule>();

        /// <summary>Toilet placements</summary>
        public Dictionary<char, object> Toilets { get; set; } = new Dictionary<char, object>();

        /// <summary>Liquid placements</summary>
        public Dictionary<char, object> Liquids { get; set; } = new Dictionary<char, object>();

        /// <summary>Rotation (0-3 for 90-degree increments)</summary>
        public int Rotation { get; set; } = 0;

        /// <summary>
        /// Validate that rows are correct size (24x24)
        /// </summary>
        public bool ValidateRows()
        {
            if (Rows == null || Rows.Length != 24)
                return false;

            foreach (var row in Rows)
            {
                if (row == null || row.Length != 24)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Get character at map position
        /// </summary>
        public char GetSymbolAt(int x, int y)
        {
            if (Rows == null || y < 0 || y >= Rows.Length)
                return ' ';

            var row = Rows[y];
            if (x < 0 || x >= row.Length)
                return ' ';

            return row[x];
        }
    }
}
