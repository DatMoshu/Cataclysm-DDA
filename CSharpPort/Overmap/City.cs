using System;
using CataMapGen.Core;

namespace CataMapGen.Overmap
{
    /// <summary>
    /// City data structure (matches CDDA's city struct)
    /// </summary>
    [Serializable]
    public class City
    {
        public Point2D Position;
        public int Size;        // Radius of city influence
        public string Name;

        public City(Point2D pos, int size, string name = "")
        {
            Position = pos;
            Size = size;
            Name = name;
        }

        /// <summary>
        /// Distance from city center to a point
        /// </summary>
        public int DistanceTo(Point2D p)
        {
            return Position.ManhattanDistance(p);
        }

        /// <summary>
        /// Check if point is within city radius
        /// </summary>
        public bool Contains(Point2D p)
        {
            return DistanceTo(p) <= Size;
        }
    }
}
