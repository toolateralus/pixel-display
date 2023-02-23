using pixel_renderer;

namespace pixel_editor
{
    public class DragTool : Tool
    {
        private Vec2 mouseSelectedNodeOffset;

        public override void Awake()
        {
            CMouse.OnLeftPressedThisFrame += SetSelectedNodeOffest;
        }
        private void SetSelectedNodeOffest()
        {
            var selected = Editor.Current.Selected;
            if (selected == null)
                return;
            mouseSelectedNodeOffset = selected.Position - CMouse.GlobalPosition;
        }
        public override void Update(float delta)
        {
            var selected = Editor.Current.Selected; 
            if (CMouse.Left && selected != null)
                selected.Position = CMouse.GlobalPosition + mouseSelectedNodeOffset;
        }
    }
}
