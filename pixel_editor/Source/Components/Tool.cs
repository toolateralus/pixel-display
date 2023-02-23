using System.Windows.Controls.Primitives;

namespace pixel_editor
{
    public abstract class Tool
    {
        public abstract void Awake();
        public abstract void Update(float delta);
    }
}
