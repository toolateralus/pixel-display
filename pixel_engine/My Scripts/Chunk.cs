using Pixel;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Pixel_Engine.My_Scripts
{
    public class Chunk : List<List<Node>>
    {
        const int width = 12;
        const float fill = 0.75f;

        public Chunk(int capacity) : base(capacity)
        {

        }
        public void Initialize(Vector2 Position, int X, int Y, CellularAutoma automa)
        {
            InitializeLists();

            GetTrigger(Position, X, Y, automa);

            automa.perlin.Seed = Random.Shared.Next();

            GenerateBlocks(Position, automa);

            automa.SetAllChunksActive(false);

            static void GetTrigger(Vector2 Position, int X, int Y, CellularAutoma automa)
            {
                Node trigger = new();
                Collider? collider = trigger.AddComponent<Collider>();
                collider.IsTrigger = true;

                Vector2 centerPosition = Position + new Vector2(width * CellularAutoma.distributionMagnitude / 2f, width * CellularAutoma.distributionMagnitude / 2f);
                collider.Position = centerPosition;
                collider.Scale = new Vector2(20, 20);

                trigger.OnTriggered += (col) =>
                {
                    if (col.collider.node.tag != "PLAYER")
                        return;

                    Runtime.Log($"PLAYER triggered chunk collider at x:{X} y:{Y}");
                    automa.SetChunkActive(X, Y, true);
                };
            }
        }

        private void InitializeLists()
        {
            for (int i = 0; i < width; i++)
            {
                var innerList = new List<Node>(width);
                for (int j = 0; j < width; j++)
                {
                    innerList.Add(new Node());
                }
                Add(innerList);
            }
        }

        private void GenerateBlocks(Vector2 Position, CellularAutoma automa)
        {
            for (int x = 0; x < width; x += 2)
            {
                for (int y = 0; y < width; y += 2)
                {
                    float v = automa.perlin.GetValue(x, y, x * y);
                    if (v < fill && v > -fill)
                    {
                        bool isVoid = true;
                        for (int z = 0; z < 2; ++z)
                        {
                            for (int n = 0; n < 2; ++n)
                            {
                                var y_index = y + z;
                                var x_index = x + n;

                                Vector2 newPosition = Position + (new Vector2(x_index, y_index) * CellularAutoma.distributionMagnitude);
                                if (automa.occupiedPositions.Contains(newPosition))
                                {
                                    isVoid = false;
                                    continue;
                                }
                                automa.occupiedPositions.Add(newPosition);

                                Node node = Rigidbody.StaticBody();
                                Sprite sprite = node.GetComponent<Sprite>();

                                if (sprite == null)
                                    continue;

                                sprite.Color = Gradient.Sample(position: (y_index * x_index) % 360, alpha: 255);

                                node.Position = newPosition;

                                Runtime.Current.GetStage()?.AddNode(node);

                                this[x_index][y_index] = node;
                            }
                        }
                    }
                }
            }
        }

        public static void DispenseCoins(CellularAutoma automa, int max)
        {
            int i = 0;
            foreach (var pos in automa.occupiedPositions)
            {
                if (i >= max)
                    return;

                bool isVoid = true;

                for (int x = -1; x <= 1; x += 2)
                {
                    for (int y = -1; y <= 1; y += 2)
                    {
                        Vector2 neighbor = pos + new Vector2(x, y);

                        if (automa.occupiedPositions.Contains(neighbor))
                        {
                            isVoid = false;
                            break;
                        }
                    }

                    if (!isVoid)
                        break;
                }

                if (isVoid)
                {
                    Node coin = Coin.Standard();
                    coin.Position = pos;
                    Runtime.Log("coin placed");
                    i++;
                }
            }
        }

    }
}