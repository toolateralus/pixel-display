﻿using pixel_renderer;
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
        List<Vector2> highlightedVertices = new();
        Collider? selectedCollider;
        const float highlightDistance = 2;
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
                Vector2 vert = highlightedVertices[i];
                ShapeDrawer.DrawCircle(vert, vertSize, Pixel.Green);
            }
        }

        public override void Update(float delta)
        {
            highlightedVertices.Clear();
            selectedCollider = GetSelectedCollider();
            if (selectedCollider == null)
                return;
            Vector2[] vertices = selectedCollider.Polygon.vertices;
            if (vertices.Length == 0)
                return;
            Vector2 mPos = CMouse.GlobalPosition;
            Vector2 cPos = selectedCollider.node.Position;
            if (selectedVertIndex == -1)
            {
                int closestVert = 0;
                for(int i = 1; i < vertices.Length; i++)
                    if (vertices[i].SqrDistanceFrom(mPos) <
                        vertices[closestVert].SqrDistanceFrom(mPos))
                        closestVert = i;
                if (Input.Get(Key.Q) &&
                    vertices[closestVert].SqrDistanceFrom(mPos) < highlightDistance * highlightDistance)
                {
                    selectedVertIndex = closestVert;
                    vertices[selectedVertIndex] = mPos;
                    selectedCollider.Polygon = new Polygon(vertices).OffsetBy(cPos * -1);
                    highlightedVertices.Add(vertices[selectedVertIndex]);
                    vertSize = 4;
                }
                else
                {
                    vertSize = 2;
                    int vertcount = vertices.Length;
                    for (int i = 0; i < vertcount; i++)
                    {
                        Vector2 v1 = vertices[i];
                        Vector2 direction = vertices[(i + 1) % vertcount] - v1;
                        Vector2 dirNormalized = direction.Normalized();
                        Vector2 mouseOffset = mPos - v1;
                        float upDot = Vector2.Dot(dirNormalized, mouseOffset);
                        float leftDot = Vector2.Dot(dirNormalized.Normal_LHS(), mouseOffset);
                        if (upDot > 0 && upDot.Squared() < direction.SqrMagnitude()
                            && leftDot.Squared() < highlightDistance)
                        {
                            Vector2 newVertex = dirNormalized * upDot + v1;
                            highlightedVertices.Add(newVertex);
                            if (Input.Get(Key.V))
                            {
                                List<Vector2> newVertices = vertices.ToList();
                                newVertices.Insert(i + 1, newVertex);
                                selectedCollider.Polygon = new Polygon(newVertices.ToArray()).OffsetBy(cPos * -1);
                                selectedVertIndex = i + 1;
                                vertSize = 4;
                            }
                            break;

                        }
                    }
                    foreach (Vector2 vert in vertices)
                        highlightedVertices.Add(vert);
                }
            }
            else
            {
                if (!Input.Get(Key.V))
                {
                    vertSize = 2;
                    foreach (Vector2 vert in vertices)
                        highlightedVertices.Add(vert);
                    return;
                }
                vertices[selectedVertIndex] = mPos;
                selectedCollider.Polygon = new Polygon(vertices).OffsetBy(cPos * -1);
                highlightedVertices.Add(vertices[selectedVertIndex]);
                vertSize = 4;
            }
        }

        Collider? GetSelectedCollider()
        {
            if (Runtime.Current.GetStage() is null ||
                !Input.Get(Key.LeftCtrl) ||
                !Input.Get(Key.LeftShift))
            {
                selectedVertIndex = -1;
                return null;
            }
            return Editor.Current.LastSelected?.GetComponent<Collider>();
        }
    }
}
