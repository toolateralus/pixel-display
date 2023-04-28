﻿using Newtonsoft.Json;
using Pixel.Assets;
using Pixel.FileIO;
using System;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace Pixel
{
    public class Texture : Asset
    {
        [Field][JsonProperty] public Vector2 size = new(1, 1);
        [JsonProperty] internal Metadata imgData;
        [JsonProperty] private JImage jImage = new();
        public bool HasImage => initializedBitmap != null;
        internal bool HasImageMetadata => imgData != null;

        private Bitmap initializedBitmap;
        public Bitmap? Image
        {
            get
            {
                if (!HasImage && HasImageMetadata)
                    initializedBitmap = new(imgData.Path);
                return initializedBitmap;
            }
            set => initializedBitmap = value;
        }
        public byte[] Data
        {
            get => jImage.data;
        }
        internal int Width
        {
            get => jImage.width;
        }
        internal int Height
        {
            get => jImage.height;
        }
        #region Constructors / Overrides
        public Texture(JImage image, string name = "Default Texture Asset") : base(name, true)
        {
            jImage = image;
        }

        public Texture(string filePath, string name = "Default Texture Asset") : base(name, true)
        {
            SetImage(filePath);
        }
        public Texture(Color[,] colors, string name = "Default Texture Asset") : base(name, true)
        {
            SetImage(colors);
        }
        public Texture(Vector2 size, Color color, string name = "Default Texture Asset") : base(name, true)
        {
            this.size = size;
            SetImage(color);
        }
        public Texture(Vector2 scale, Metadata imgData, string name = "Default Texture Asset") : base(name, true)
        {
            this.size = scale;
            SetImage(imgData, scale);
        }
        [JsonConstructor]
        protected Texture(JImage image, Metadata imgData, Vector2 scale, string Name) : base(Name, true)
        {
            this.imgData = imgData;
            this.Name = Name;
            this.size = scale;
            this.jImage = image;
        }
        public JImage GetImage()
        {
            return jImage;
        }
        public void SetImageRelative(string relativePath)
        {
            var meta = Library.FetchMetaRelative(relativePath);
            if (meta is not null)
                jImage = new(new Bitmap(meta.Path));

        }
        public void SetImage(string fullPath)
        {
            if (fullPath.Contains('.'))
            {
                imgData = new(fullPath);
                jImage = new(new Bitmap(fullPath));
            }
            else throw new FileNamingException("Invalid path");
        }
        public void SetImage(Color color)
        {
            jImage = new(CBit.SolidColorSquare(size, color));
        }
        public void SetImage(JImage image)
        {
            this.size = jImage.Size;
            jImage = image;
        }

        public void SetImage(Color[,] colors)
        {
            this.size = new(colors.GetLength(0), colors.GetLength(1));
            jImage = new(colors);
        }
        public void SetImage(Metadata imgData, Vector2 scale)
        {
            this.size = scale;

            if (imgData is not null)
            {
                this.imgData = imgData;
                Image = new(imgData.Path);
            }

            jImage = new();

            // maybe unneccesary
            if (Image is null)
                return;

            var colors = CBit.PixelFromBitmap(Image);
            SetImage(colors);
        }
        public void SetImage(Vector2 size, byte[] data)
        {
            jImage = new(size, data);
        }
        #endregion
        public Color GetPixel(int x, int y) => jImage.GetPixel(x, y);
        public void SetPixel(int x, int y, Color pixel) => jImage?.SetPixel(x, y, pixel);
        public static JImage ApplyGaussianFilter(JImage texture, float radius, int kernelSize)
        {
            int halfKernelSize = kernelSize / 2;
            float[,] kernel = new float[kernelSize, kernelSize];
            float kernelSum = 0;

            // Populate the kernel with Gaussian values
            for (int x = -halfKernelSize; x <= halfKernelSize; x++)
            {
                for (int y = -halfKernelSize; y <= halfKernelSize; y++)
                {
                    float gaussianValue = (float)(1.0 / (2.0 * Math.PI * radius * radius) * Math.Exp(-(x * x + y * y) / (2 * radius * radius)));
                    kernel[x + halfKernelSize, y + halfKernelSize] = gaussianValue;
                    kernelSum += gaussianValue;
                }
            }

            for (int x = 0; x < kernelSize; x++)
                for (int y = 0; y < kernelSize; y++)
                    kernel[x, y] /= kernelSum;

            // Apply the filter to each pixel in the texture
            for (int x = 0; x < texture.width; x++)
                for (int y = 0; y < texture.height; y++)
                {
                    Color color = texture.GetPixel(x, y);
                    float red = 0, green = 0, blue = 0, alpha = color.a;

                    // Apply the kernel to the pixel and its neighboring pixels
                    for (int i = -halfKernelSize; i <= halfKernelSize; i++)
                    {
                        for (int j = -halfKernelSize; j <= halfKernelSize; j++)
                        {
                            int neighborX = x + i;
                            int neighborY = y + j;

                            if (neighborX >= 0 && neighborX < texture.width && neighborY >= 0 && neighborY < texture.height)
                            {
                                Color neighborColor = texture.GetPixel(neighborX, neighborY);
                                float neighborWeight = kernel[i + halfKernelSize, j + halfKernelSize];

                                red += neighborColor.r * neighborWeight;
                                green += neighborColor.g * neighborWeight;
                                blue += neighborColor.b * neighborWeight;
                            }
                        }
                    }

                    Color filteredColor = new((byte)red, (byte)green, (byte)blue, (byte)alpha);
                    texture.SetPixel(x, y, filteredColor);
                }
            return texture;
        }
    }
}
