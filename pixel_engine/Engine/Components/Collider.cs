

using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Windows.Documents;

namespace pixel_renderer
{
    public class Collider : Component
    {
        [JsonProperty] Polygon polygon = new();
        public Polygon Polygon
        {
            get => polygon.OffsetBy(parent.Position);
            set => polygon = new(value.vertices);
        }
        [JsonProperty] [Field] public Vec2 scale = new(1,1);
        [JsonProperty] [Field] public TriggerInteraction InteractionType = TriggerInteraction.All;
        [Field] public bool drawCollider = false;
        [Field] public bool drawNormals = false;
        [Field] public Pixel colliderPixel = Color.LimeGreen;
        public Polygon GetUntransformedPolygon() => polygon;
        [JsonProperty]public bool IsTrigger { get; internal set; } = false;
        public override void OnDrawShapes()
        {
            if(drawCollider)
                DrawCollider();
            if(drawNormals)
                DrawNormals();
        }

        public void DrawCollider()
        {
            var poly = Polygon;
            int vertLength = poly.vertices.Length;
            for (int i = 0; i < vertLength; i++)
            {
                var nextIndex = (i + 1) % vertLength;
                ShapeDrawer.DrawLine(poly.vertices[i], poly.vertices[nextIndex], colliderPixel);
            }
        }

        public void DrawNormals()
        {
            var poly = Polygon;
            int vertLength = poly.vertices.Length;
            for (int i = 0; i < vertLength; i++)
            {
                var nextIndex = (i + 1) % vertLength;
                var midpoint = (poly.vertices[i] + poly.vertices[nextIndex]) / 2;
                ShapeDrawer.DrawLine(midpoint, midpoint + (poly.normals[i] * 10), Color.Blue);
            }
        }

        internal void SetVertices(Vec2[] vertices)
        {
            polygon = new Polygon(vertices);
        }
    }
}
