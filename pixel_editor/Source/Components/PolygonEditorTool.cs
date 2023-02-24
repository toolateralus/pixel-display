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
        List<Vec2> highlightedVertices = new List<Vec2>();
        Collider? selectedCollider;
        const float minHighlightDistance = 2;
        float vertSize = 2;
        public override void Awake() { }
        public override void OnDrawShapes()
        {
            if (selectedCollider == null)
                return;
            selectedCollider.DrawCollider();
            selectedCollider.DrawNormals();
            for(int i = 0; i < highlightedVertices.Count; i++)
            {
                Vec2 vert = highlightedVertices[i];
                ShapeDrawer.DrawCircle(vert, vertSize, System.Drawing.Color.GreenYellow);
            }
        }

        public override void Update(float delta)
        {
            highlightedVertices.Clear();
            selectedCollider = GetSelectedCollider();
            if (selectedCollider == null)
                return;
            Vec2[] vertices = selectedCollider.Polygon.vertices;
            Vec2 mPos = CMouse.GlobalPosition;
            Vec2 closestVert = vertices.OrderBy((v) => v.SqrDistanceFrom(mPos)).First();
            if (closestVert.SqrDistanceFrom(mPos) < minHighlightDistance * minHighlightDistance)
            {
                highlightedVertices.Add(closestVert);
                vertSize = 4;
            }
            else
            {
                foreach (Vec2 vert in vertices)
                    highlightedVertices.Add(vert);
                vertSize = 2;
            }
        }

        Collider? GetSelectedCollider()
        {
            if (Runtime.Current.GetStage() is not Stage stage ||
                !Input.GetInputValue(InputEventType.KeyDown, "LeftCtrl") ||
                !Input.GetInputValue(InputEventType.KeyDown, "LeftShift"))
                return null;
            if (Editor.Current.Selected is not Node node)
                node = stage.GetNodesAtGlobalPosition(CMouse.GlobalPosition).FirstOrDefault();
            return node?.GetComponent<Collider>();
        }
    }
}
