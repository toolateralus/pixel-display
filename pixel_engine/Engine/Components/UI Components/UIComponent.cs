using Newtonsoft.Json;
using System.Drawing;
using System.Numerics;

namespace pixel_renderer
{
   
    public abstract class UIComponent : Component
    {
        [Field][JsonProperty] public float drawOrder = 0f;
        public Vector2 Center { get => Transform.Translation; set => Transform.Translation = value; }
        public Vector2 Size
        {
            get => new(Transform.M11, Transform.M22);
            set
            {
                Transform.M11 = value.X;
                Transform.M22 = value.Y;
            }
        }

        public abstract void Draw(RendererBase renderer); 

        internal Vector2 GlobalToLocal(Vector2 global)
        {
            Matrix3x2.Invert(Transform, out var inverted);
            return Vector2.Transform(global, inverted);
        }
        
        public Vector2 LocalToGlobal(Vector2 local) => Vector2.Transform(local, Transform);
        public Vector2[] GetCorners()
        {
            return new Vector2[]
            {
                    Vector2.Transform(new Vector2(-0.5f, -0.5f), Transform), // Top Left
                    Vector2.Transform(new Vector2(0.5f, -0.5f), Transform), // Top Right
                    Vector2.Transform(new Vector2(0.5f, 0.5f), Transform), // Bottom Right
                    Vector2.Transform(new Vector2(-0.5f, 0.5f), Transform), // Bottom Left
            };
        }
    }
}