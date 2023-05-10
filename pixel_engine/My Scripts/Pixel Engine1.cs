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
    public class BlockGen : Component
    {
        List<Node> nodes = new();
        // you must dispose of any references to nodes and components here, simply set them as null.
        public override void Dispose()
        {
            nodes.Clear();
            nodes = null;
        }
        public override async void Awake()
        {
            const int max = 12;
            Task task = new(async () => { 
                 for(int i = 0; i < max; i++)
                    for (int j = 0; j < max; j++)
                    {
                        var position = i * j;
                        Node block = new($"block{position}", new Vector2(i, j) * 2f, Vector2.One);
                        var sprite = block.AddComponent<Sprite>();

                        sprite.Type = ImageType.SolidColor;
                        var pallette = new Color[] {
                            System.Drawing.Color.MediumSeaGreen,
                            System.Drawing.Color.MediumSlateBlue, 
                            System.Drawing.Color.MediumAquamarine, 
                            System.Drawing.Color.DeepPink, 
                        };
                        var color = Gradient.Sample(position, max * max, 255, pallette);
                            sprite.SetImage(Vector2.One * 16, CBit.ByteFromPixel(CBit.SolidColorSquare(Vector2.One * 16, color)));
                        nodes.Add(block);
                        await Task.Delay(25);
                    }
            });
            task.Start();
        }
        public override void FixedUpdate(float delta)
        {

        }
        public override void OnDrawShapes()
        {
        }
    }
}
