﻿using Newtonsoft.Json;
using pixel_renderer.ShapeDrawing;
using System.Drawing;

namespace pixel_renderer
{

    public class Collider : Component
    {

        [JsonProperty] private Polygon model = Polygon.UnitSquare();
        /// <returns>Copy of model, not model itself</returns>
        public Polygon GetModel() => new(model);
        /// <summary>
        /// Sets the model to a copy of input.
        /// Assumes input polygon has already calculated normals
        /// </summary>
        /// <param name="polygon"></param>
        public void SetModel(Polygon polygon)
        {
            model = new(polygon);
            transformedModel = new(polygon);
        }
        Polygon transformedModel = Polygon.UnitSquare();
        public Polygon Polygon
        {
            get
            {
                model.CopyTo(ref transformedModel);
                transformedModel.Transform(Transform);
                return transformedModel;
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
                    SetModel(new Triangle().Polygon);
                    primitive = PrimitiveType.triangle;
                    break;
                case PrimitiveType.circle:
                    SetModel(new Box().Polygon);
                    primitive = PrimitiveType.box;
                    break;
                case PrimitiveType.triangle:
                    SetModel(new Circle().Polygon);
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
    }
}
