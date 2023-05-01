using Newtonsoft.Json;
using Pixel.Types.Components;
using Pixel.Types.Physics;
using System.Drawing;

namespace Pixel
{

    public enum PrimitiveType { Box, Circle, Triangle };
    public class Collider : Component
    {

        [JsonProperty]
        private PrimitiveType primitive;

        [JsonProperty]
        private Polygon model = Polygon.Square(1);

        [Field]
        // this is for the nGon editor method
        int vertexCount = 6;

        /// <summary>
        /// Sets the model to a copy of input.
        /// Assumes input polygon has already calculated normals
        /// </summary>
        /// <param name="polygon"></param>
        [JsonProperty]
        Polygon transformedModel = Polygon.Square(1);

        [JsonProperty]
        public Polygon Polygon
        {
            get
            {
                model.CopyTo(ref transformedModel);
                transformedModel.Transform(Transform);
                return transformedModel;
            }
        }

        [Field]
        [JsonProperty]
        public bool drawCollider = true;

        [Field]
        [JsonProperty]
        public bool drawNormals = false;

        [Field]
        [JsonProperty]
        public Color colliderPixel = System.Drawing.Color.LimeGreen;

        [JsonProperty]
        public bool IsTrigger { get; internal set; } = false;

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

        public void SetModel(Polygon polygon)
        {
            model = new(polygon);
            transformedModel = new(polygon);
        }

        [Method]
        // for editor.
        void CreateNGonWithVertexCt() => model = Polygon.nGon(Scale.X - Scale.Y * 2, vertexCount);

        /// <returns>a copy of the colliders' model, not the model itself</returns>
        public Polygon GetModel() => new(model);
        [Method]
        void CyclePrimitiveType()
        {
            switch (primitive)
            {
                case PrimitiveType.Box:

                    SetModel(Polygon.Triangle(1, 1));
                    primitive = PrimitiveType.Triangle;

                    break;
                case PrimitiveType.Circle:

                    SetModel(Polygon.Rectangle(new(1, 1)));
                    primitive = PrimitiveType.Box;

                    break;
                case PrimitiveType.Triangle:

                    SetModel(Polygon.nGon(size: 1, sides: 32));
                    primitive = PrimitiveType.Circle;
                    break;
                default:
                    break;

            }
            Interop.Log($"{node.Name}'s collider now has primitive shape : {primitive}");
        }
        public override void Dispose()
        {
        }

        public override void on_draw_shapes_internal()
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
                Interop.DrawLine(poly.vertices[i], poly.vertices[nextIndex], colliderPixel);
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
                Interop.DrawLine(midpoint, midpoint + poly.normals[i] * 1, System.Drawing.Color.Blue);
            }
        }
    }
}
