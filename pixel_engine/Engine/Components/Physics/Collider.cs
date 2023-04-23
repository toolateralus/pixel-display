using Newtonsoft.Json;
using pixel_renderer.ShapeDrawing;
using System.Drawing;

namespace pixel_renderer
{

    public class Collider : Component
    {

        [JsonProperty] private Polygon model = Polygon.Square(1);
        [Field] int vertexCount = 6;
        [Method] void CreateNGonWithVertexCt() => model = Polygon.nGon(Scale.X - Scale.Y * 2, vertexCount);
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
        [JsonProperty] Polygon transformedModel = Polygon.Square(1);
        [JsonProperty] public Polygon Polygon
        {
            get
            {
                model.CopyTo(ref transformedModel);
                transformedModel.Transform(Transform);
                return transformedModel;
            }
        }
        [JsonProperty][Field] public bool drawCollider = false;
        [JsonProperty][Field] public bool drawNormals = false;
        [JsonProperty][Field] public Pixel colliderPixel = Color.LimeGreen;
        [JsonProperty] public bool IsTrigger { get; internal set; } = false;
        enum PrimitiveType { Box, Circle, Triangle };
        [JsonProperty]
        PrimitiveType primitive; 
        [Method]
        void CyclePrimitiveType()
        {
            switch (primitive)
            {
                case PrimitiveType.Box:

                    SetModel(pixel_renderer.Polygon.Triangle(1, 1));
                    primitive = PrimitiveType.Triangle;

                    break;
                case PrimitiveType.Circle:

                    SetModel(pixel_renderer.Polygon.Rectangle(new(1,1)));
                    primitive = PrimitiveType.Box;

                    break;
                case PrimitiveType.Triangle:

                    SetModel(pixel_renderer.Polygon.nGon(size: 1, sides: 32));
                    primitive = PrimitiveType.Circle;
                    break;
                default:
                    break;
                    
            }
            Runtime.Log($"{node.Name}'s collider now has primitive shape : {primitive}");
        }
        public override void Dispose()
        {
        }
        [JsonProperty]
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
