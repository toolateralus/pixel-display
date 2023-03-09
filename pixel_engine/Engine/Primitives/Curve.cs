using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Windows.Media.Animation;

namespace pixel_renderer
{

    public class Curve
    {
        public Dictionary<Vector2, Vector2> points = new();

        private int padding;
        private int index;
        private int startIndex;
        public bool looping;

        public void Reset()
        {
            index = startIndex;
        }
        public static Curve Circlular(float speed, int length, float radius = 1f, bool looping = true)
        {
            Curve curve = new()
            {
                padding = (int)(length / speed)
            };
            int totalLength = length * curve.padding;
            Vector2[] vecs = new Vector2[totalLength];


            for (int i = 0; i < totalLength; i++)
            {
                float t = (float)i / totalLength * CMath.Tau;
                vecs[i] = new Vector2(MathF.Sin(t), MathF.Cos(t)) * radius;
            }

            curve.SetVertices(vecs);
            curve.looping = looping;
            return curve;

        }
        public static Curve CreateCurve(Vector2[] vertices, int padding = 1)
        {
            if (padding <= 0)
                padding = 1;

            Curve curve = new(); 

            curve.padding = padding;

            for (int i = 0; i < vertices.Length * padding; i += padding)
            {
                var point = vertices[i / padding];
                curve.points.Add(new Vector2(i, i + padding - 1), point);
            }
            return curve;
        }
        public void SetVertices(Vector2[] vertices, int padding = 1)
        {
            if (padding <= 0)
                padding = 1;

            this.padding = padding;

            for (int i = 0; i < vertices.Length * padding; i += padding)
            {
                var point = vertices[i / padding];
                points.Add(new Vector2(i, i + padding - 1), point);
            }
        }
        public Vector2 Next()
        {
            return Next(this);
        }
        public static Vector2 Next(Curve curve)
        {
            var outVec = new Vector2();

            if (curve.index > curve.points.Count * curve.padding - 1 && curve.looping)
                curve.index = curve.startIndex;

            foreach (var pt in curve.points)
            {
                float i = curve.index;
                if (i.IsWithin(pt.Key.X, pt.Key.Y))
                    outVec = pt.Value;
            }

            curve.index++;

            return outVec;
        }
        public static Curve Normalize(Curve curve)
        {
            float max = curve.Max; 

            var normalizedCurve = new Curve();
            for (int i = 0; i < curve.points.Count; i++)
            {
                var kvp = curve.points.ElementAt(i);
                var normalizedPoint = new Vector2(kvp.Value.X, kvp.Value.Y / max);
                normalizedCurve.points[kvp.Key] = normalizedPoint;
            }
            return normalizedCurve;
        }
        public float Min
        {
            get
            {
                float min = float.MaxValue;
                for (int i = 0; i < points.Count; i++)
                {
                    var kvp = points.ElementAt(i);
                    var pt = kvp.Value;

                    if (pt.Y < min)
                        min = pt.Y;
                }

                return min;
            }
        }
        public float Max
        {
            get
            {

                float max = float.MinValue;
                for (int i = 0; i < points.Count; i++)
                {
                    var kvp = points.ElementAt(i);
                    var pt = kvp.Value;

                    if (pt.Y > max)
                        max = pt.Y;
                }

                return max;
            }
        }

        internal static Curve Linear(Vector2 start = default, Vector2 end = default, float speed = 1, int vertices = 16)
        {
            if (start == default)
                start = Vector2.Zero;

            if (end == default)
                end = Vector2.One;

            Vector2[] output = new Vector2[vertices];

            for (int i = 0; i < vertices; ++i)
            {
                float t = (vertices - i - 1) / (float)(vertices - 1);

                output[i] = Vector2.Lerp(start, end, t);
            }

            Curve curve = new();
            int padding = (int)(vertices / speed);
            if (padding <= 0)
                padding = 1;

            curve.SetVertices(output, padding);
            return curve;

        }
        public static Curve LinearExponential(Vector2 start = default, Vector2 end = default, float speed = 1, int vertices = 16, float pow = 1.1f)
        {
            if (start == default)
                start = Vector2.Zero;

            if (end == default)
                end = Vector2.One;

            Vector2[] output = new Vector2[vertices];

            for (int i = 0; i < vertices; ++i)
            {
                float t = (vertices - i - 1) / (float)(vertices - 1);
                output[i] = Vector2.Lerp(start, end, MathF.Pow(t, pow));
            }

            Curve curve = new();
            int padding = (int)(vertices / speed);
            
            if (padding <= 0) 
                padding = 1; 

            curve.SetVertices(output, padding);
            return curve;
        }
    }
}
