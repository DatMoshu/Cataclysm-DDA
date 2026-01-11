using System;
using System.Collections.Generic;
using CataMapGen.Mapgen.Data;

namespace CataMapGen.Mapgen
{
    /// <summary>
    /// Resolves palette inheritance chain and merges all symbol mappings
    /// </summary>
    public class PaletteResolver
    {
        private readonly MapgenRegistry _registry;
        private readonly HashSet<string> _resolving = new HashSet<string>();

        public PaletteResolver(MapgenRegistry registry)
        {
            _registry = registry;
        }

        /// <summary>
        /// Resolve all palettes for a mapgen definition into a single merged palette
        /// Applies palettes in order, then applies definition overrides
        /// </summary>
        public MapgenPalette Resolve(MapgenDefinition def)
        {
            var result = new MapgenPalette { Id = "_resolved_" };

            if (def.Object == null) return result;

            // Apply palettes in order
            foreach (var paletteId in def.Object.Palettes)
            {
                var resolved = ResolvePalette(paletteId);
                if (resolved != null)
                    result.MergeFrom(resolved);
            }

            // Apply definition-level overrides
            foreach (var kvp in def.Object.Terrain)
                result.Terrain[kvp.Key] = kvp.Value;

            foreach (var kvp in def.Object.Furniture)
                result.Furniture[kvp.Key] = kvp.Value;

            foreach (var kvp in def.Object.Items)
            {
                if (!result.Items.ContainsKey(kvp.Key))
                    result.Items[kvp.Key] = new List<ItemSpawnRule>();
                result.Items[kvp.Key].AddRange(kvp.Value);
            }

            return result;
        }

        /// <summary>
        /// Resolve a single palette with its inheritance chain
        /// </summary>
        public MapgenPalette ResolvePalette(string paletteId)
        {
            if (string.IsNullOrEmpty(paletteId))
                return null;

            // Prevent infinite recursion
            if (_resolving.Contains(paletteId))
            {
                Console.WriteLine($"Warning: Circular palette reference detected: {paletteId}");
                return null;
            }

            var palette = _registry.GetPalette(paletteId);
            if (palette == null)
                return null;

            // If no inheritance, return as-is
            if (palette.Palettes == null || palette.Palettes.Count == 0)
                return palette;

            // Resolve with inheritance
            _resolving.Add(paletteId);
            try
            {
                var result = new MapgenPalette { Id = palette.Id };

                // First apply all parent palettes
                foreach (var parentId in palette.Palettes)
                {
                    var parent = ResolvePalette(parentId);
                    if (parent != null)
                        result.MergeFrom(parent);
                }

                // Then apply this palette's own definitions
                result.MergeFrom(palette);

                return result;
            }
            finally
            {
                _resolving.Remove(paletteId);
            }
        }
    }
}
