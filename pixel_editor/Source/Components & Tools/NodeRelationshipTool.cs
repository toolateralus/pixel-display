using pixel_renderer;
using pixel_renderer.ShapeDrawing;
using System;
using System.Threading.Tasks;
using System.Windows.Input; 
namespace pixel_editor
{
    public class NodeRelationshipTool : Tool
    {
        private bool busy;
        Node? parent;
        private bool shouldDraw;

        public override void Awake()
        {
            ShapeDrawer.DrawShapeActions += OnDrawShapes;
        }
        public override async void Update(float delta)
        {
            if (Editor.Current.LastSelected is Node _parent)
            {
                if (CMouse.LeftPressedThisFrame && Input.Get(Key.RightShift) && !busy)
                {
                    parent = _parent;
                    busy = true;

                    while (busy)
                    {
                        shouldDraw = true;
                        await Task.Delay(30);
                        if (!Input.Get(Key.RightShift))
                        {
                            ResetTool();
                            break;
                        }
                        if (CMouse.Left &&  parent != null && Editor.Current.LastSelected is Node other && other != parent)
                        {
                            Child(other, parent);
                            Console.Print($"{parent.Name} had child {other.Name} added to it");
                            ResetTool();
                            break;
                        }
                    }
                }
            }
            else ResetTool();
           
        }

        private void ResetTool()
        {
            busy = false;
            shouldDraw = false;
            parent = null;
        }

        public override void OnDrawShapes()
        {
            if (parent is null || !shouldDraw)
                return; 
            ShapeDrawer.DrawLine(parent.Position, CMouse.GlobalPosition);
        }

        private void Child(Node child, Node parent) => parent.Child(child);
            
    }
}
