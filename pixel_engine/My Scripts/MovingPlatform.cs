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
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Windows.Documents;

namespace Pixel_Engine.My_Scripts
{
    public class CellularAutoma : Component
    {
        public SimplexPerlin perlin = new();
        public List<Chunk> Chunks = new();
        public HashSet<Vector2> occupiedPositions = new HashSet<Vector2>();
        public const int width = 16;
        public const int chunks = 2;
        public const int distributionMagnitude = 2;

        public override async void Awake()
        {
            perlin = new();
            Generate();
            while (true)
            {
                SetAllChunksActive(false);
                await Task.Delay(TimeSpan.FromSeconds(15));
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

            if (Chunks.Count > index)
                foreach (var list in Chunks[index])
                    foreach (var node in list)
                        node?.SetActive(value);
        }
        public override void Dispose()
        {
            foreach (var chnk in Chunks)
                foreach (var list in chnk)
                        list.DestroyAll();
        }
        internal static Node Standard()
        {
            Node output = new Node("world_root");
            output.AddComponent<CellularAutoma>();
            return output;
        }
    }
}