namespace CataMapGen.Core
{
    /// <summary>
    /// Cardinal directions for overmap tiles (matches CDDA's om_direction)
    /// </summary>
    public enum OmDirection
    {
        North = 0,
        East = 1,
        South = 2,
        West = 3,
        Invalid = 4
    }

    public static class OmDirectionExtensions
    {
        private const int DirectionCount = 4;
        private const int Bits = 0b1111; // All 4 direction bits

        /// <summary>
        /// Rotates direction clockwise by 90 degrees
        /// </summary>
        public static OmDirection RotateCW(this OmDirection dir)
        {
            if (dir == OmDirection.Invalid) return OmDirection.Invalid;
            return (OmDirection)(((int)dir + 1) % DirectionCount);
        }

        /// <summary>
        /// Rotates direction counter-clockwise by 90 degrees
        /// </summary>
        public static OmDirection RotateCCW(this OmDirection dir)
        {
            if (dir == OmDirection.Invalid) return OmDirection.Invalid;
            return (OmDirection)(((int)dir + 3) % DirectionCount);
        }

        /// <summary>
        /// Returns the opposite direction
        /// </summary>
        public static OmDirection Opposite(this OmDirection dir)
        {
            if (dir == OmDirection.Invalid) return OmDirection.Invalid;
            return (OmDirection)(((int)dir + 2) % DirectionCount);
        }

        /// <summary>
        /// Gets offset point for this direction
        /// </summary>
        public static Point2D ToOffset(this OmDirection dir)
        {
            switch (dir)
            {
                case OmDirection.North: return Point2D.North;
                case OmDirection.East: return Point2D.East;
                case OmDirection.South: return Point2D.South;
                case OmDirection.West: return Point2D.West;
                default: return Point2D.Zero;
            }
        }

        /// <summary>
        /// Rotates a line bitmask (used for road/river connections)
        /// CDDA: om_lines::rotate
        /// </summary>
        public static int RotateLine(int line, OmDirection dir)
        {
            if (dir == OmDirection.Invalid) return line;
            int shift = (int)dir;
            return ((line << shift) | (line >> (DirectionCount - shift))) & Bits;
        }

        /// <summary>
        /// Sets a connection segment in a line bitmask
        /// </summary>
        public static int SetSegment(int line, OmDirection dir)
        {
            if (dir == OmDirection.Invalid) return line;
            return line | (1 << (int)dir);
        }

        /// <summary>
        /// Checks if line has a segment in the given direction
        /// </summary>
        public static bool HasSegment(int line, OmDirection dir)
        {
            if (dir == OmDirection.Invalid) return false;
            return (line & (1 << (int)dir)) != 0;
        }

        /// <summary>
        /// Checks if line is straight (N-S or E-W only)
        /// </summary>
        public static bool IsStraight(int line)
        {
            return line == 0b0101 || line == 0b1010; // NS or EW
        }
    }
}
