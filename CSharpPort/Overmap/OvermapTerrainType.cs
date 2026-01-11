using System;

namespace CataMapGen.Overmap
{
    /// <summary>
    /// Overmap terrain types (simplified from CDDA's oter_id system)
    /// </summary>
    public enum OvermapTerrainType
    {
        // Base terrain
        Field,
        Forest,
        ForestThick,
        ForestWater,
        
        // Water features
        River,
        RiverCenter,
        Lake,
        LakeShore,
        Ocean,
        OceanShore,
        
        // Wetlands
        Swamp,
        Floodplain,
        
        // Urban
        Road,
        RoadManhole,
        City,
        Residential,
        Commercial,
        Industrial,
        
        // Special Buildings
        Hospital,
        Mall,
        GasStation,
        Police,
        FireStation,
        School,
        Church,
        Park,
        Cemetery,
        
        // Infrastructure
        Bridge,
        Empty,
        SolidEarth,  // Underground
        OpenAir      // Above ground
    }

    /// <summary>
    /// Flags for overmap terrain properties (matches CDDA's oter_flags)
    /// </summary>
    [Flags]
    public enum OvermapTerrainFlags
    {
        None = 0,
        Water = 1 << 0,
        River = 1 << 1,
        Lake = 1 << 2,
        LakeShore = 1 << 3,
        Ocean = 1 << 4,
        OceanShore = 1 << 5,
        Road = 1 << 6,
        Sidewalk = 1 << 7,
        Bridge = 1 << 8,
        LinearFeature = 1 << 9,  // Roads, rivers with segments
        KnownDown = 1 << 10,     // Stairs down
        KnownUp = 1 << 11        // Stairs up
    }

    /// <summary>
    /// Vision level for overmap tiles
    /// </summary>
    public enum OmVisionLevel
    {
        Unseen,
        Vague,      // Quick glance - forest/field/buildings/water
        Outlines,   // Distance scan - roads, obvious features
        Details,    // Detailed scan - building types
        Full        // Complete knowledge
    }

    /// <summary>
    /// Represents a single overmap tile with terrain and metadata
    /// </summary>
    [Serializable]
    public class OvermapTile
    {
        public OvermapTerrainType Terrain;
        public OvermapTerrainFlags Flags;
        public OmVisionLevel Vision;
        public int Rotation;      // 0-3 for cardinal directions
        public int LineSegments;  // Bitmask for linear features (roads/rivers)
        public bool Explored;

        public OvermapTile()
        {
            Terrain = OvermapTerrainType.Field;
            Flags = OvermapTerrainFlags.None;
            Vision = OmVisionLevel.Unseen;
            Rotation = 0;
            LineSegments = 0;
            Explored = false;
        }

        public OvermapTile(OvermapTerrainType terrain) : this()
        {
            Terrain = terrain;
            UpdateFlags();
        }

        /// <summary>
        /// Updates flags based on terrain type
        /// </summary>
        public void UpdateFlags()
        {
            Flags = OvermapTerrainFlags.None;

            switch (Terrain)
            {
                case OvermapTerrainType.River:
                case OvermapTerrainType.RiverCenter:
                    Flags |= OvermapTerrainFlags.Water | OvermapTerrainFlags.River;
                    break;
                case OvermapTerrainType.Lake:
                    Flags |= OvermapTerrainFlags.Water | OvermapTerrainFlags.Lake;
                    break;
                case OvermapTerrainType.LakeShore:
                    Flags |= OvermapTerrainFlags.Water | OvermapTerrainFlags.LakeShore;
                    break;
                case OvermapTerrainType.Ocean:
                    Flags |= OvermapTerrainFlags.Water | OvermapTerrainFlags.Ocean;
                    break;
                case OvermapTerrainType.OceanShore:
                    Flags |= OvermapTerrainFlags.Water | OvermapTerrainFlags.OceanShore;
                    break;
                case OvermapTerrainType.Road:
                case OvermapTerrainType.RoadManhole:
                    Flags |= OvermapTerrainFlags.Road | OvermapTerrainFlags.Sidewalk | OvermapTerrainFlags.LinearFeature;
                    break;
                case OvermapTerrainType.Bridge:
                    Flags |= OvermapTerrainFlags.Road | OvermapTerrainFlags.Bridge;
                    break;
            }
        }

        public bool IsWater => (Flags & OvermapTerrainFlags.Water) != 0;
        public bool IsRoad => (Flags & OvermapTerrainFlags.Road) != 0;
        public bool IsLinear => (Flags & OvermapTerrainFlags.LinearFeature) != 0;

        /// <summary>
        /// Gets ASCII symbol for map display
        /// </summary>
        public char GetSymbol()
        {
            switch (Terrain)
            {
                case OvermapTerrainType.Field: return '.';
                case OvermapTerrainType.Forest: return 'F';
                case OvermapTerrainType.ForestThick: return 'F';
                case OvermapTerrainType.ForestWater: return 'f';
                case OvermapTerrainType.River:
                case OvermapTerrainType.RiverCenter: return '~';
                case OvermapTerrainType.Lake:
                case OvermapTerrainType.LakeShore: return '≈';
                case OvermapTerrainType.Ocean:
                case OvermapTerrainType.OceanShore: return '≋';
                case OvermapTerrainType.Swamp: return '&';
                case OvermapTerrainType.Floodplain: return ',';
                case OvermapTerrainType.Road:
                    return GetRoadSymbol();
                case OvermapTerrainType.RoadManhole: return 'O';
                case OvermapTerrainType.City:
                case OvermapTerrainType.Residential: return 'H';
                case OvermapTerrainType.Commercial: return 'C';
                case OvermapTerrainType.Industrial: return 'I';
                // Special buildings
                case OvermapTerrainType.Hospital: return 'X';
                case OvermapTerrainType.Mall: return 'M';
                case OvermapTerrainType.GasStation: return 'G';
                case OvermapTerrainType.Police: return 'P';
                case OvermapTerrainType.FireStation: return 'f';
                case OvermapTerrainType.School: return 's';
                case OvermapTerrainType.Church: return '+';
                case OvermapTerrainType.Park: return 'O';
                case OvermapTerrainType.Cemetery: return 'c';
                case OvermapTerrainType.Bridge: return '=';
                default: return '?';
            }
        }

        /// <summary>
        /// Gets road symbol based on connections (N=1, E=2, S=4, W=8)
        /// </summary>
        private char GetRoadSymbol()
        {
            // LineSegments bitmask: N=1, E=2, S=4, W=8
            switch (LineSegments)
            {
                // Dead ends
                case 0b0001: return '│'; // N only
                case 0b0010: return '─'; // E only
                case 0b0100: return '│'; // S only
                case 0b1000: return '─'; // W only
                
                // Straight lines
                case 0b0101: return '│'; // N-S
                case 0b1010: return '─'; // E-W
                
                // Curves (corners)
                case 0b0011: return '└'; // N-E
                case 0b0110: return '┌'; // E-S
                case 0b1100: return '┐'; // S-W
                case 0b1001: return '┘'; // W-N
                
                // T-junctions
                case 0b0111: return '├'; // N-E-S
                case 0b1011: return '┴'; // N-E-W
                case 0b1101: return '┤'; // N-S-W
                case 0b1110: return '┬'; // E-S-W
                
                // 4-way intersection
                case 0b1111: return '┼';
                
                // No connections (isolated)
                case 0: return '+';
                
                default: return '#';
            }
        }
    }
}
