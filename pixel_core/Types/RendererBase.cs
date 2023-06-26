using Pixel.Statics;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace Pixel
{
    /// <summary>
    /// an abstract starter class to base all Renderers on, which are utilized by the RenderHost.
    /// </summary>
    public abstract class RendererBase
    {
        Vector2 zero = Vector2.Zero;
        Vector2 one = Vector2.One;
        /// <summary>
        /// The stage's background cached.
        /// </summary>
        public JImage baseImage;

        /// <summary>
        /// the frame that will be drawn to each cycle.
        /// </summary>
        /// 

        public ByteArrayPool frame_pool = new();

        public class ByteArrayPool : ArrayPool<byte>
        {

            public override byte[] Rent(int minimumLength)
            {
                return Shared.Rent(minimumLength); 
            }

            public override void Return(byte[] array, bool clearArray = false)
            {
                Shared.Return(array, clearArray);
            }
        }

        public List<byte[]> frameBuffer = new();
  
        public int stride = 0;

        /// <summary>
        /// the frame that is sent out after each cycle, cached.
        /// </summary>
        public byte[] Frame
        {
            get
            {
                if (frameBuffer.Count < 3)
                    return Array.Empty<byte>();

                return frameBuffer[2];
            }
        }

        /// <summary>
        /// the stride of the render output image.
        /// </summary>
        public int Stride => stride;
        /// <summary>
        /// the cached resolution of the screen.
        /// </summary>
        public Vector2 Resolution
        {
            get => _resolution;
            set => _resolution = value;
        }
        public Vector2 _resolution = Constants.DefaultResolution;

        /// <summary>
        /// dictates whether to redraw the background or not on the next cycle.
        /// </summary>
        public bool baseImageDirty = true;

        /// <summary>
        /// Sends the last rendered frame up to the UI.
        /// </summary>
        /// <param name="output"></param>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public abstract void Render(System.Windows.Controls.Image output);
        /// <summary>
        /// Takes all the prepared renderInfo from the stage and constructs an image starting with the base image.
        /// </summary>
        /// <param name="info"></param>
        public abstract void Draw(StageRenderInfo info);
        /// <summary>
        /// cleans up any managed/unmanaged resources from last cycle.
        /// </summary>
        public abstract void Dispose();
        /// <summary>
        /// Places a pixel on the render texture of this cycle at the desired position.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="framePos"></param>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void WriteColorToFrame(ref Color color, ref Vector2 framePos)
        {
            int index = (int)framePos.Y * stride + ((int)framePos.X * 3);

            float colorB = (float)color.b / 255 * color.a;
            float colorG = (float)color.g / 255 * color.a;
            float colorR = (float)color.r / 255 * color.a;

            float frameB = (float)frameBuffer[0][index + 0] / 255 * (255 - color.a);
            float frameG = (float)frameBuffer[0][index + 1] / 255 * (255 - color.a);
            float frameR = (float)frameBuffer[0][index + 2] / 255 * (255 - color.a);

            frameBuffer[0][index + 0] = (byte)(colorB + frameB);
            frameBuffer[0][index + 1] = (byte)(colorG + frameG);
            frameBuffer[0][index + 2] = (byte)(colorR + frameR);
        }
        /// <summary>
        /// Places a pixel on the render texture of this cycle at the desired position.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="framePos"></param>
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void WriteColorToFrame(ref Color color, int x, int y)
        {
            int index = y * stride + (x * 3);

            float colorB = (float)color.b / 255 * color.a;
            float colorG = (float)color.g / 255 * color.a;
            float colorR = (float)color.r / 255 * color.a;

            float frameB = (float)frameBuffer[0][index + 0] / 255 * (255 - color.a);
            float frameG = (float)frameBuffer[0][index + 1] / 255 * (255 - color.a);
            float frameR = (float)frameBuffer[0][index + 2] / 255 * (255 - color.a);

            frameBuffer[0][index + 0] = (byte)(colorB + frameB);
            frameBuffer[0][index + 1] = (byte)(colorG + frameG);
            frameBuffer[0][index + 2] = (byte)(colorR + frameR);
        }
        /// <summary>
        /// calling this will redraw the background next frame
        /// </summary>
        public void MarkDirty()
        {
            baseImageDirty = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWithinMaxExclusive(float x, float y, float min, float max)
        {
            return x >= min && x < max && y >= min && y < max;
        }
        /// <summary>
        /// Reads a color from the current render texture.
        /// </summary>
        /// <param name="vector2"></param>
        /// <returns></returns>
        public Color ReadColorFromFrame(Vector2 vector2)
        {
            int index = (int)vector2.Y * stride + ((int)vector2.X * 3);

            float frameB = (float)frameBuffer[0][index + 0] / 255;
            float frameG = (float)frameBuffer[0][index + 1] / 255;
            float frameR = (float)frameBuffer[0][index + 2] / 255;

            byte a = 255; // assume full alpha
            if (frameB == 0 && frameG == 0 && frameR == 0)
                a = 0;
            else if (frameB == 1 && frameG == 1 && frameR == 1)
                a = 255;
            else
            {
                // Otherwise, compute the alpha value based on the RGB values
                float max = MathF.Max(MathF.Max(frameR, frameG), frameB);
                a = (byte)(max * 255);
            }

            return new Color((byte)(frameR * 255), (byte)(frameG * 255), (byte)(frameB * 255), a);
        }
    }
}

