using System;
using System.Collections.Generic;

namespace CataMapGen.Mapgen.Data
{
    /// <summary>
    /// Mapgen palette matching CDDA's palette format
    /// Defines symbol->terrain/furniture/item mappings that can be inherited
    /// </summary>
    [Serializable]
    public class MapgenPalette
    {
        /// <summary>Palette ID for inheritance</summary>
        public string Id { get; set; }

        /// <summary>List of palette IDs to inherit from (applied in order)</summary>
        public List<string> Palettes { get; set; } = new List<string>();

        /// <summary>Symbol->terrain mappings (can be single ID or weighted list)</summary>
        public Dictionary<char, WeightedEntry<string>> Terrain { get; set; } = new Dictionary<char, WeightedEntry<string>>();

        /// <summary>Symbol->furniture mappings</summary>
        public Dictionary<char, WeightedEntry<string>> Furniture { get; set; } = new Dictionary<char, WeightedEntry<string>>();

        /// <summary>Symbol->item spawn rules</summary>
        public Dictionary<char, List<ItemSpawnRule>> Items { get; set; } = new Dictionary<char, List<ItemSpawnRule>>();

        /// <summary>Toilets placement (CDDA-specific)</summary>
        public Dictionary<char, object> Toilets { get; set; } = new Dictionary<char, object>();

        /// <summary>Liquids placement</summary>
        public Dictionary<char, object> Liquids { get; set; } = new Dictionary<char, object>();

        /// <summary>
        /// Merge another palette into this one (used for inheritance)
        /// Later values override earlier ones
        /// </summary>
        public void MergeFrom(MapgenPalette other)
        {
            foreach (var kvp in other.Terrain)
                Terrain[kvp.Key] = kvp.Value;

            foreach (var kvp in other.Furniture)
                Furniture[kvp.Key] = kvp.Value;

            foreach (var kvp in other.Items)
            {
                if (!Items.ContainsKey(kvp.Key))
                    Items[kvp.Key] = new List<ItemSpawnRule>();
                Items[kvp.Key].AddRange(kvp.Value);
            }

            foreach (var kvp in other.Toilets)
                Toilets[kvp.Key] = kvp.Value;

            foreach (var kvp in other.Liquids)
                Liquids[kvp.Key] = kvp.Value;
        }

        /// <summary>
        /// Get terrain for symbol, or null if not defined
        /// </summary>
        public string GetTerrain(char symbol, Random rng)
        {
            if (Terrain.TryGetValue(symbol, out var entry))
                return entry.Pick(rng);
            return null;
        }

        /// <summary>
        /// Get furniture for symbol, or null if not defined
        /// </summary>
        public string GetFurniture(char symbol, Random rng)
        {
            if (Furniture.TryGetValue(symbol, out var entry))
                return entry.Pick(rng);
            return null;
        }

        /// <summary>
        /// Get item spawn rules for symbol
        /// </summary>
        public List<ItemSpawnRule> GetItemRules(char symbol)
        {
            if (Items.TryGetValue(symbol, out var rules))
                return rules;
            return null;
        }
    }
}
