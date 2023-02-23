using pixel_renderer;
using System;
using System.Threading.Tasks;
using System.Windows.Input; 
namespace pixel_editor
{
    public class NodeRelationshipTool : Tool
    {
        private bool busy;
        public override void Awake()
        {
        }
        public override async void Update(float delta)
        {
            if (Editor.Current.Selected is Node selected)
                if (CMouse.Left && !busy)
                {
                    while (Input.GetInputValue(Key.RightShift))
                    {
                        busy = true; 
                        await Task.Delay(10);
                        // TODO: add OnDrawShapes flag so a line is drawn from the parent to the cursor pos .
                        if (CMouse.Left)
                            if (Editor.Current.Selected is Node other)
                            {
                                Child(selected, other);
                                
                                break;
                            }
                    }
                }
                busy = false; 
        }

        private void Child(Node child, Node parent) => parent.Child(child);
            
    }
}
