using Pixel;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Numerics;

namespace Pixel_Engine.My_Scripts
{
    public class Chunk : List<List<Node>>
    {
        const int width = 16;
        const float fill = 0.25f;
        public Chunk(int capacity) : base(capacity)
        {
            // used to access base capacity constructor.
        }
        public void Initialize(Vector2 Position, int X, int Y, CellularAutoma automa)
        {
            InitializeLists();

            GetTrigger(Position, X, Y, automa);

            automa.perlin.Seed = Random.Shared.Next();

            GenerateBlocks(Position, automa);

            static void GetTrigger(Vector2 Position, int X, int Y, CellularAutoma automa)
            {
                Node trigger = new();
                Collider? collider = trigger.AddComponent<Collider>();
                collider.IsTrigger = true;

                Vector2 centerPosition = Position + new Vector2(width * automa.distributionMagnitude / 2f, width * automa.distributionMagnitude / 2f);
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
                for (int y = 0; y < width; y += 2)
                {
                    float v = automa.perlin.GetValue(x, y, x * y);

                    if (v < fill && v > -fill)
                    {
                        for (int z = 0; z < 2; ++z)
                            for (int n = 0; n < 2; ++n)
                            {
                                var y_index = y + z;
                                var x_index = x + n;

                                Vector2 newPosition = Position + (new Vector2(x_index, y_index) * automa.distributionMagnitude);
                               
                                if (automa.occupiedPositions.Contains(newPosition))
                                    continue;

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
        public static void DispenseCoins(CellularAutoma automa, int max)
        {
            for (int i = 0; i < automa.occupiedPositions.Count; ++i)
            {

                if (i > max)
                    return;

                Vector2 pos = automa.occupiedPositions.ElementAt(i);
                int numDirections = 8;
                float angleIncrement = 2 * (float)Math.PI / numDirections;

                for (int z = 0; z < numDirections; z++)
                {
                    float angle = i * angleIncrement;

                    float x = (float)Math.Cos(angle);
                    float y = (float)Math.Sin(angle);

                    Vector2 direction = new Vector2(x, y) * automa.distributionMagnitude;
                    Vector2 neighbor = pos + direction;

                    TryAddCoin(neighbor);
                    break;
                }

                

            }
                void TryAddCoin(Vector2 dir)
                {
                    if (automa.occupiedPositions.Contains(dir))
                        return;

                    Node coin = Coin.Standard();
                    coin.Position = dir;
                    automa.occupiedPositions.Add(dir);
                }

        }
    }
}
