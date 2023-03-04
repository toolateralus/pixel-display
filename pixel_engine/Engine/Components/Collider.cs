

using Newtonsoft.Json;
using pixel_renderer.ShapeDrawing;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Windows.Documents;
using System.Windows.Shapes;

namespace pixel_renderer
{
    public interface IPrimitiveGeometry
    {
        public static Polygon DefiningGeometry { get; }
    }
    public class Box : IPrimitiveGeometry
    {
        public static Vector2 DefaultSize;
        private static readonly Polygon DefaultPolygon = Polygon.Rectangle(DefaultSize.X, DefaultSize.Y);

        public Polygon Polygon = DefaultPolygon;
        public Polygon DefiningGeometry => Polygon;
    }
    public class Triangle : IPrimitiveGeometry
    {
        public static Vector2 DefaultSize;
        private static readonly Polygon DefaultPolygon = Polygon.Triangle(DefaultSize.X, DefaultSize.Y);

        public Polygon Polygon = DefaultPolygon;
        public Polygon DefiningGeometry => Polygon;
    }
    public class Circle : IPrimitiveGeometry
    {
        public static Vector2 DefaultSize;
        private static readonly Polygon DefaultPolygon = Polygon.Circle(DefaultSize.X, 8);

        public Polygon Polygon = DefaultPolygon;
        public Polygon DefiningGeometry => Polygon;
    }



    public class Collider : Component
    {
        [JsonProperty] Polygon polygon = new();
        public Polygon Polygon
        {
            get => polygon.OffsetBy(node.Position);
            set => polygon = new(value.vertices);
        }
        [JsonProperty] [Field] public Vector2 scale = new(1,1);
        [JsonProperty] [Field] public TriggerInteraction InteractionType = TriggerInteraction.All;
        [Field] public bool drawCollider = false;
        [Field] public bool drawNormals = false;
        [Field] public Pixel colliderPixel = Color.LimeGreen;
        public Polygon GetUntransformedPolygon() => polygon;
        [JsonProperty]public bool IsTrigger { get; internal set; } = false;
        private BoundingBox2D? boundingBox;
        public BoundingBox2D BoundingBox
        {
            get
            {
                if (boundingBox == null && polygon?.vertices != null)
                    boundingBox = Polygon.GetBoundingBox(polygon.vertices);
                return boundingBox ?? default;
            }
        }

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

        internal void SetVertices(Vector2[] vertices)
        {
            polygon = new Polygon(vertices);
            boundingBox = null; 
        }
    }
}
