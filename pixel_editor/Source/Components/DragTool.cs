using pixel_renderer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Windows.Documents;
using System.Xml.Linq;

namespace pixel_editor
{
    public class DragTool : Tool
    {
        private Vector2 mouseSelectedNodeOffset;

        public override void Awake()
        {
            CMouse.OnLeftPressedThisFrame += SetSelectedNodeOffest;
        }
        private void SetSelectedNodeOffest()
        {
            var selected = Editor.Current.LastSelected;

            switch (Editor.Current.ActivelySelected.Count)
            {
                case > 0 when selected == null:
                    {
                        foreach (var x in Editor.Current.ActivelySelected)
                        {
                            if (!x.TryGetComponent<Collider>(out var c)) continue;
                            c.drawCollider = false;
                            c.drawNormals = false;
                            c.colliderPixel = Color.Blue;
                        }
                        Editor.Current.ActivelySelected.Clear();
                        break;
                    }
                case > 0:
                    if (!Editor.Current.ActivelySelected.Contains(selected))
                    {
                        draggingMultiple = false;
                        return;
                    }
                    DragMultipleNodes();
                    break;
                default:
                    mouseSelectedNodeOffsets.Clear();
                    draggingMultiple = false;
                    break;
            }

            if (selected == null)
                return;

            mouseSelectedNodeOffset = selected.Position - CMouse.GlobalPosition;
        }

        private void DragMultipleNodes()
        {
            foreach (var x in Editor.Current.ActivelySelected)
                mouseSelectedNodeOffsets.Add(x.Position - CMouse.GlobalPosition);

            draggingMultiple = true;
        }

        List<Vector2> mouseSelectedNodeOffsets = new();
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
                        if (node.Position is Vector2 vec)
                            if (vec.IsWithin(boxStart, boxEnd))
                            {
                                if (node.TryGetComponent(out Collider sprite))
                                {
                                    Editor.Current.ActivelySelected.Add(node);
                                    sprite.drawCollider = true;
                                    sprite.drawNormals = true;
                                    sprite.colliderPixel = Color.Orange;
                                }
                            }
                }

                InBoxSelect = false;
            }
        }
        Vector2 boxStart;
        Vector2 boxEnd;
        public bool InBoxSelect { get; private set; } = false;
        private bool draggingMultiple;

        public override void OnDrawShapes()
        {
          
              
            if (!InBoxSelect) return;
            ShapeDrawer.DrawRect(boxStart, boxEnd, Pixel.Green);
            ShapeDrawer.DrawCircle(boxEnd, 3, Pixel.Red);

            var stage = Runtime.Current.GetStage();
            List<Node> nodes = new(stage.nodes);

            if (stage is null)
                return;
        }
    }

}
