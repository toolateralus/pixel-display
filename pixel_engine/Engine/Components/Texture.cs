using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Windows.Media;
using Newtonsoft.Json;
using pixel_renderer.Assets;
using pixel_renderer.FileIO;
namespace pixel_renderer
{
    public class Texture : Asset
    {
        #region Constructors / Overrides
        public Texture(JImage image)
        {
            jImage = image; 
        }
        public Texture(Bitmap source)
        {
            SetImage(source);
        }
        public Texture(string filePath)
        {
            SetImage(filePath);
        }
        public Texture(Pixel[,] colors)
        {
            SetImage(colors);
        }
        public Texture(Vector2 size, Pixel color)
        {
            scale = size;
            SetImage(color);
        }
        public Texture(Vector2 scale, Metadata imgData)
        {
            this.scale = scale; 
            SetImage(imgData, scale);
        }
        [JsonConstructor] protected Texture(JImage image, Metadata imgData, Vector2 scale, string Name) : base(Name, true)
        {
            this.imgData = imgData;
            this.Name = Name;
            this.scale = scale;
            this.jImage = image; 
        }

        public JImage GetImage()
        {
            return jImage; 
        }
        public void SetImageRelative(string relativePath)
        {
            var meta = AssetLibrary.FetchMetaRelative(relativePath);
            if (meta is not null)
                jImage = new(new Bitmap(meta.Path));

        }
        public void SetImage(string fullPath)
        {
            jImage = new(new Bitmap(fullPath));
        }
        public void SetImage(Pixel color)
        {
            jImage = new(CBit.SolidColorSquare(scale, color));
        }
        public void SetImage(JImage image)
        {
            jImage = image;
        }
        public void SetImage(Bitmap source)
        {
            jImage = new(source);
        }
        public void SetImage(Pixel[,] colors)
        {
            jImage = new(colors);
        }
        public void SetImage(Metadata imgData, Vector2 scale)
        {
            this.scale = scale;

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

        [Field][JsonProperty] public Vector2 scale = new(1, 1);
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

        public Pixel GetPixel(int x, int y) => jImage.GetPixel(x, y);
        public void SetPixel(int x, int y, Pixel pixel) => jImage?.SetPixel(x, y, pixel);
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
                    Pixel color = texture.GetPixel(x, y);
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
                                Pixel neighborColor = texture.GetPixel(neighborX, neighborY);
                                float neighborWeight = kernel[i + halfKernelSize, j + halfKernelSize];

                                red += neighborColor.r * neighborWeight;
                                green += neighborColor.g * neighborWeight;
                                blue += neighborColor.b * neighborWeight;
                            }
                        }
                    }

                    Pixel filteredColor = new((byte)red, (byte)green, (byte)blue, (byte)alpha);
                    texture.SetPixel(x, y, filteredColor);
                }
            return texture;
        }
    }
}
