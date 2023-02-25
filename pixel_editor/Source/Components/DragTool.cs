using pixel_renderer;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Documents;

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
            var selected = Editor.Current.LastSelected;

            if (Editor.Current.ActivelySelected.Count > 0 && selected == null)
            {
                foreach (var x in Editor.Current.ActivelySelected)
                {
                    if (!x.TryGetComponent<Collider>(out var c)) continue;
                    c.drawCollider = false;
                    c.drawNormals = false;
                    c.colliderColor = Color.Blue;
                }

                Editor.Current.ActivelySelected.Clear();
            }
            else if (Editor.Current.ActivelySelected.Count > 0)
            {
                foreach (var x in Editor.Current.ActivelySelected)
                    mouseSelectedNodeOffsets.Add(x.Position - CMouse.GlobalPosition);

                draggingMultiple = true;
            }
            else
            {
                mouseSelectedNodeOffsets.Clear(); 
                draggingMultiple = false;
            }

            if (selected == null)
                return;
            mouseSelectedNodeOffset = selected.Position - CMouse.GlobalPosition;
        }
        List<Vec2> mouseSelectedNodeOffsets = new();
        public override void Update(float delta)
        {
            var selected = Editor.Current.LastSelected;
            if (CMouse.Left && selected != null)
            {
                selected.Position = CMouse.GlobalPosition + mouseSelectedNodeOffset;
                int i = 0; 


                if (draggingMultiple && mouseSelectedNodeOffsets.Count - 1 > i)
                    foreach (var x in Editor.Current.ActivelySelected )
                        x.Position = CMouse.GlobalPosition + mouseSelectedNodeOffsets[i++];

            }
            else if (CMouse.Left)
            {
                InBoxSelect = true;
                if (CMouse.LeftPressedThisFrame)
                    boxStart = CMouse.GlobalPosition;
                boxEnd = CMouse.GlobalPosition;
            }
            else
            {
                if (InBoxSelect)
                {
                    var stage = Runtime.Current.GetStage();

                    List<Node> nodes = new(stage.nodes);

                    if (stage is null)
                        return;

                    foreach (Node node in nodes)
                        if (node.Position is Vec2 vec)
                            if (vec.IsWithin(boxStart, boxEnd))
                            {
                                Editor.Current.ActivelySelected.Add(node);
                                if (node.TryGetComponent(out Collider sprite))
                                {
                                    sprite.drawCollider = true;
                                    sprite.drawNormals = true;
                                    sprite.colliderColor = Color.Orange;
                                }
                            }
                }

                InBoxSelect = false;
            }
        }
        private bool InBoxSelect = false;
        Vec2 boxStart;
        Vec2 boxEnd;
        private bool draggingMultiple;

        public override void OnDrawShapes()
        {
            if (!InBoxSelect) return;
            ShapeDrawer.DrawRect(boxStart, boxEnd, Color.Green);
            ShapeDrawer.DrawCircle(boxEnd, 3, Color.Red);

        }

    }
}
