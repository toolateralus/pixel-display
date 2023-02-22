

using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Windows.Documents;

namespace pixel_renderer
{
    public class Collider : Component
    {
        [JsonProperty] Polygon? polygon;
        public Polygon Polygon
        {
            get
            {
                polygon ??= new(GetVertices());
                return polygon.OffsetBy(parent.Position);
            }
                
            set => polygon = value;
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
        public static Vec2[] GetVertices(Sprite sprite)
        {
            Vec2 topLeft = Vec2.zero;
            Vec2 topRight = new(sprite.size.x, 0);
            Vec2 bottomRight = sprite.size;
            Vec2 bottomLeft = new(0, sprite.size.y);

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
            var poly = Polygon;
            int vertLength = poly.vertices.Length;
            for (int i = 0; i < vertLength; i++)
            {
                var nextIndex = (i + 1) % vertLength;
                if(drawCollider)
                    ShapeDrawer.DrawLine(poly.vertices[i], poly.vertices[nextIndex], colliderColor);
                if (drawNormals)
                {
                    var midpoint = (poly.vertices[i] + poly.vertices[nextIndex]) / 2;
                    ShapeDrawer.DrawLine(midpoint, midpoint + (poly.normals[i] * 10), Color.Blue);
                }
            }
        }
    }
}
