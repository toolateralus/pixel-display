﻿namespace PixelRenderer
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Input;
    using Random = System.Random;
    using Color = System.Drawing.Color;
    public static class Constants
    {
        public const int frameRateCheckThresh = 60;
        public const int screenWidth = 64;
        public const int screenHeight = 64;
    }
    public static class CMath
    {
        public const float Gravity = 3f; 
        public const float PI = MathF.PI; 
        public const float Tau = MathF.PI * 2;
        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
        public static float Power(float value, int power)
        {
            float output = 1f; 
            for (int i = 0; i < power; i++)
            {
                output *= value; 
            }
            return output; 
        }
    }
    public static class JRandom
    {
        public static Color GetRandomColor()
        {
            byte r = RandomByte(),
                g = RandomByte(),
                b = RandomByte(),
                a = RandomByte();

            return Color.FromArgb(r, g, b, a);
        }
        public static byte RandomByte() => (byte)new Random().Next(0, 255);
        public static int RandomInt(int min, int max) => new Random().Next(min, max);

    }
    public static class WaveForms
    {
        public static int vertices = 1024;
        public static float amplitude = 1;
        public static float frequency = .5f;
        public static Vec2 xLimits = new Vec2(0, 15);
        public static float movementSpeed = .5f;
        public static float radians = 2 * CMath.PI;
        /// <summary>
        /// Samples a random vertex point on a Sine Wave operating within pre-defined parameters.
        /// </summary>
        public static Vec2 Next => GetPointOnSine();
        /// <summary>
        /// Manually define parameters for a sample from a sine wave.
        /// </summary>
        /// <param name="startPosition">the start of the wave</param>
        /// <param name="endPosition">the end position of the wave</param>
        /// <param name="Tau">A float within the range of 0 to PI * 2</param>
        /// <param name="vertexIndex">the individual vertex of the wave which will be returned</param>
        /// <param name="x">out X of the returned vector</param>
        /// <param name="y">out Y of the returned vector</param>
        /// <returns>A Vertex position on the specified wave.</returns>
        public static Vec2 GetPointOnSine(float startPosition = 0, float endPosition = 1, float Tau = CMath.PI * 2, int vertexIndex = 0)
        {
            float progress = (float)vertexIndex / (vertices - 1);
            float x = CMath.Lerp(startPosition, endPosition, progress);
            float y = (float)(amplitude * Math.Sin((Tau * frequency * x) + Runtime.Instance.frameCount * movementSpeed));
            return new Vec2(x, y);
        }
        /// <summary>
        /// Sample a sine wave under the current defined parameters of the static class Sine.
        /// </summary>
        /// <returns>A Vertex position at a random point on a sine wave</returns>
        public static Vec2 GetPointOnSine()
        {
            int vertexIndex = JRandom.RandomInt(0, vertices);
            const float Tau = CMath.PI * 2;
            float progress = (float)vertexIndex / (vertices - 1);
            var x = CMath.Lerp(0, 1, progress);
            float y = (float)(amplitude * Math.Sin((Tau * frequency * x) + (Runtime.Instance.frameCount * movementSpeed)));
            return new Vec2(x, y);
        }
    }
    public static class Input
    {
        static Dictionary<Key, bool> KeyDown = new Dictionary<Key, bool>();
        static Key[] keys =
        {
            Key.W, Key.A, Key.S, Key.D, Key.Space, Key.Q
        };

        public static bool GetKeyDown(Key key)
        {
            if (KeyDown.ContainsKey(key))
            {
                return KeyDown[key];
            }
            return false;
        }
        public static void SetKey(Key key, bool result)
        {
            if (!KeyDown.ContainsKey(key))
            {
                KeyDown.Add(key, result);
            }
            else KeyDown[key] = result;
        }
        public static void UpdateKeyboardState()
        {
            foreach (Key key in keys)
            {
                SetKey(key, Keyboard.IsKeyDown(key));
            }
        }
        /// <summary>
        /// Get a Vec2 representing WASD input values, with a vector max range of -1 to 1 
        /// </summary>
        /// <returns>new Vec2(right - left, up - down) between -1 and 1</returns>
        public static Vec2 GetMoveVector()
    {
        bool right = GetKeyDown(Key.A);
        bool left = GetKeyDown(Key.D);
        bool up = GetKeyDown(Key.W);
        bool down = GetKeyDown(Key.S);

        int upMove = up ? 0 : 1;
        int downMove = down ? 0 : 1;
        int rightMove = right ? 0 : 1;
        int leftMove = left ? 0 : 1;

        return new Vec2(rightMove - leftMove, upMove - downMove); 
    }
    }
}


