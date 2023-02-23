using pixel_renderer;
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
            if (Editor.Current.Selected is Node _parent)
            {
                parent = _parent; 
                if (CMouse.Left && !busy)
                {
                    busy = true;
                    shouldDraw = true;
                    while (busy)
                    {
                        if (!Input.GetInputValue(Key.RightShift))
                            break;

                        if (CMouse.Left && Editor.Current.Selected is Node other && other != parent)
                        {
                            Child(other);
                            return;
                        }
                        await Task.Delay(250);
                    }
                }
                  
                busy = false;
            }
           
        }

        private void Child(Node other)
        {
            Child(parent, other);
            Console.Print($"{parent.Name} had child {other.Name} added to it");
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
