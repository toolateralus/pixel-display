namespace pixel_renderer
{
    using System;
    using System.Collections.Generic;

    public class SpatialHash
    {
        int rows;
        int columns;
        int cellSize;
        List<List<Node>> Buckets = new List<List<Node>>();
        internal bool busy;

        public SpatialHash(int screenWidth, int screenHeight, int cellSize)
        {
            Buckets = new List<List<Node>>(); 
            rows = screenHeight / cellSize;
            columns = screenWidth / cellSize;
            this.cellSize = cellSize;
            for (int i = 0; i < columns * rows; i++)
            {
                Buckets.Add(new List<Node>());
            }
        }
        internal void ClearBuckets()
        {
            busy = true;
            for (int i = 0; i < columns * rows; i++)
            {
                Buckets[i].Clear();
            }
            busy = false; 
        }
        internal void RegisterObject(Node obj)
        {
            List<int> cells = Hash(obj);
            foreach (var index in cells)
            {
                if (index < 0 || index >= rows * columns) continue;
                Buckets[index].Add(obj);
            }
        }
        internal List<Node> GetNearby(Node node)
        {
            List<Node> nodes = new List<Node>();
            List<int> buckets = Hash(node);
            foreach (var index in buckets)
            {
                if (index < 0 || index >= rows * columns -1) continue;
                nodes.AddRange(Buckets[index]);
            }
            return nodes;
        }
        private void AddBucket(Vec2 vector, float width, List<int> bucket)
        {
            int cellPosition = (int)(
                       (Math.Floor(vector.x / cellSize)) +
                       (Math.Floor(vector.y / cellSize)) *
                       width
            );

            if (!bucket.Contains(cellPosition))
                bucket.Add(cellPosition);

        }
        private List<int> Hash(Node obj)
        {
            Sprite sprite = obj.GetComponent<Sprite>(); 
            List<int> bucketsObjIsIn = new List<int>();
            Vec2 min = new Vec2(
                obj.position.x,
                obj.position.y);
            Vec2 max = new Vec2(
                obj.position.x + sprite.size.x,
                obj.position.y + sprite.size.y);
            float width = Constants.screenWidth / cellSize;
            
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