using Pixel;
using Pixel.Assets;
using Pixel.Types.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Pixel_Engine.My_Scripts
{
    public class SpriteController : Sprite
    {
        Polygon polygon = null; 
        public override void Awake()
        {
            Type = ImageType.SolidColor;
            texture = Texture.Default;
        }
        public override void Update()
        {
            if (IsDirty)
                Refresh();
        }
        public override void OnDrawShapes()
        {
            if (selected_by_editor && polygon != null)
            {
                int vertLength = polygon.vertices.Length;
                for (int i = 0; i < vertLength; i++)
                {
                    var nextIndex = (i + 1) % vertLength;
                    Interop.DrawLine(Position + polygon.vertices[i], Position + polygon.vertices[nextIndex], System.Drawing.Color.Orange);
                }
            }
        }
        [Method]
        async void MeshColliderFromSelectedAsset()
        {
            var task = Interop.GetSelectedFileMetadataAsync();
            await task;
            var meta = task.Result;
            var poly = MeshImporter.GetPolygonFromMesh(meta);
            
            if (TryGetComponent<Collider>(out var col)) 
                col.SetModel(poly);

            this.polygon = poly;
            IsDirty = true;
        }
        [Method]
        public override void Refresh()
        {
            if (Type == ImageType.SolidColor)
            {
                if (polygon != null)
                {
                    Color[,] colorArray = CBit.SolidColorPolygon(polygon, Color);

                    if (texture.Name == Texture.Default.Name)
                        texture = new Texture(Polygon.GetBoundingBox(polygon.vertices).max, Color, "CustomSprite" + UUID);
       
                    texture.SetImage(colorArray);
                }
            }

            IsDirty = false;
        }
    }
}
