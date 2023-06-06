using Pixel.Assets;
using Pixel.Types;
using Pixel.Types.Components;
using Pixel.Types.Physics;
using Pixel_Core.Types.Attributes;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;
using static Pixel.Input;
using static Pixel.Runtime;
using static Pixel.ShapeDrawer;

namespace Pixel
{
    public class GridNodeGenerator : Component
    {
        List<Node> nodes = new();
        private float radius = 0.0015f * 75f;
        private int length = 32;

        private Vector3 pos_depth = default;
        private Vector2 _pos = default;
        private Color _col = default;

        private Animation<Vector3> pos;
        private Animation<Color> colors;

        

        // you must dispose of any references to nodes and components here, simply set them as null.
        public override void Dispose()
        {
            nodes.DestroyAll();
            nodes.Clear();
            nodes = null;
        }
        public override void Awake()
        {
            const int max = 16;
            var pair = Sine.GetColorfulSineWaveAnim(length, radius, alpha : 250, frameTime: 4);
            pos = pair.pos;
            colors = pair.col;
            AssembleGrid(max);
        } 
        private void AssembleGrid(int max)
        {
            for (int i = 0; i < max; i++)
                for (int j = 0; j < max; j++)
                {
                    var position = i * j;
                    Vector2 origin = new(Position.X + i, Position.Y - j);
                    Node block = new($"block{position}", origin + new Vector2(i, -j) * 2f, Vector2.One);
                    nodes.Add(block);
                }
        }
        public override void Update()
        {
            pos_depth = pos.GetValue(true);
            _col = colors.GetValue(true);
        }
        public override void OnDrawShapes()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                Node? node = nodes[i];
                _pos = node.Position;
                _pos.X += pos_depth.X;
                _pos.Y += pos_depth.Y;
                DrawCircleFilled(_pos, radius, _col);
            }
        }
    }
}
