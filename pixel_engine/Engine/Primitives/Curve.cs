using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Windows.Media.Animation;

namespace pixel_renderer
{
    public class Curve : Animation<Vector2>
    {
        public float Max
        {
            get
            {

                float max = float.MinValue;
                for (int i = 0; i < frames.Count; i++)
                {
                    var kvp = frames.ElementAt(i);
                    var pt = kvp.Value;

                    if (pt.Y > max)
                        max = pt.Y;
                }

                return max;
            }
        }
        public float Min
        {
            get
            {
                float min = float.MaxValue;
                for (int i = 0; i < frames.Count; i++)
                {
                    var kvp = frames.ElementAt(i);
                    var pt = kvp.Value;

                    if (pt.Y < min)
                        min = pt.Y;
                }

                return min;
            }
        }

        #region static methods

        public static Curve Circlular(float speed, int length, float radius = 1f, bool looping = true)
        {
            Curve curve = new()
            {
                frameTime = (int)(length / speed)
            };
            int totalLength = length * curve.frameTime;

            for (int i = 0; i < totalLength; i++)
            {
                float t = (float)i / totalLength * CMath.Tau;
                var val = new Vector2(MathF.Sin(t), MathF.Cos(t)) * radius;
                curve.SetValue(i, val);
            }

            curve.looping = looping;
            return curve;

        }
        public static Curve CreateCurve(Vector2[] vertices, int frameTime = 1)
        {
            if (frameTime <= 0)
                frameTime = 1;

            Curve curve = new(); 

            curve.frameTime = frameTime;

            for (int i = 0; i < vertices.Length * frameTime; i += frameTime)
            {
                var point = vertices[i / frameTime];
                var vec = new Vector2(i, i + frameTime - 1);
                curve.SetValue(i, vec);
            }
            return curve;
        }
        public static Curve Normalize(Curve curve)
        {
            float max = curve.Max; 

            var normalizedCurve = new Curve();
            for (int i = 0; i < curve.frames.Count; i++)
            {
                var kvp = curve.frames.ElementAt(i);
                var normalizedPoint = new Vector2(kvp.Value.X, kvp.Value.Y / max);
                normalizedCurve.frames[kvp.Key] = normalizedPoint;
            }
            return normalizedCurve;
        }
        public static Curve Linear(Vector2 start = default, Vector2 end = default, float speed = 1, int vertices = 16)
        {
            if (start == default)
                start = Vector2.Zero;

            if (end == default)
                end = Vector2.One;

            Curve curve = new();

            for (int i = 0; i < vertices; ++i)
            {
                float t = (vertices - i - 1) / (float)(vertices - 1);
                var vert = Vector2.Lerp(start, end, t);
                curve.SetValue(i, vert);
            }

            int frameTime = (int)(vertices / speed);
            if (frameTime <= 0)
                frameTime = 1;

            return curve;

        }
        public static Curve LinearExponential(Vector2 start = default, Vector2 end = default, float speed = 1, int vertices = 16, float pow = 1.1f)
        {
            if (start == default)
                start = Vector2.Zero;

            if (end == default)
                end = Vector2.One;

            Vector2[] output = new Vector2[vertices];

            Curve curve = new();

            for (int i = 0; i < vertices; ++i)
            {
                float t = (vertices - i - 1) / (float)(vertices - 1);
                var vec = Vector2.Lerp(start, end, MathF.Pow(t, pow));
                curve.SetValue(i, vec);
            }

            int frameTime = (int)(vertices / speed);
            
            if (frameTime <= 0) 
                frameTime = 1; 

            return curve;
        }
        #endregion
    }
}
