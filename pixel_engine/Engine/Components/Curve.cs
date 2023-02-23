using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace pixel_renderer
{

    public class Curve
    {
        public Dictionary<Vec2, Vec2> points = new();

        private int padding;
        private int index;
        private int startIndex;
        public bool looping;
        public static Curve Circlular(float speed, int length, float radius = 1f, bool looping = true)
        {
            Curve curve = new()
            {
                padding = (int)(length / speed)
            };
            int totalLength = length * curve.padding;
            Vec2[] vecs = new Vec2[totalLength];


            for (int i = 0; i < totalLength; i++)
            {
                float t = (float)i / totalLength * CMath.Tau;
                vecs[i] = new Vec2(MathF.Sin(t), MathF.Cos(t)) * radius;
            }

            curve.CreateCurve(vecs);
            curve.looping = looping;
            return curve;

        }
        public void CreateCurve(Vec2[] vertices, int padding = 1)
        {
            this.padding = padding;
            for (int i = 0; i < vertices.Length * padding; i += padding)
            {
                var point = vertices[i / padding];
                points.Add(new Vec2(i, i + padding - 1), point);
            }
        }
        public Vec2 Next()
        {
            return Next(this);
        }
        public static Vec2 Next(Curve curve)
        {
            var outVec = new Vec2();

            if (curve.index > curve.points.Count * curve.padding - 1 && curve.looping)
                curve.index = curve.startIndex;

            foreach (var pt in curve.points)
            {
                float i = curve.index;
                if (i.IsWithin(pt.Key.x, pt.Key.y))
                    outVec =  pt.Value;
            }

            curve.index++;

            return outVec;
        }
    }
}
