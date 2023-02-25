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
        List<Vec2> highlightedVertices = new();
        Collider? selectedCollider;
        const float minHighlightDistance = 2;
        int selectedVertIndex = -1;
        float vertSize = 2;
        public override void Awake() { }
        public override void OnDrawShapes()
        {
            if (selectedCollider == null)
                return;
            selectedCollider.DrawCollider();
            selectedCollider.DrawNormals();
            for (int i = 0; i < highlightedVertices.Count; i++)
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
            if (vertices.Length == 0)
                return;
            Vec2 mPos = CMouse.GlobalPosition;
            Vec2 cPos = selectedCollider.parent.Position;
            if (selectedVertIndex == -1)
            {
                int closestVert = 0;
                for(int i = 1; i < vertices.Length; i++)
                    if (vertices[i].SqrDistanceFrom(mPos) <
                        vertices[closestVert].SqrDistanceFrom(mPos))
                        closestVert = i;
                if (vertices[closestVert].SqrDistanceFrom(mPos) < minHighlightDistance * minHighlightDistance)
                {
                    selectedVertIndex = closestVert;
                    vertices[selectedVertIndex] = mPos;
                    selectedCollider.Polygon = new Polygon(vertices).OffsetBy(cPos * -1);
                    highlightedVertices.Add(vertices[selectedVertIndex]);
                    vertSize = 4;
                }
                else
                {
                    foreach(Line line in selectedCollider.Polygon.GetLines())
                    {
                        Vec2 mouseOffset = mPos - line.startPoint;
                        float upDot = Vec2.Dot(line.Direction.Normalized(), mouseOffset);
                        //float leftDot = Vec2.Dot(line.Direction.Normal_LHS.Normalized(), mouseOffset);
                        highlightedVertices.Add(line.Direction.Normalized() * upDot + line.startPoint);
                        vertSize = 2;
                    }
                    if (highlightedVertices.Count == 0)
                    {
                        foreach (Vec2 vert in vertices)
                            highlightedVertices.Add(vert);
                        vertSize = 2;
                    }
                }
            }
            else
            {
                vertices[selectedVertIndex] = mPos;
                selectedCollider.Polygon = new Polygon(vertices).OffsetBy(cPos * -1);
                highlightedVertices.Add(vertices[selectedVertIndex]);
                vertSize = 4;
            }
        }

        Collider? GetSelectedCollider()
        {
            if (Runtime.Current.GetStage() is null ||
                !Input.GetInputValue(Key.LeftCtrl) ||
                !Input.GetInputValue(Key.LeftShift))
            {
                selectedVertIndex = -1;
                return null;
            }
            return Editor.Current.LastSelected?.GetComponent<Collider>();
        }
    }
}
