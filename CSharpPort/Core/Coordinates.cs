using System;

namespace CataMapGen.Core
{
    /// <summary>
    /// 2D integer point for overmap coordinates
    /// </summary>
    [Serializable]
    public struct Point2D : IEquatable<Point2D>, IComparable<Point2D>
    {
        public int X;
        public int Y;

        public Point2D(int x, int y) { X = x; Y = y; }

        public static Point2D Zero => new Point2D(0, 0);
        public static Point2D North => new Point2D(0, -1);
        public static Point2D South => new Point2D(0, 1);
        public static Point2D East => new Point2D(1, 0);
        public static Point2D West => new Point2D(-1, 0);

        public static Point2D operator +(Point2D a, Point2D b) => new Point2D(a.X + b.X, a.Y + b.Y);
        public static Point2D operator -(Point2D a, Point2D b) => new Point2D(a.X - b.X, a.Y - b.Y);
        public static Point2D operator *(Point2D p, int s) => new Point2D(p.X * s, p.Y * s);
        public static bool operator ==(Point2D a, Point2D b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(Point2D a, Point2D b) => !(a == b);

        public float Magnitude => (float)Math.Sqrt(X * X + Y * Y);
        public int ManhattanDistance(Point2D other) => Math.Abs(X - other.X) + Math.Abs(Y - other.Y);

        public bool Equals(Point2D other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is Point2D p && Equals(p);
        public override int GetHashCode() => X * 31 + Y;
        public override string ToString() => $"({X}, {Y})";

        public int CompareTo(Point2D other)
        {
            int cmp = X.CompareTo(other.X);
            return cmp != 0 ? cmp : Y.CompareTo(other.Y);
        }
    }

    /// <summary>
    /// 3D integer point for overmap coordinates with Z level
    /// </summary>
    [Serializable]
    public struct Point3D : IEquatable<Point3D>
    {
        public int X;
        public int Y;
        public int Z;

        public Point3D(int x, int y, int z) { X = x; Y = y; Z = z; }
        public Point3D(Point2D p, int z) { X = p.X; Y = p.Y; Z = z; }

        public Point2D XY => new Point2D(X, Y);
        public static Point3D Zero => new Point3D(0, 0, 0);
        public static Point3D Above => new Point3D(0, 0, 1);
        public static Point3D Below => new Point3D(0, 0, -1);

        public static Point3D operator +(Point3D a, Point3D b) => new Point3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Point3D operator -(Point3D a, Point3D b) => new Point3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static bool operator ==(Point3D a, Point3D b) => a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        public static bool operator !=(Point3D a, Point3D b) => !(a == b);

        public bool Equals(Point3D other) => X == other.X && Y == other.Y && Z == other.Z;
        public override bool Equals(object obj) => obj is Point3D p && Equals(p);
        public override int GetHashCode() => (X * 31 + Y) * 31 + Z;
        public override string ToString() => $"({X}, {Y}, {Z})";
    }
}
