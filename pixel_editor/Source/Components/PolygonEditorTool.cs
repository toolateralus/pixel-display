using pixel_renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace pixel_editor
{
    internal class PolygonEditorTool : Tool
    {
        Node? lastNode;
        public override void Awake() { }

        public override void Update(float delta)
        {
            //Editor
            if (Input.GetInputValue(InputEventType.KeyDown, "LeftCtrl") &&
                Input.GetInputValue(InputEventType.KeyDown, "LeftShift"))
            {

            }
        }
    }
}
