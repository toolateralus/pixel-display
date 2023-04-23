using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Newtonsoft.Json;
using pixel_renderer.Assets;
using pixel_renderer.FileIO;
using pixel_renderer.ShapeDrawing;

namespace pixel_renderer
{
    public enum ImageType { SolidColor, Image, Custom};
    public enum TextureFiltering { Point, Bilinear }
    public class Sprite : Component
    {
        /// <summary>
        /// color data is refreshed from source on update if this is true
        /// </summary>
        internal protected bool IsDirty = true;
        [Field] [JsonProperty] public Vector2 viewportScale = new(1, 1);
        [Field] [JsonProperty] public Vector2 viewportOffset = new(0.0f, 0.0f);
        [Field] [JsonProperty] public float camDistance = 1;
        /// <summary>
        /// stores color data
        /// </summary>
        [Field] [JsonProperty] public Texture texture;
        /// <summary>
        /// this determines what source the color data will come from
        /// </summary>
        [Field] [JsonProperty] public ImageType Type = ImageType.SolidColor;
        /// <summary>
        /// this dictates how the renderer filters the sprite during drawing
        /// </summary>
        [Field] [JsonProperty] public TextureFiltering textureFiltering = 0;
        /// <summary>
        /// this will toggle participation in lighting
        /// </summary>
        [Field] [JsonProperty] public bool lit = false;
        /// <summary>
        /// this is the color that a solid color sprite will be drawn as
        /// </summary>
        [Field] [JsonProperty] public Pixel color = Pixel.Blue;
        /// <summary>
        /// this determines what layer the sprite will be drawn in, ie -1 for bckground and 1 for on top of that.
        /// </summary>
        [Field] [JsonProperty] public float drawOrder = 0f;
        /// <summary>
        /// this prevents the image/color data from being overwritten or changed.
        /// </summary>
        [Field] [JsonProperty] public bool IsReadOnly = false;
        private void ApplyLighting()
        {
            var light = FirstLight;
            var data = texture?.GetImage();
            if (light is null)
                return;
            if (!node.TryGetComponent<Collider>(out var col))
                return; 

            JImage lightmap; 
            if (data is not null)
            {
                Pixel[,] colors = VertexLighting(light);
                lightmap = new(colors);
                texture?.SetImage(lightmap);
                IsDirty = true;
            }
        }
        public Sprite()
        {
            texture = new Texture(Vector2.One, Pixel.Red);
        }
        public Sprite(int x, int y) : this()
        {
            Scale = new(x, y);
        }
        [Method]
        public async Task SetFileAsTexture()
        {
            EditorEvent e = new("$nolog_get_selected_meta");
            object? asset = null;
            e.action = (e) => { asset = e.First(); };
            Runtime.RaiseInspectorEvent(e);
            float time = 0;

            while (!e.processed && time < 1500)
            {
                if (asset != null && asset is Metadata meta)
                {
                    texture.SetImage(meta.Path);
                    return;
                }
                time += 15f;
                await Task.Delay(15);
            }
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
                default: Type = 0;
                    break;
            }

        }
        [Method]
        private void Refresh()
        {
            switch (Type)
            {
                case ImageType.SolidColor:
                    Pixel[,] colorArray = CBit.SolidColorSquare(Scale, color);
                    texture.SetImage(colorArray);
                    break;
                case ImageType.Image:
                    if(texture.imgData != null)
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
                ApplyLighting();
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
                    ShapeDrawer.DrawLine(mesh.vertices[i], mesh.vertices[nextIndex], Runtime.Current.projectSettings.EditorHighlightColor);
                }
            }
        }
        public static Light? FirstLight
        {
            get
            {
                var lights = Runtime.Current.GetStage().GetAllComponents<Light>();
                if (!lights.Any())
                    return null;
                return lights.First();
            }
        }
        public void LightingPerPixel(Light light)
        {
            int x = 0, y = 0;

            Pixel color = Pixel.White;
            PixelShader((e) => { color = e; }, getColor, X, Y);

            void OnShadingComplete(JImage image) {
                texture.SetImage(image);
            }

            Pixel getColor() {
                // color 
                var localPos = new Vector2(x, y) / texture.scale;
                var global = LocalToGlobal(localPos);
                float distance = Vector2.Distance(global, light.Position);
                float lightAmount = 0f - Math.Clamp(distance / light.radius, 0, 1);
                Pixel blendedPixel = Pixel.Lerp(color, light.color, lightAmount) * distance;
                return blendedPixel;
            };

            int X() => x++;
            int Y() => y++; 
        }
        Pixel[,] VertexLighting(Light light)
        {
            Pixel[,] colors = new Pixel[texture.Width, texture.Height]; 
            for (int x = 0; x < texture.Width; x++)
                for (int y = 0; y < texture.Height; y++)
                {
                    var localPos = new Vector2(x, y) / texture.scale;
                    var global = LocalToGlobal(localPos);

                    float distance = Vector2.Distance(global, light.Position);

                    float lightAmount = 0f - Math.Clamp(distance / light.radius, 0, 1);

                    Pixel existingPixel = texture.GetPixel(x, y);
                    Pixel blendedPixel;

                    if (lightAmount > 0)
                    {
                        lightAmount = (float)CMath.Negate((double)lightAmount);
                        blendedPixel = Pixel.Lerp(existingPixel, Pixel.Black, lightAmount);
                    }
                    blendedPixel = Pixel.Lerp(existingPixel, light.color, lightAmount);
                    colors[x, y] = blendedPixel;
                }
            return colors; 
        }
        /// <summary>
        /// see LightingPerPixel to see an example 
        /// </summary>
        /// <param name="colorOut"></param>
        /// <param name="colorIn"></param>
        /// <param name="indexerX"></param>
        /// <param name="indexerY"></param>
        /// <param name="onIteraton"></param>
        public virtual void PixelShader(Action<Pixel> colorOut, Func<Pixel> colorIn,  Func<int> indexerX, Func<int> indexerY)
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
        internal void SetImage(Pixel[,] colors) => texture.SetImage(colors);
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
