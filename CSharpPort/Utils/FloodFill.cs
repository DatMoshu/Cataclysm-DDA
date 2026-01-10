using System;
using System.Collections.Generic;
using CataMapGen.Core;

namespace CataMapGen.Utils
{
    /// <summary>
    /// Flood fill utility for finding connected regions
    /// </summary>
    public static class FloodFill
    {
        /// <summary>
        /// Performs flood fill from a starting point
        /// </summary>
        /// <param name="start">Starting position</param>
        /// <param name="width">Grid width</param>
        /// <param name="height">Grid height</param>
        /// <param name="canFill">Predicate to check if a position can be filled</param>
        /// <returns>Set of all filled positions</returns>
        public static HashSet<Point2D> Fill(Point2D start, int width, int height, Func<Point2D, bool> canFill)
        {
            var filled = new HashSet<Point2D>();
            var queue = new Queue<Point2D>();

            if (!InBounds(start, width, height) || !canFill(start))
                return filled;

            queue.Enqueue(start);
            filled.Add(start);

            Point2D[] neighbors = { Point2D.North, Point2D.South, Point2D.East, Point2D.West };

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                foreach (var offset in neighbors)
                {
                    var next = current + offset;
                    if (InBounds(next, width, height) && !filled.Contains(next) && canFill(next))
                    {
                        filled.Add(next);
                        queue.Enqueue(next);
                    }
                }
            }

            return filled;
        }

        /// <summary>
        /// Finds the largest connected region matching the predicate
        /// </summary>
        public static HashSet<Point2D> FindLargestRegion(int width, int height, Func<Point2D, bool> matches)
        {
            var visited = new HashSet<Point2D>();
            HashSet<Point2D> largest = null;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var p = new Point2D(x, y);
                    if (!visited.Contains(p) && matches(p))
                    {
                        var region = Fill(p, width, height, pos => !visited.Contains(pos) && matches(pos));
                        foreach (var filled in region)
                            visited.Add(filled);

                        if (largest == null || region.Count > largest.Count)
                            largest = region;
                    }
                }
            }

            return largest ?? new HashSet<Point2D>();
        }

        private static bool InBounds(Point2D p, int width, int height)
        {
            return p.X >= 0 && p.X < width && p.Y >= 0 && p.Y < height;
        }
    }
}
