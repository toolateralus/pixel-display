﻿namespace pixel_renderer
{
    using System;
    using System.Collections.Generic;

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
        internal void RegisterObject(Node obj)
        {
            List<int> cells = Hash(obj);
            foreach (var index in cells)
            {
                if (index < 0 || index >= rows * columns)
                    continue;
                Buckets[index].Add(obj);
            }
        }
        internal List<Node> GetNearby(Node node)
        {

            List<Node> nodes = new List<Node>(256);

            List<int> buckets = Hash(node);
            foreach (var index in buckets)
            {
                if (index < 0 || index >= rows * columns - 1) continue;

                if (Buckets[index].Count > nodes.Capacity)
                    nodes.Capacity = Buckets[index].Count;

                nodes.AddRange(Buckets[index]);
            }
            return nodes;
        }
        private void AddBucket(Vec2 vector, float width, List<int> bucket)
        {
            int cellPosition = (int)
                (Math.Floor((double)vector.x / cellSize) +
                 Math.Floor((double)vector.y / cellSize) * width);

            if (!bucket.Contains(cellPosition))
                bucket.Add(cellPosition);
        }
        private List<int> Hash(Node obj)
        {
            if (!obj.TryGetComponent(out Sprite sprite)) 
                return new();

            List<int> bucketsObjIsIn = new();

            Vec2 min = new Vec2(
                obj.Position.x,
                obj.Position.y);

            Vec2 max = new Vec2(
                obj.Position.x + sprite.size.x,
                obj.Position.y + sprite.size.y);

            float width = Constants.ScreenH / cellSize;

            //TopLeft
            AddBucket(min, width, bucketsObjIsIn);
            //TopRight
            AddBucket(new Vec2(max.x, min.y), width, bucketsObjIsIn);
            //BottomRight
            AddBucket(new Vec2(max.x, min.y), width, bucketsObjIsIn);
            //BottomLeft
            AddBucket(new Vec2(max.x, min.y), width, bucketsObjIsIn);

            return bucketsObjIsIn;

        }
    }
}