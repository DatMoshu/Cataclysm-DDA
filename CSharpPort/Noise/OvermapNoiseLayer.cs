using System;
using CataMapGen.Core;

namespace CataMapGen.Noise
{
    /// <summary>
    /// Abstract base class for overmap noise layers (matches CDDA's om_noise_layer)
    /// Each layer generates continuous noise across overmap boundaries using global coordinates
    /// </summary>
    public abstract class OvermapNoiseLayer
    {
        protected readonly Point2D GlobalBasePoint;
        protected readonly float Seed;

        private const int SimplexNoiseRandomSeedLimit = 2147483647;

        protected OvermapNoiseLayer(Point2D globalBasePoint, uint seed)
        {
            GlobalBasePoint = globalBasePoint;
            Seed = seed % SimplexNoiseRandomSeedLimit;
        }

        /// <summary>
        /// Gets noise value at local overmap coordinates
        /// </summary>
        public abstract float NoiseAt(Point2D localPos);

        /// <summary>
        /// Converts local overmap position to global coordinates
        /// </summary>
        protected Point2D GlobalPos(Point2D localPos)
        {
            return GlobalBasePoint + localPos;
        }
    }

    /// <summary>
    /// Forest density noise layer
    /// Simple octave noise with balanced parameters to avoid overwhelming the map
    /// </summary>
    public class ForestNoiseLayer : OvermapNoiseLayer
    {
        public ForestNoiseLayer(Point2D globalBasePoint, uint seed) : base(globalBasePoint, seed) { }

        public override float NoiseAt(Point2D localPos)
        {
            var p = GlobalPos(localPos);
            
            // Single layer of noise - simpler is better for balance
            float noise = SimplexNoise.ScaledOctaveNoise3D(3, 0.5f, 0.025f, 0, 1, p.X, p.Y, Seed);
            
            // Apply power curve to create more variation and clusters
            return (float)Math.Pow(noise, 1.8f);
        }
    }

    /// <summary>
    /// Floodplain noise layer for low-lying wet areas
    /// </summary>
    public class FloodplainNoiseLayer : OvermapNoiseLayer
    {
        public FloodplainNoiseLayer(Point2D globalBasePoint, uint seed) : base(globalBasePoint, seed) { }

        public override float NoiseAt(Point2D localPos)
        {
            var p = GlobalPos(localPos);
            float r = SimplexNoise.ScaledOctaveNoise3D(4, 0.5f, 0.05f, 0, 1, p.X, p.Y, Seed);
            r = (float)Math.Pow(r, 2.0f);
            return r;
        }
    }

    /// <summary>
    /// Lake noise layer - 8 octaves with very low scale for large features
    /// Power of 4 creates sharper edges
    /// </summary>
    public class LakeNoiseLayer : OvermapNoiseLayer
    {
        public LakeNoiseLayer(Point2D globalBasePoint, uint seed) : base(globalBasePoint, seed) { }

        public override float NoiseAt(Point2D localPos)
        {
            var p = GlobalPos(localPos);
            float r = SimplexNoise.ScaledOctaveNoise3D(8, 0.5f, 0.002f, 0, 1, p.X, p.Y, Seed);
            r = (float)Math.Pow(r, 4.0f);
            return r;
        }
    }

    /// <summary>
    /// Ocean noise layer - same as lake for seamless transitions
    /// </summary>
    public class OceanNoiseLayer : OvermapNoiseLayer
    {
        public OceanNoiseLayer(Point2D globalBasePoint, uint seed) : base(globalBasePoint, seed) { }

        public override float NoiseAt(Point2D localPos)
        {
            var p = GlobalPos(localPos);
            float r = SimplexNoise.ScaledOctaveNoise3D(8, 0.5f, 0.002f, 0, 1, p.X, p.Y, Seed);
            r = (float)Math.Pow(r, 4.0f);
            return r;
        }
    }

    /// <summary>
    /// Swamp noise layer for wetland areas
    /// </summary>
    public class SwampNoiseLayer : OvermapNoiseLayer
    {
        public SwampNoiseLayer(Point2D globalBasePoint, uint seed) : base(globalBasePoint, seed) { }

        public override float NoiseAt(Point2D localPos)
        {
            var p = GlobalPos(localPos);
            float r = SimplexNoise.ScaledOctaveNoise3D(4, 0.5f, 0.04f, 0, 1, p.X, p.Y, Seed);
            r = (float)Math.Pow(r, 2.0f);
            return r;
        }
    }
}
