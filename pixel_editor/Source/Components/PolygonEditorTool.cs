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
        Collider? selectedCollider;
        public override void Awake() { }
        public override void OnDrawShapes()
        {
            if (selectedCollider == null)
                return;
            selectedCollider.DrawCollider();
            selectedCollider.DrawNormals();
            foreach (Vec2 vert in selectedCollider.Polygon.vertices)
            {
                ShapeDrawer.DrawCircle(vert, 2, System.Drawing.Color.GreenYellow);
            }
        }

        public override void Update(float delta)
        {
            if (Input.GetInputValue(InputEventType.KeyDown, "LeftCtrl") &&
                Input.GetInputValue(InputEventType.KeyDown, "LeftShift") &&
                Runtime.Current.GetStage() is Stage stage &&
                stage.GetNodesAtGlobalPosition(CMouse.GlobalPosition).FirstOrDefault() is Node node)
            {
                selectedCollider = node.GetComponent<Collider>();
            }
            else
            {
                selectedCollider = null;
            }
        }
    }
}
