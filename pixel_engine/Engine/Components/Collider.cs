

using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Windows.Documents;

namespace pixel_renderer
{
    public class Collider : Component
    {
        [JsonProperty] Polygon mesh;
        public Polygon Mesh
        {
            get
            {
                mesh ??= new(GetVertices());
                return mesh.OffsetBy(parent.Position);
            }
                
            set => mesh = value;
        }
        [JsonProperty] [Field] public Vec2 size = new(0,0);
        [JsonProperty] [Field] public Sprite? sprite;
        [JsonProperty] [Field] public TriggerInteraction InteractionType = TriggerInteraction.All;
        [Field] public bool drawCollider = false;
        [Field] public bool drawNormals = false;
        [Field] public Color colliderColor = Color.LimeGreen;
        [JsonProperty]public bool IsTrigger { get; internal set; } = false;
        /// <summary>
        /// <code>
        /// Gets the colliders corners in a list organized as such
        /// Top Left, 
        /// Top Right,
        /// Bottom Right,
        /// Bottom Left,
        /// </code>
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Vec2[] GetVertices()
        {
            Vec2 topLeft = Vec2.zero;
            Vec2 topRight = new(size.x, 0);
            Vec2 bottomRight = size;
            Vec2 bottomLeft = new(0, size.y);

            var vertices = new Vec2[]
            {
                    topLeft,
                    topRight,
                    bottomRight,
                    bottomLeft,
            };

            return vertices;
        }
        public override void OnDrawShapes()
        {
            if (!(drawCollider || drawNormals))
                return;
            var mesh = Mesh;
            int vertLength = mesh.vertices.Length;
            for (int i = 0; i < vertLength; i++)
            {
                var nextIndex = (i + 1) % vertLength;
                if(drawCollider)
                    ShapeDrawer.DrawLine(mesh.vertices[i], mesh.vertices[nextIndex], colliderColor);
                if (drawNormals)
                {
                    var midpoint = (mesh.vertices[i] + mesh.vertices[nextIndex]) / 2;
                    ShapeDrawer.DrawLine(midpoint, midpoint + (mesh.normals[i] * 10), Color.Blue);
                }
            }
        }
    }
}
