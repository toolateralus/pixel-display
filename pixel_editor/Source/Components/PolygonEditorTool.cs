using pixel_renderer;
using pixel_renderer.Engine.Components.Physics;
using pixel_renderer.ShapeDrawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace pixel_editor
{
    internal class PolygonEditorTool : Tool
    {
        Collider? selectedCollider;
        const float highlightDistance = 4;
        int grabbedVertexIndex = -1;
        Vector2? newVertexTarget;
        public override void Awake() { }
        public override void OnDrawShapes()
        {
            if (selectedCollider == null)
                return;
            selectedCollider.DrawCollider();
            selectedCollider.DrawNormals();
            Polygon poly = selectedCollider.Polygon;
            for (int i = 0; i < poly.vertices.Length; i++)
            {
                Vector2 vert = poly.vertices[i];
                if (i == grabbedVertexIndex)
                    ShapeDrawer.DrawCircle(vert, highlightDistance, Pixel.Green);
                else
                    ShapeDrawer.DrawCircle(vert, highlightDistance / 2, Pixel.Green);
            }
            if (newVertexTarget != null)
                ShapeDrawer.DrawCircle((Vector2)newVertexTarget, highlightDistance / 2, Pixel.Green);
        }

        public override void Update(float delta)
        {
            // not editing
            // targeting (new vertex, existing, none)
            // grabbing vertex
            selectedCollider = GetSelectedCollider();
            if (selectedCollider == null)
                return;
            var localPoly = selectedCollider.untransformedPolygon;
            Vector2 mPosLocal = CMouse.GlobalPosition.Transformed(selectedCollider.Transform.Inverted());
            if (grabbedVertexIndex != -1 && Input.Get(Key.V))
            {
                localPoly.MoveVertex(grabbedVertexIndex, mPosLocal);
                return;
            }
            int closestVertIndex = 0;
            float closestSqrDistance = localPoly.vertices[0].SqrDistanceFrom(mPosLocal);
            for (int i = 1; i < localPoly.vertices.Length; i++)
                if (localPoly.vertices[i].SqrDistanceFrom(mPosLocal) < closestSqrDistance)
                {
                    closestSqrDistance = localPoly.vertices[i].SqrDistanceFrom(mPosLocal);
                    closestVertIndex = i;
                }
            if (closestSqrDistance < highlightDistance.Squared())
            {
                grabbedVertexIndex = closestVertIndex;
                if(Input.Get(Key.V))
                    localPoly.MoveVertex(grabbedVertexIndex, mPosLocal);
                return;
            }
            int vertcount = localPoly.vertices.Length;
            for (int i = 0; i < vertcount; i++)
            {
                Vector2 v1 = localPoly.vertices[i];
                Vector2 direction = localPoly.vertices[(i + 1) % vertcount] - v1;
                Vector2 dirNormalized = direction.Normalized();
                Vector2 mouseOffset = mPosLocal - v1;
                float dot = Vector2.Dot(dirNormalized, mouseOffset);
                if (dot > 0 && dot.Squared() < direction.SqrMagnitude() &&
                    mPosLocal.SqrDistanceFrom(direction * dot + v1) < highlightDistance.Squared())
                {
                    Vector2 newVertex = dirNormalized * dot + v1;
                    if (Input.Get(Key.V))
                    {
                        localPoly.InsertVertex(i + 1, newVertex);
                        grabbedVertexIndex = i + 1;
                        return;
                    }
                    newVertexTarget = newVertex;
                    return;
                }
            }
            grabbedVertexIndex = -1;
            newVertexTarget = null;
        }

        Collider? GetSelectedCollider()
        {
            if (Runtime.Current.GetStage() is null ||
                !Input.Get(Key.LeftCtrl) ||
                !Input.Get(Key.LeftShift))
            {
                grabbedVertexIndex = -1;
                return null;
            }
            return Editor.Current.LastSelected?.GetComponent<Collider>();
        }
    }
}
