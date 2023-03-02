namespace pixel_renderer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;

    public class SpatialHash
    {
        int rows;
        int columns;
        int cellSize;
        List<List<Node>> Buckets = new List<List<Node>>();
        /// <summary>
        /// emits a busy signal while invalid operations are very likely
        /// </summary>
        internal bool busy;

        public SpatialHash(int screenWidth, int screenHeight, int cellSize)
        {
            busy = true; 
            
            Buckets = new List<List<Node>>();

            rows = screenHeight / cellSize;
            columns = screenWidth / cellSize;

            this.cellSize = cellSize;

            for (int i = 0; i < columns * rows; i++)
                Buckets.Add(new List<Node>());
            busy = false; 
        }
        internal void ClearBuckets()
        {
            busy = true;
            for (int i = 0; i < columns * rows; i++)
                Buckets[i].Clear();
            busy = false;
        }
        internal void RegisterNode(Node node)
        {
            List<int> cells = Hash(node);
            foreach (var index in cells)
            {
                if (index < 0 || index >= rows * columns)
                    continue;
                Buckets[index].Add(node);
            }
        }
        internal List<Node> GetNearby(Node node)
        {
            List<Node> nodes = new(256);
            List<int> buckets = Hash(node);

            foreach (var index in buckets)
            {
                if (index < 0 || index >= rows * columns - 1)
                    continue;

                if (Buckets[index].Count > nodes.Capacity)
                    nodes.Capacity = Buckets[index].Count;

                nodes.AddRange(Buckets[index]);
            }
            return nodes;
        }
        private void AddBucket(Vector2 vector, float width, List<int> bucket)
        {
            int cellPosition = (int)
                (Math.Floor((double)vector.X / cellSize) +
                 Math.Floor((double)vector.Y / cellSize) * width);

            if (!bucket.Contains(cellPosition))
                bucket.Add(cellPosition);
        }
        private List<int> Hash(Node obj)
        {
            if (!obj.TryGetComponent(out Sprite sprite)) 
                return new();

            List<int> bucketsObjIsIn = new();

            Vector2 min = new Vector2(
                obj.Position.X,
                obj.Position.Y);

            Vector2 max = new Vector2(
                obj.Position.X + sprite.size.X,
                obj.Position.Y + sprite.size.Y);

            float width = Constants.ScreenH / cellSize;

            //TopLeft
            AddBucket(min, width, bucketsObjIsIn);
            //TopRight
            AddBucket(new Vector2(max.X ,min.Y), width, bucketsObjIsIn);
            //BottomRight
            AddBucket(new Vector2(max.X ,min.Y), width, bucketsObjIsIn);
            //BottomLeft
            AddBucket(new Vector2(max.X ,min.Y), width, bucketsObjIsIn);

            return bucketsObjIsIn;

        }
    }
}