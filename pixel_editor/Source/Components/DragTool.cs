using pixel_renderer;

namespace pixel_editor
{
    public class DragTool : Tool
    {
        public Node? selected;
        private Vec2 mouseSelectedNodeOffset;

        public override void Awake()
        {
            CMouse.OnLeftPressedThisFrame += SetSelectedNodeOffest;
        }
        private void SetSelectedNodeOffest()
        {
            if (selected == null)
                return;
            mouseSelectedNodeOffset = selected.Position - CMouse.GlobalPosition;
        }
        public override void Update(float delta)
        {
            selected = Editor.Current.Selected; 
            if (CMouse.Left && selected != null)
                selected.Position = CMouse.GlobalPosition + mouseSelectedNodeOffset;
        }
    }
}
