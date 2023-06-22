using Pixel;
using Pixel.Types.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using Animation = Pixel.Animation<System.Numerics.Vector2>;
using LibNoise;
using LibNoise.Primitive;
using Newtonsoft.Json.Linq;
using System.Data;

namespace Pixel_Engine.My_Scripts
{
    public class CellularAutoma : Component
    {
        SimplexPerlin perlin = new();

        Node[,][,] Metagrid = new Node[chunks,chunks][,];
        List<Node> triggers = new();
        const int width = 12;
        const int chunks = 5;
        private float distributionMagnitude =  2;

        public override async void Awake()
        {
            perlin = new();
            Generate();

            SetAllChunksActive(false);
            awake = true;

            while (true)
            {
                SetAllChunksActive(false);
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }

        private void Generate()
        {
            Vector2 initialPosition = Position;  // Remember the initial position

            for (int X = 0; X < chunks; ++X)
            {
                for (int Y = 0; Y < chunks; ++Y)
                {
                    // The position of each chunk is based on its own X and Y values
                    Position = initialPosition + new Vector2(X * 10 * distributionMagnitude, Y * 10 * distributionMagnitude);
                    Metagrid[X, Y] = new Node[width, width];

                    Node trigger = new();
                    trigger.AddComponent<Collider>();
                    trigger.Position = Position;
                    trigger.Scale = new Vector2(20, 20);
                    trigger.GetComponent<Collider>().IsTrigger = true;
                    trigger.OnTriggered += (col) =>
                    {
                        if (col.collider.node.tag != "PLAYER")
                            return;

                        SetChunkActive(X, Y, true);
                    };
                    triggers.Add(trigger);

                    // Randomize cave output
                    perlin.Seed = Random.Shared.Next();

                    for (int x = 0; x < width; ++x)
                        for (int y = 0; y < width; ++y)
                        {
                            float v = perlin.GetValue(x, y, x * y);

                            if (v > 0)
                            {
                                Node node = Rigidbody.StaticBody();
                                Sprite sprite = node.GetComponent<Sprite>();

                                if (sprite == null)
                                    continue;

                                sprite.Color = Gradient.Sample(x * y, 100 * 100, Math.Clamp((byte)(x * y), (byte)0, (byte)255));

                                // Node positions are relative to the chunk's position
                                node.Position = Position + (new Vector2(x, y) * distributionMagnitude);
                                Metagrid[X, Y][x, y] = node;
                            }
                        }
                }
            }
        }



        private void SetAllChunksActive(bool value)
        {
            for (int i = 0; i < Metagrid.GetLength(0); ++i)
                for (int j = 0; j < Metagrid.GetLength(1); ++j)
                    SetChunkActive(i, j, value);
        }

        private void SetChunkActive(int x, int y, bool value)
        {
            if (Metagrid.GetLength(0) > x && Metagrid.GetLength(1) > y)
                foreach (var node in Metagrid[x, y])
                    if(node != null)
                        node.Enabled = value;
        }

        public override void Dispose()
        {
        }
    }
}
