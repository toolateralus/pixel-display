using System;
using System.Numerics;

namespace pixel_renderer
{
    public partial class Node 
    {
        public class TransformComponent : Component
        {
            [Field] public Vector2 position;
            [Field] public Vector2 scale;
            [Field] public float rotation;
            [Field] public string name;

            public override void Dispose()
            {
                throw new NotImplementedException();
            }

            public override void OnFieldEdited(string field)
            {
                switch (field)
                {
                    case nameof(name):
                        node.Name = name;
                        break;
                    case nameof(position):
                        Position = position;
                        break;
                    case nameof(scale):
                        Scale = scale;
                        break;
                    case nameof(rotation):
                        Rotation = rotation;
                        break;
                }
            }
        }
        
    }
}
