﻿using Newtonsoft.Json;
using pixel_renderer.ShapeDrawing;
using System.Drawing;

namespace pixel_renderer
{

    public class Collider : Component
    {

        [JsonProperty] public Polygon model = new();
        public Polygon Polygon
        {
            get
            {
                if (model is null)
                    model = new Box().Polygon;
                return model.Transformed(Transform);
            }
        }

        [JsonProperty][Field] public TriggerInteraction InteractionType = TriggerInteraction.All;

        [Field] public bool drawCollider = false;
        [Field] public bool drawNormals = false;
        [Field] public Pixel colliderPixel = Color.LimeGreen;

        [JsonProperty] public bool IsTrigger { get; internal set; } = false;


        enum PrimitiveType { box, circle, triangle };
        PrimitiveType primitive; 
        [Method]
        void CyclePrimitiveType()
        {
            switch (primitive)
            {
                case PrimitiveType.box:
                    model = new Triangle().Polygon;
                    primitive = PrimitiveType.triangle;
                    model.CalculateNormals(); 
                    break;
                case PrimitiveType.circle:
                    model = new Box().Polygon;
                    model.CalculateNormals();
                    primitive = PrimitiveType.box;
                    break;
                case PrimitiveType.triangle:
                    model = new Circle().Polygon;
                    model.CalculateNormals();
                    primitive = PrimitiveType.circle;
                    break;
                default:
                    break;
                    
            }
            Runtime.Log($"{node.Name} : Primitive = {primitive}");
        }


        private BoundingBox2D boundingBox = new();
        public BoundingBox2D BoundingBox
        {
            get
            {
                Polygon.GetBoundingBox(ref boundingBox);
                return boundingBox;
            }
        }

        public override void OnDrawShapes()
        {
            if (drawCollider)
                DrawCollider();
            if (drawNormals)
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
                ShapeDrawer.DrawLine(midpoint, midpoint + poly.normals[i] * 1, Color.Blue);
            }
        }
        public void SetPolygonFromWorldSpace(Polygon worldspacePolygon)
        {
            model = worldspacePolygon.Transformed(Transform.Inverted());
        }
    }
}
