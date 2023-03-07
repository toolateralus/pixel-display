

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

    public class Collider : Component
    {
        
        [JsonProperty] public Polygon untransformedPolygon;
        public Polygon Polygon
        {
            get
            {
                if (untransformedPolygon is null)
                    untransformedPolygon = new Box().DefiningGeometry; 
                return untransformedPolygon.Transformed(Transform);
            }
        }

        [JsonProperty] [Field] public TriggerInteraction InteractionType = TriggerInteraction.All;
        
        [Field] public bool drawCollider = false;
        [Field] public bool drawNormals = false;
        [Field] public Pixel colliderPixel = Color.LimeGreen;
        
        [JsonProperty] public bool IsTrigger { get; internal set; } = false;
        
        private BoundingBox2D? boundingBox;
        public BoundingBox2D BoundingBox
        {
            get
            {
                if (boundingBox == null && Polygon.vertices != null)
                    boundingBox = Polygon.GetBoundingBox(Polygon.vertices);
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
            BoundingBox2D box = new(poly.vertices);
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
        public void SetPolygonFromWorldSpace(Polygon worldspacePolygon)
        {
            untransformedPolygon = worldspacePolygon.Transformed(Transform.Inverted());
        }
    }
}
