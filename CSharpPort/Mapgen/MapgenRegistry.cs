using System;
using System.Collections.Generic;
using CataMapGen.Mapgen.Data;

namespace CataMapGen.Mapgen
{
    /// <summary>
    /// Registry/lookup service for mapgen definitions and palettes
    /// Stores all loaded definitions and provides weighted random selection
    /// </summary>
    public class MapgenRegistry
    {
        /// <summary>Definitions indexed by om_terrain</summary>
        private readonly Dictionary<string, List<MapgenDefinition>> _byOmTerrain = new Dictionary<string, List<MapgenDefinition>>();

        /// <summary>Palettes indexed by ID</summary>
        private readonly Dictionary<string, MapgenPalette> _palettes = new Dictionary<string, MapgenPalette>();

        /// <summary>Nested chunks indexed by ID</summary>
        private readonly Dictionary<string, MapgenDefinition> _nestedChunks = new Dictionary<string, MapgenDefinition>();

        /// <summary>
        /// Register a mapgen definition
        /// </summary>
        public void RegisterDefinition(MapgenDefinition def)
        {
            if (string.IsNullOrEmpty(def.OmTerrain))
                return;

            if (!_byOmTerrain.TryGetValue(def.OmTerrain, out var list))
            {
                list = new List<MapgenDefinition>();
                _byOmTerrain[def.OmTerrain] = list;
            }

            list.Add(def);
        }

        /// <summary>
        /// Register a palette
        /// </summary>
        public void RegisterPalette(MapgenPalette palette)
        {
            if (!string.IsNullOrEmpty(palette.Id))
                _palettes[palette.Id] = palette;
        }

        /// <summary>
        /// Register a nested chunk definition
        /// </summary>
        public void RegisterNestedChunk(string id, MapgenDefinition def)
        {
            if (!string.IsNullOrEmpty(id))
                _nestedChunks[id] = def;
        }

        /// <summary>
        /// Pick a random definition for the given om_terrain using weighted selection
        /// </summary>
        public MapgenDefinition Pick(string omTerrain, Random rng)
        {
            if (!_byOmTerrain.TryGetValue(omTerrain, out var list) || list.Count == 0)
                return null;

            // Calculate total weight
            int totalWeight = 0;
            foreach (var def in list)
                totalWeight += def.Weight;

            if (totalWeight == 0)
                return list[rng.Next(list.Count)];

            // Weighted selection
            int roll = rng.Next(totalWeight);
            int cumulative = 0;

            foreach (var def in list)
            {
                cumulative += def.Weight;
                if (roll < cumulative)
                    return def;
            }

            return list[list.Count - 1];
        }

        /// <summary>
        /// Get all definitions for an om_terrain
        /// </summary>
        public IReadOnlyList<MapgenDefinition> GetDefinitions(string omTerrain)
        {
            if (_byOmTerrain.TryGetValue(omTerrain, out var list))
                return list;
            return Array.Empty<MapgenDefinition>();
        }

        /// <summary>
        /// Get a palette by ID
        /// </summary>
        public MapgenPalette GetPalette(string id)
        {
            if (_palettes.TryGetValue(id, out var palette))
                return palette;
            return null;
        }

        /// <summary>
        /// Get a nested chunk by ID
        /// </summary>
        public MapgenDefinition GetNestedChunk(string id)
        {
            if (_nestedChunks.TryGetValue(id, out var def))
                return def;
            return null;
        }

        /// <summary>
        /// Check if definitions exist for an om_terrain
        /// </summary>
        public bool HasDefinitions(string omTerrain)
        {
            return _byOmTerrain.ContainsKey(omTerrain) && _byOmTerrain[omTerrain].Count > 0;
        }

        /// <summary>
        /// Get count of registered definitions
        /// </summary>
        public int DefinitionCount
        {
            get
            {
                int count = 0;
                foreach (var list in _byOmTerrain.Values)
                    count += list.Count;
                return count;
            }
        }

        /// <summary>
        /// Get count of registered palettes
        /// </summary>
        public int PaletteCount => _palettes.Count;

        /// <summary>
        /// Clear all registered items
        /// </summary>
        public void Clear()
        {
            _byOmTerrain.Clear();
            _palettes.Clear();
            _nestedChunks.Clear();
        }
    }
}
