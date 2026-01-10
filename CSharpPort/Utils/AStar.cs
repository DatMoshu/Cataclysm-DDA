using System;
using System.Collections.Generic;
using CataMapGen.Core;

namespace CataMapGen.Utils
{
    /// <summary>
    /// A* pathfinding for road/river connections
    /// </summary>
    public static class AStar
    {
        public class PathResult
        {
            public List<Point2D> Path { get; set; } = new List<Point2D>();
            public bool Found { get; set; }
            public float TotalCost { get; set; }
        }

        /// <summary>
        /// Finds path from start to end using A*
        /// </summary>
        /// <param name="start">Starting position</param>
        /// <param name="end">Target position</param>
        /// <param name="width">Grid width</param>
        /// <param name="height">Grid height</param>
        /// <param name="getCost">Function returning movement cost (return float.MaxValue for impassable)</param>
        /// <returns>Path result with list of points</returns>
        public static PathResult FindPath(Point2D start, Point2D end, int width, int height, 
            Func<Point2D, Point2D, float> getCost)
        {
            var result = new PathResult();
            
            if (start == end)
            {
                result.Found = true;
                result.Path.Add(start);
                return result;
            }

            var openSet = new SortedSet<(float f, int tieBreaker, Point2D pos)>();
            var cameFrom = new Dictionary<Point2D, Point2D>();
            var gScore = new Dictionary<Point2D, float>();
            var inOpen = new HashSet<Point2D>();
            int tieBreaker = 0;

            gScore[start] = 0;
            float h = Heuristic(start, end);
            openSet.Add((h, tieBreaker++, start));
            inOpen.Add(start);

            Point2D[] neighbors = { Point2D.North, Point2D.South, Point2D.East, Point2D.West };

            while (openSet.Count > 0)
            {
                var currentEntry = First(openSet);
                openSet.Remove(currentEntry);
                var current = currentEntry.pos;
                inOpen.Remove(current);

                if (current == end)
                {
                    // Reconstruct path
                    var path = new List<Point2D> { current };
                    while (cameFrom.ContainsKey(current))
                    {
                        current = cameFrom[current];
                        path.Add(current);
                    }
                    path.Reverse();
                    result.Path = path;
                    result.Found = true;
                    result.TotalCost = gScore[end];
                    return result;
                }

                foreach (var offset in neighbors)
                {
                    var neighbor = current + offset;

                    if (!InBounds(neighbor, width, height))
                        continue;

                    float moveCost = getCost(current, neighbor);
                    if (float.IsPositiveInfinity(moveCost))
                        continue;

                    float tentativeG = gScore[current] + moveCost;

                    if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeG;
                        float f = tentativeG + Heuristic(neighbor, end);

                        if (!inOpen.Contains(neighbor))
                        {
                            openSet.Add((f, tieBreaker++, neighbor));
                            inOpen.Add(neighbor);
                        }
                    }
                }
            }

            return result; // No path found
        }

        /// <summary>
        /// Simple straight-line path (for roads that can override terrain)
        /// </summary>
        public static List<Point2D> StraightLine(Point2D start, Point2D end)
        {
            var path = new List<Point2D>();
            int dx = Math.Abs(end.X - start.X);
            int dy = Math.Abs(end.Y - start.Y);
            int sx = start.X < end.X ? 1 : -1;
            int sy = start.Y < end.Y ? 1 : -1;
            int err = dx - dy;

            var current = start;
            while (true)
            {
                path.Add(current);
                if (current == end) break;

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    current = new Point2D(current.X + sx, current.Y);
                }
                if (e2 < dx)
                {
                    err += dx;
                    current = new Point2D(current.X, current.Y + sy);
                }
            }

            return path;
        }

        private static float Heuristic(Point2D a, Point2D b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        private static bool InBounds(Point2D p, int width, int height)
        {
            return p.X >= 0 && p.X < width && p.Y >= 0 && p.Y < height;
        }

        private static T First<T>(SortedSet<T> set)
        {
            using (var enumerator = set.GetEnumerator())
            {
                if (enumerator.MoveNext())
                    return enumerator.Current;
            }
            return default;
        }
    }
}
