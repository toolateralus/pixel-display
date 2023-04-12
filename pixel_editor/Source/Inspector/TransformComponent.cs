using pixel_renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace pixel_editor
{
    internal class TransformComponent : Component
    {
        [Field] public Vector2 position;
        [Field] public Vector2 scale;
        [Field] public float rotation;
        [Field] public string name; 

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
