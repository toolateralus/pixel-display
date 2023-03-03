using System;
using System.Drawing;
using System.Numerics;
using System.Windows.Media;
using Newtonsoft.Json;
using pixel_renderer.FileIO;
namespace pixel_renderer
{
    public class Texture : Asset
    {
        [JsonConstructor]
        public Texture(JImage image, Metadata imgData, Vector2 scale, string Name = "Texture Asset") : base(Name, true)
        {
            this.imgData = imgData;
            this.Name = Name;
            this.scale = scale;
            this.jImage = image; 
        }
        public Texture(Vector2 scale, Metadata imgData)
        {
            SetImage(imgData, scale);
        }
        public Texture(Vector2 size, Pixel color)
        {
            scale = size;
            SetImage(color);
        }
        [Field] 
        [JsonProperty] 
        public Vector2 scale = new(1, 1);
        [JsonProperty] 
        internal Metadata imgData;
        [JsonProperty]
        private JImage jImage = new();
        Bitmap initializedBitmap;
        public Bitmap? Image {
            get 
            {
                if (!HasImage && HasImageMetadata)
                    initializedBitmap = new(imgData.Path);
                return initializedBitmap;
            }
            set => initializedBitmap = value;
        }
        public bool HasImage => initializedBitmap != null;
        internal bool HasImageMetadata => imgData != null;
        public Bitmap GetScaledBitmap() => ImageScaling.Scale(Image, scale);

        public void SetImage(Metadata imgData, Vector2 scale)
        {
            this.scale = scale;

            if (imgData is not null)
            {
                this.imgData = imgData;
                Image = new(imgData.Path);
            }
            else
            {
                this.imgData = Player.PlayerSprite;
                Image = new(imgData.Path);
            }
            jImage = new();
            var colors = CBit.PixelArrayFromBitmap(Image);
            SetImage(colors);
        }
        public void SetImage(Vector2 size, byte[] data)
        {
            jImage = new(size, data);
        }
        public void SetImage(JImage image)
        {
            jImage = image;
        }
        public void SetImage(Pixel[,] colors)
        {
            jImage = new(colors);
        }
        public void SetImage(Pixel color)
        {
            jImage = new(CBit.SolidColorSquare(scale, color));
        }

        public Pixel GetPixel(int x, int y) => jImage.GetPixel(x, y);
        public void SetPixel(int x, int y, Pixel pixel) => jImage?.SetPixel(x, y, pixel);

        public JImage GetImage()
        {
            return jImage; 
        }

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

            // Normalize the kernel so that all values sum up to 1
            for (int x = 0; x < kernelSize; x++)
            {
                for (int y = 0; y < kernelSize; y++)
                {
                    kernel[x, y] /= kernelSum;
                }
            }

            // Apply the filter to each pixel in the texture
            for (int x = 0; x < texture.width; x++)
            {
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

                    // Set the filtered color for the current pixel
                    Pixel filteredColor = new Pixel((byte)red, (byte)green, (byte)blue, (byte)alpha);
                    texture.SetPixel(x, y, filteredColor);
                }
            }
            return texture;
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
    }
}
