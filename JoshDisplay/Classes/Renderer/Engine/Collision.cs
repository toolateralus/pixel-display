namespace pixel_renderer
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Documents;
    using System.Xml.Linq;

    public static class Collision
    {
        public readonly static SpatialHash hash = new(Constants.screenWidth, Constants.screenHeight, Constants.collisionCellSize);
        public static bool CheckOverlap(this Node nodeA, Node nodeB)
        {
            Vec2 a = nodeA.position;
            Vec2 b = nodeB.position;
            Vec2 spriteSizeA = nodeA.GetComponent<Sprite>().size;
            Vec2 spriteSizeB = nodeB.GetComponent<Sprite>().size;

            if (spriteSizeA != null && spriteSizeB != null)
            {
                // messy if for box collision; 
                if (a.x < b.x + spriteSizeB.y &&
                    a.x + spriteSizeA.x > b.x &&
                    a.y < b.y + spriteSizeB.y &&
                    spriteSizeA.y + a.y > b.y)
                    return true;
            }
            return false;

        }
        public static async Task BroadPhase(Stage stage, List<List<Node>> broadMap)
        {
            hash.ClearBuckets();
            broadMap.Clear();
            while (hash.busy)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(0.01f));
            }
            foreach (var node in stage)
            {
                if (!node.TryGetComponent(out Sprite sprite) || !sprite.isCollider)
                {
                    continue;
                }
                hash.RegisterObject(node);
            }
            foreach (var node in stage)
            {
                List<Node> result = hash.GetNearby(node);
                broadMap.Add(result);
            }
        }
        public static void NarrowPhase(List<List<Node>> collisionMap)
        {
            if (collisionMap.Count <= 0 || collisionMap[0] == null) return; 
            for(int i = 0; i < collisionMap.Count(); i++)
            {
                var cell = collisionMap[i];
                if(cell.Count <= 0) continue;

                for (int j = 0; j < cell.Count; j++)
                {
                    var nodeA = cell[j];
                    if (nodeA is null) continue;
                    var colliders = new List<Node>();
                    for(int k = 0; k < cell.Count; k++)
                    {
                        var nodeB = cell[k];
                        if (nodeB is null) continue;
                        if (nodeA.UUID.Equals(nodeB.UUID)) continue;
                        colliders.Add(nodeB);
                    }
                    RegisterCollisionEvent(nodeA, colliders.ToArray());
                }
            }
        } 
    
        static ConcurrentDictionary<Node, Node[]> CollisionQueue = new(); 
        public static void RegisterCollisionEvent(Node A, Node[] colliders)
        {
            if (A is null) return;
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] is null) continue;
                if (A.CheckOverlap(colliders[i]))
                {
                    _ = CollisionQueue.GetOrAdd(key : A, value : colliders);
                }

            }
        }
        public static void ViewportCollision(Node node)
        {
            Sprite sprite = node.GetComponent<Sprite>();
            Rigidbody rb = node.GetComponent<Rigidbody>();
            if (sprite is null || rb is null) return;
            if (sprite.isCollider)
            {
                if (node.position.y > Constants.screenHeight - 4 - sprite.size.y)
                {
                    node.position.y = Constants.screenHeight - 4 - sprite.size.y;
                }
                if (node.position.x > Constants.screenWidth - sprite.size.x)
                {
                    node.position.x = Constants.screenWidth - sprite.size.x; 
                    rb.velocity.x = 0;
                }
                if (node.position.x < 0)
                {
                    node.position.x = 0;
                    rb.velocity.x = 0;
                }
            }
        }
        internal static void GetCollision()
        {
            foreach (var collisionPair in CollisionQueue)
            {
                Node A = collisionPair.Key;
                Parallel.ForEach(collisionPair.Value, B =>
                {
                    GetCollisionComponents(A, B, out Rigidbody rbA, out Rigidbody rbB);
                    GetDominantBody(rbA, rbB, out Rigidbody submissive, out Rigidbody dominant);
                    Collide(submissive, dominant);
                }); 
            }
            CollisionQueue.Clear(); 
        }
        private static void Collide(Rigidbody submissive, Rigidbody dominant)
        {
            submissive.parentNode.position += dominant.velocity * 1.5f;
        }
        private static void GetDominantBody(Rigidbody rbA, Rigidbody rbB, out Rigidbody submissive, out Rigidbody dominant)
        {
            if (rbA.velocity.sqrMagnitude >= rbB.velocity.sqrMagnitude)
            {
                dominant = rbA;
                submissive = rbB;
                return; 
            }
            dominant = rbB;
            submissive = rbA;
        }
        /// <summary>
        /// Retrieves all relevant Node components to solve an already verified collision between two Nodes. 
        /// </summary>
        /// <param name="colliders"></param>
        /// <param name="rbA"></param>
        /// <param name="rbB"></param>
        /// <param name="submissive"></param>
        /// <param name="dominant"></param>
        private static void GetCollisionComponents(Node A, Node B, out Rigidbody rbA, out Rigidbody rbB)
        {
            rbA = A.GetComponent<Rigidbody>();
            rbB = B.GetComponent<Rigidbody>();
        }
           
    }

}