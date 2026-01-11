using System;
using System.Collections.Generic;

namespace CataMapGen.Mapgen.Data
{
    /// <summary>
    /// Item spawn rule matching CDDA's place_loot/items format
    /// { "item": "clothing", "chance": 50, "repeat": [1, 3] }
    /// </summary>
    [Serializable]
    public class ItemSpawnRule
    {
        public string Item { get; set; }
        public int Chance { get; set; } = 100;
        public int RepeatMin { get; set; } = 1;
        public int RepeatMax { get; set; } = 1;
        public int Ammo { get; set; } = 0;
        public int Magazine { get; set; } = 0;

        public ItemSpawnRule() { }

        public ItemSpawnRule(string item, int chance = 100, int repeatMin = 1, int repeatMax = 1)
        {
            Item = item;
            Chance = chance;
            RepeatMin = repeatMin;
            RepeatMax = repeatMax;
        }

        /// <summary>
        /// Check if item should spawn based on chance
        /// </summary>
        public bool ShouldSpawn(Random rng)
        {
            return rng.Next(100) < Chance;
        }

        /// <summary>
        /// Get random repeat count
        /// </summary>
        public int GetRepeatCount(Random rng)
        {
            return rng.Next(RepeatMin, RepeatMax + 1);
        }
    }

    /// <summary>
    /// Weighted item or terrain entry matching CDDA format
    /// Can be a single value or weighted distribution
    /// </summary>
    [Serializable]
    public class WeightedEntry<T>
    {
        public List<(T Value, int Weight)> Options { get; set; } = new List<(T, int)>();

        public WeightedEntry() { }

        public WeightedEntry(T singleValue)
        {
            Options.Add((singleValue, 100));
        }

        public T Pick(Random rng)
        {
            if (Options.Count == 0) return default;
            if (Options.Count == 1) return Options[0].Value;

            int totalWeight = 0;
            foreach (var opt in Options)
                totalWeight += opt.Weight;

            int roll = rng.Next(totalWeight);
            int cumulative = 0;

            foreach (var opt in Options)
            {
                cumulative += opt.Weight;
                if (roll < cumulative) return opt.Value;
            }

            return Options[Options.Count - 1].Value;
        }
    }

    /// <summary>
    /// Monster spawn placement matching CDDA format
    /// { "monster": "GROUP_ZOMBIE", "x": [11, 13], "y": [12, 13], "repeat": [1, 2] }
    /// </summary>
    [Serializable]
    public class MonsterSpawnRule
    {
        public string Monster { get; set; }
        public int XMin { get; set; }
        public int XMax { get; set; }
        public int YMin { get; set; }
        public int YMax { get; set; }
        public int RepeatMin { get; set; } = 1;
        public int RepeatMax { get; set; } = 1;
    }

    /// <summary>
    /// Place loot at specific location matching CDDA format
    /// { "item": "television", "x": 11, "y": 9 }
    /// </summary>
    [Serializable]
    public class PlaceLootRule
    {
        public string Item { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Chance { get; set; } = 100;
        public int RepeatMin { get; set; } = 1;
        public int RepeatMax { get; set; } = 1;
    }

    /// <summary>
    /// Nested chunk placement matching CDDA format
    /// { "chunks": [["shed_6x6_junk", 15], ["null", 85]], "x": 4, "y": 17 }
    /// </summary>
    [Serializable]
    public class PlaceNestedRule
    {
        public List<(string ChunkId, int Weight)> Chunks { get; set; } = new List<(string, int)>();
        public int XMin { get; set; }
        public int XMax { get; set; }
        public int YMin { get; set; }
        public int YMax { get; set; }

        public string PickChunk(Random rng)
        {
            int totalWeight = 0;
            foreach (var chunk in Chunks)
                totalWeight += chunk.Weight;

            if (totalWeight == 0) return null;

            int roll = rng.Next(totalWeight);
            int cumulative = 0;

            foreach (var chunk in Chunks)
            {
                cumulative += chunk.Weight;
                if (roll < cumulative) return chunk.ChunkId;
            }

            return Chunks[Chunks.Count - 1].ChunkId;
        }
    }
}
