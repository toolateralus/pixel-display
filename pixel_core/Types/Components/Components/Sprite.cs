using Newtonsoft.Json;
using Pixel.Types.Components;
using Pixel.Types.Physics;
using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Pixel
{
    public enum ImageType { SolidColor, Image, Custom };
    public enum TextureFiltering { Point, Bilinear }
    public class Sprite : Component
    {
        /// <summary>
        /// color data is refreshed from source on update if this is true
        /// </summary>
        public bool IsDirty = true;
        [Field][JsonProperty] public Vector2 viewportScale = new(1, 1);
        [Field][JsonProperty] public Vector2 viewportOffset = new(0.0f, 0.0f);
        [Field][JsonProperty] public float camDistance = 1;
        /// <summary>
        /// stores color data
        /// </summary>
        [Field][JsonProperty] public Texture texture;
        /// <summary>
        /// this determines what source the color data will come from
        /// </summary>
        [Field][JsonProperty] public ImageType Type = ImageType.SolidColor;
        /// <summary>
        /// this dictates how the renderer filters the sprite during drawing
        /// </summary>
        [Field][JsonProperty] public TextureFiltering textureFiltering = 0;
        /// <summary>
        /// this will toggle participation in lighting
        /// </summary>
        [Field][JsonProperty] public bool lit = true;
        /// <summary>
        /// this is the color that a solid color sprite will be drawn as
        /// </summary>
        [Field][JsonProperty] public Color color = Color.Blue;
        /// <summary>
        /// this determines what layer the sprite will be drawn in, ie -1 for bckground and 1 for on top of that.
        /// </summary>
        [Field][JsonProperty] public float drawOrder = 0f;
        /// <summary>
        /// this prevents the image/color data from being overwritten or changed.
        /// </summary>
        [Field][JsonProperty] public bool IsReadOnly = false;

        internal JImage lightmap;
        public JImage GetLightmap()
        {
            var light = FirstLight;

            var data = texture.GetImage();

            if (light is null)
                return data;

            if (!node.TryGetComponent<Collider>(out var col))
                return data;

            if (data is not null)
            {
                if (!IsDirty && lightmap is not null)
                    return lightmap;

                IsDirty = false;
                lightmap = VertexLighting(light);
            }

            return lightmap; 
        }
        public Sprite()
        {
            texture = new Texture(Vector2.One, Color.Red);
        }
        public Sprite(int x, int y) : this()
        {
            Scale = new(x, y);
        }
        [Method]
        public async Task SetFileAsTexture()
        {
            var task = Interop.GetSelectedFileMetadataAsync();
            await task;
            string path = task.Result.Path;
            
            if(path != null)
                texture.SetImage(path);
        }
        [Method]
        public void CycleSpriteType()
        {
            switch (Type)
            {
                case ImageType.SolidColor:
                    Type = ImageType.Image;
                    break;
                case ImageType.Image:
                    Type = ImageType.SolidColor;
                    break;
                default:
                    Type = 0;
                    break;
            }

        }
        [Method]
        private void Refresh()
        {
            switch (Type)
            {
                case ImageType.SolidColor:
                    Color[,] colorArray = CBit.SolidColorSquare(Scale, color);
                    texture.SetImage(colorArray);
                    break;
                case ImageType.Image:
                    if (texture.imgData != null)
                        texture.SetImage(texture.imgData.Path);
                    break;
            }
            IsDirty = false;
        }
        public override void Awake()
        {
            Refresh();
        }
        public override void Update()
        {
            if (IsDirty)
                Refresh();

            if (lit)
               lightmap =  GetLightmap();
        }
        public override void OnDrawShapes()
        {
            if (selected_by_editor)
            {
                Polygon mesh = new(GetCorners());
                int vertLength = mesh.vertices.Length;
                for (int i = 0; i < vertLength; i++)
                {
                    var nextIndex = (i + 1) % vertLength;
                    Interop.DrawLine(mesh.vertices[i], mesh.vertices[nextIndex], System.Drawing.Color.Orange);
                }
            }
        }
        public static Light? FirstLight
        {
            get
            {
                var lights = Interop.Stage.GetAllComponents<Light>();
                if (!lights.Any())
                    return null;
                return lights.First();
            }
        }
        public void LightingPerPixel(Light light)
        {
            int x = 0, y = 0;

            Color color = Color.White;
            PixelShader((e) => { color = e; }, getColor, X, Y);

            void OnShadingComplete(JImage image)
            {
                texture.SetImage(image);
            }

            Color getColor()
            {
                // color 
                var localPos = new Vector2(x, y) / texture.size;
                var global = LocalToGlobal(localPos);
                float distance = Vector2.Distance(global, light.Position);
                float lightAmount = 0f - Math.Clamp(distance / light.radius, 0, 1);
                Color blendedPixel = Color.Blend(color, light.color, lightAmount) * distance;
                return blendedPixel;
            };

            int X() => x++;
            int Y() => y++;
        }
        JImage VertexLighting(Light light)
        {
            JImage image = new(); 
            for (int x = 0; x < texture.Width; x++)
                for (int y = 0; y < texture.Height; y++)
                {
                    var localPos = new Vector2(x, y) / texture.size;
                    var global = LocalToGlobal(localPos);

                    float distance = Vector2.Distance(global, light.Position);

                    float lightAmount = 1f - Math.Clamp(distance / light.radius, 0, 1);

                    Color existingPixel = texture.GetPixel(x, y);
                    
                    if (existingPixel.a == 0)
                    {
                        image.SetPixel(x, y, existingPixel);
                        continue;
                    }

                    Color blendedPixel;

                    if (lightAmount > 0)
                    {
                        lightAmount = (float)CMath.Negate((double)lightAmount);
                        blendedPixel = Color.Blend(existingPixel, Color.Black, lightAmount);
                    }
                    else blendedPixel = Color.Blend(existingPixel, light.color, Math.Clamp(lightAmount, 0, 1));

                    image.SetPixel(x, y, blendedPixel);
                }
            return image; 
        }
        /// <summary>
        /// see LightingPerPixel to see an example 
        /// </summary>
        /// <param name="colorOut"></param>
        /// <param name="colorIn"></param>
        /// <param name="indexerX"></param>
        /// <param name="indexerY"></param>
        /// <param name="onIteraton"></param>
        public virtual void PixelShader(Action<Color> colorOut, Func<Color> colorIn, Func<int> indexerX, Func<int> indexerY)
        {
            for (int x = 0; x < texture.Width * 4; x = indexerX.Invoke())
                for (int y = 0; y < texture.Height; y = indexerY.Invoke())
                {
                    var col = texture.GetPixel(x, y);
                    colorOut.Invoke(col);
                    texture.SetPixel(x, y, colorIn.Invoke());
                }
        }
        public Vector2[] GetCorners()
        {
            var viewport = Polygon.Square(1);
            viewport.Transform(Transform);
            return viewport.vertices;
        }
        internal void SetImage(Color[,] colors) => texture.SetImage(colors);
        public void SetImage(Vector2 size, byte[] color)
        {
            texture.SetImage(size, color);
        }
        public void SetImage(JImage? image)
        {
            if (image is not null)
            {
                Vector2 size = new(image.width, image.height);
                SetImage(size, image.data);
            }
        }

        public override void Dispose()
        {
        }
    }
}
