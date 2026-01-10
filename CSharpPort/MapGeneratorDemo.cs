using System;
using System.IO;
using CataMapGen.Core;
using CataMapGen.Overmap;
using CataMapGen.Mapgen;

namespace CataMapGen
{
    /// <summary>
    /// Demo/test class for the CDDA map generation port
    /// Can be run as console app or called from Unity
    /// </summary>
    public static class MapGeneratorDemo
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== CDDA Map Generation Demo ===\n");

            uint seed = (uint)DateTime.Now.Ticks;
            if (args.Length > 0 && uint.TryParse(args[0], out uint parsedSeed))
                seed = parsedSeed;

            Console.WriteLine($"Using seed: {seed}\n");

            // Generate overmap using the new orchestrator
            Console.WriteLine("Generating overmap (180x180)...");
            var settings = new OvermapSettings
            {
                MinCities = 2,
                MaxCities = 5,
                MinCitySize = 5,
                MaxCitySize = 18
            };

            var overmap = OvermapGenerator.Create(Point2D.Zero, seed, settings);

            // Print legend
            Console.WriteLine(CataMapGen.Overmap.Overmap.GetLegend());
            Console.WriteLine();

            // Save to file
            string overmapPath = "overmap_output.txt";
            File.WriteAllText(overmapPath, overmap.ToAscii());
            Console.WriteLine($"Overmap saved to: {overmapPath}");

            // Show city info
            Console.WriteLine($"\nGenerated {overmap.Cities.Count} cities:");
            foreach (var city in overmap.Cities)
                Console.WriteLine($"  - {city.Name} at {city.Position}, size {city.Size}");

            // Generate a few local maps
            Console.WriteLine("\n--- Local Map Samples ---\n");
            GenerateAndShowLocalMap("Forest", OvermapTerrainType.Forest, seed);
            GenerateAndShowLocalMap("House", OvermapTerrainType.Residential, seed + 2);

            Console.WriteLine("\nDone!");
        }

        private static void GenerateAndShowLocalMap(string name, OvermapTerrainType terrain, uint seed)
        {
            Console.WriteLine($"--- {name} ---");
            var mapdata = new MapgenData(terrain, Point2D.Zero, Point2D.Zero, seed);
            var localMap = TerrainGenerator.Generate(mapdata);
            Console.WriteLine(localMap.ToAscii());
        }

        /// <summary>
        /// Unity-compatible generation method
        /// </summary>
        public static CataMapGen.Overmap.Overmap GenerateOvermap(uint seed, OvermapSettings settings = null)
        {
            return OvermapGenerator.Create(Point2D.Zero, seed, settings);
        }

        /// <summary>
        /// Unity-compatible local map generation
        /// </summary>
        public static LocalMap GenerateLocalMap(OvermapTerrainType terrain, uint seed)
        {
            var mapdata = new MapgenData(terrain, Point2D.Zero, Point2D.Zero, seed);
            return TerrainGenerator.Generate(mapdata);
        }
    }
}
