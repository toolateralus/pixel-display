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
using System.Reflection.Metadata.Ecma335;
using System.Windows;
using System.Collections;

namespace Pixel_Engine.My_Scripts
{
    public class CellularAutoma : Component
    {
        public SimplexPerlin perlin = new();
        public List<Chunk> Chunks = new();
        public HashSet<Vector2> occupiedPositions = new HashSet<Vector2>();

        public const int width = 12;
        public const int chunks = 6;
        public const int distributionMagnitude = 2;

        public override async void Awake()
        {
            perlin = new();

            Generate();
            SetAllChunksActive(false);

            while (true)
            {
                SetAllChunksActive(false);
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
        private void Generate()
        {
            Vector2 initialPosition = Position;  

            for (int X = 0; X < chunks; ++X)
            {
                for (int Y = 0; Y < chunks; ++Y)
                {
                    Position = initialPosition + new Vector2(X * 10 * distributionMagnitude, Y * 10 * distributionMagnitude);
                    Chunk chunk = new(width);
                    chunk.Initialize(Position, X, Y, this);
                    Chunks.Add(chunk);
                }
            }
        }
        public void SetAllChunksActive(bool value)
        {
            foreach (var chnk in Chunks)
                foreach (var list in chnk)
                    foreach (var node in list)
                        node?.SetActive(value);
        }

        public void SetChunkActive(int x, int y, bool value)
        {
            var index = y * width + x; 
            if(Chunks.Count > index)
                foreach (var list in Chunks[index])
                    foreach (var node in list)
                        node?.SetActive(value);
        }
        public override void Dispose()
        {
            
        }
        internal static Node Standard()
        {
            Node output = new Node("world_root");
            output.AddComponent<CellularAutoma>();
            return output;
        }
    }
    public class Chunk : List<List<Node>>
    {
        const int width = 12;
        const float fill = 0.75f;

        public Chunk(int capacity) : base(capacity)
        {

        }
        public void Initialize(Vector2 Position, int X, int Y, CellularAutoma automa)
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


            Node trigger = new();
            Collider? collider = trigger.AddComponent<Collider>();
            collider.IsTrigger = true;
            collider.Position = Position;
            collider.Scale = new Vector2(20, 20);
            trigger.OnTriggered += (col) =>
            {
                if (col.collider.node.tag != "PLAYER")
                    return;

                Runtime.Log("hit chunk collider");
                automa.SetChunkActive(X, Y, true);
            };

            automa.perlin.Seed = Random.Shared.Next();

            for (int x = 0; x < width; x += 2)
            {
                for (int y = 0; y < width; y += 2)
                {
                    float v = automa.perlin.GetValue(x, y, x * y);
                    if (v < fill && v > -fill)
                    {
                        for (int z = 0; z < 2; ++z)
                        {
                            for (int n = 0; n < 2; ++n)
                            {
                                var y_index = y + z;
                                var x_index = x + n;

                                Vector2 newPosition = Position + (new Vector2(x_index, y_index) * CellularAutoma.distributionMagnitude);
                                if (automa.occupiedPositions.Contains(newPosition))
                                    continue;
                                automa.occupiedPositions.Add(newPosition);

                                Node node = Rigidbody.StaticBody();
                                Sprite sprite = node.GetComponent<Sprite>();

                                if (sprite == null)
                                    continue;

                                sprite.Color = Gradient.Sample(position: (y_index * x_index) % 360, alpha: 255);

                                node.Position = newPosition;

                                this[x_index][y_index] = node;
                            }
                        }
                    }
                }
            }
        }
    }
         
}