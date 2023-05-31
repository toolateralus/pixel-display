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
        private float radius = 2;
        private int length = 32;

        private Vector3 pos_depth = default;
        private Vector2 _pos = default;
        private Color _col = default;

        private Animation<Vector3> pos;
        private Animation<Color> colors;

        public static Color[] Pallette = new Color[] {
                System.Drawing.Color.Black,
                System.Drawing.Color.Gray,
                System.Drawing.Color.MediumOrchid,
                System.Drawing.Color.MediumSeaGreen,
                System.Drawing.Color.MediumSlateBlue,
                System.Drawing.Color.MediumAquamarine,
                System.Drawing.Color.DeepPink,
                System.Drawing.Color.White,
        };

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
            var pair = Sine.GetColorfulSineWaveAnim(length, radius);
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
                DrawLine(_pos, _pos * radius, _col * pos_depth.Z);
            }
        }
    }
}
