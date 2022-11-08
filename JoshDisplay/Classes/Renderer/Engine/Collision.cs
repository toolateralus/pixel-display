namespace pixel_renderer
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows.Documents;
    using System.Xml.Linq;

    public static class Collision
    {
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
                if (i > collisionMap.Count) break; 
                var cell = collisionMap[i];
                if (cell is null) continue; 
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
        [STAThread]
        public static void GetCollision()
        {
            foreach (var collisionPair in CollisionQueue)
            {
                Node A = collisionPair.Key;
                Parallel.ForEach(collisionPair.Value, B =>
                {
                    if (!A.TryGetComponent(out Rigidbody rbA) ||
                        !B.TryGetComponent(out Rigidbody rbB)) return;
                    GetDominantBody(rbA, rbB, out Rigidbody submissive, out Rigidbody dominant);
                    Collide(ref submissive, ref dominant);
                    AttemptCallbacks(ref dominant, ref submissive);
                }); 
            }
            CollisionQueue.Clear(); 
        }
        private readonly static SpatialHash hash = new(Constants.screenWidth, Constants.screenHeight, Constants.collisionCellSize);
        private static bool CheckOverlap(this Node nodeA, Node nodeB)
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
        private static ConcurrentDictionary<Node, Node[]> CollisionQueue = new(); 
        private static void RegisterCollisionEvent(Node A, Node[] colliders)
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
        private static void AttemptCallbacks(ref Rigidbody A, ref Rigidbody B)
        {
            if (A.IsTrigger)
            {
                B.OnTrigger(A);
                A.OnTrigger(B);
                return;
            }
            if(!B.IsTrigger)
            {
                A.OnCollision(B);
                B.OnCollision(A);
            }

        }
        private static void Collide(ref Rigidbody submissive, ref Rigidbody dominant)
        {
            if (submissive.IsTrigger || dominant.IsTrigger) return;
            



        }
        private static void GetDominantBody(Rigidbody rbA, Rigidbody rbB, out Rigidbody submissive, out Rigidbody dominant)
        {
            if (rbA.velocity.sqrMagnitude >= rbB.velocity.sqrMagnitude)
            {
                if (!rbB.usingGravity)
                {
                    dominant = rbB;
                    submissive = rbA;
                    return;
                }
                dominant = rbA;
                submissive = rbB;
                return; 
            }
            if (!rbA.usingGravity)
            {
                dominant = rbA;
                submissive = rbB;
                return;
            }
            dominant = rbB;
            submissive = rbA;
        }
    }

}