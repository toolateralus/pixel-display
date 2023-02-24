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
            if (vertices.Count() == 0)
                return;
            Vec2 mPos = CMouse.GlobalPosition;
            Vec2 cPos = selectedCollider.parent.Position;
            int closest = 0;
            for(int i = 1; i < vertices.Count(); i++)
                if (vertices[i].SqrDistanceFrom(mPos) <
                    vertices[closest].SqrDistanceFrom(mPos))
                    closest = i;
            if (vertices[closest].SqrDistanceFrom(mPos) < minHighlightDistance * minHighlightDistance)
            {
                vertices[closest] = mPos;
                selectedCollider.Polygon = new Polygon(vertices).OffsetBy(cPos * -1);
                highlightedVertices.Add(vertices[closest]);
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
                !Input.GetInputValue(Key.LeftCtrl) ||
                !Input.GetInputValue(Key.LeftShift))
                return null;
            return Editor.Current.Selected?.GetComponent<Collider>();
        }
    }
}
