using CataMapGen.Core;

namespace CataMapGen.Overmap
{
    /// <summary>
    /// Orchestrates the generation of an overmap using individual generators
    /// </summary>
    public class OvermapGenerator
    {
        private readonly Overmap _overmap;
        private readonly ForestGenerator _forestGen;
        private readonly WaterGenerator _waterGen;
        private readonly CityGenerator _cityGen;
        private readonly RoadGenerator _roadGen;
        private readonly SpecialsGenerator _specialsGen;

        public OvermapGenerator(Overmap overmap)
        {
            _overmap = overmap;
            _forestGen = new ForestGenerator(overmap);
            _waterGen = new WaterGenerator(overmap);
            _cityGen = new CityGenerator(overmap);
            _roadGen = new RoadGenerator(overmap);
            _specialsGen = new SpecialsGenerator(overmap);
        }

        /// <summary>
        /// Generates the complete overmap in CDDA order
        /// </summary>
        public void Generate()
        {
            var settings = _overmap.Settings;

            // 1. Water first (rivers, lakes)
            if (settings.PlaceRivers)
                _waterGen.PlaceRivers();
            
            if (settings.PlaceLakes)
                _waterGen.PlaceLakes();

            // 2. Terrain (forests, swamps)
            if (settings.PlaceForests)
                _forestGen.PlaceForests();
            
            if (settings.PlaceSwamps)
                _forestGen.PlaceSwamps();

            // 3. Civilization
            if (settings.PlaceCities)
                _cityGen.PlaceCities();
            
            if (settings.PlaceRoads)
                _roadGen.PlaceRoads();

            // 4. Specials (hospitals, gas stations, etc)
            if (settings.PlaceCities)
                _specialsGen.PlaceSpecials();

            // 5. Polish
            _waterGen.PolishRivers();
        }

        /// <summary>
        /// Static helper to generate an overmap in one call
        /// </summary>
        public static Overmap Create(Point2D globalPos, uint seed, OvermapSettings settings = null)
        {
            var overmap = new Overmap(globalPos, seed, settings);
            var generator = new OvermapGenerator(overmap);
            generator.Generate();
            return overmap;
        }
    }
}
