using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pixel_renderer
{
    public static class Collision
    {
        static ConcurrentBag<Node> colliderNodes = new();
        private static ConcurrentDictionary<Node, Node[]> CollisionQueue = new(); 
        private readonly static SpatialHash hash = new(Constants.ScreenHeight, Constants.ScreenWidth, Constants.CollisionCellSize);
        
        [Obsolete]
        public static void ViewportCollision(Node node)
        {
            Sprite sprite = node.GetComponent<Sprite>();
            Rigidbody rb = node.GetComponent<Rigidbody>();
            if (sprite is null || rb is null) return;
            if (sprite.isCollider)
            {
                if (node.position.y > Constants.ScreenWidth - 4 - sprite.size.y)
                {
                    node.position.y = Constants.ScreenWidth - 4 - sprite.size.y;
                }
                if (node.position.x > Constants.ScreenHeight - sprite.size.x)
                {
                    node.position.x = Constants.ScreenHeight - sprite.size.x; 
                    rb.velocity.x = 0;
                }
                if (node.position.x < 0)
                {
                    node.position.x = 0;
                    rb.velocity.x = 0;
                }
            }
        }

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
        
        public static void BroadPhase(Stage stage, List<List<Node>> collisionCells)
        {
            collisionCells.Clear();

            colliderNodes.Clear(); 
            
            // could look for a more graceful solution, but as of right now nodes that
            // don't have both a Sprite and a Rigidbody can't participate in collision events
            // check if node has rigidbody, then if has sprite, then if is collider, so proceed.

            Parallel.ForEach(stage.Nodes, _node =>
            {
                if (_node.TryGetComponent(out Rigidbody _))
                    if (_node.TryGetComponent(out Sprite sprite))
                        if (sprite.isCollider) colliderNodes.Add(_node);
                           
            });

            Parallel.ForEach(colliderNodes, node =>
            {
                List<Node> result = hash.GetNearby(node);
                collisionCells.Add(result);
            });
        }

        public static void NarrowPhase(List<List<Node>> collisionCells)
        {
            if (collisionCells.Count <= 0 || collisionCells[0] is null) return;
              

            Parallel.For(0, collisionCells.Count, i =>
            {
                if (i >= collisionCells.Count) return; 
                var cell = collisionCells[i];
                if (cell is null) return;
                if (cell.Count <= 0) return;

                Parallel.For(0, cell.Count, j =>
                {
                    var nodeA = cell[j];
                    if (nodeA is null) return;
                    var colliders = new List<Node>(32);
                    Parallel.For(0, cell.Count, k =>
                    {
                        var nodeB = cell[k];
                        if (nodeB is null) return;
                        // compare node's UUID since each node could contain several colliders, 
                        // todo : maybe implement intranodular collision
                        if (nodeA.UUID.Equals(nodeB.UUID)) return;
                            colliders.Add(nodeB);
                    }); 
                    RegisterCollisionEvent(nodeA, colliders.ToArray());
                }); 
            }); 
        }
       
        public static async Task RegisterColliders(Stage stage)
        {
            hash.ClearBuckets();

            while (hash.busy)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(0.01f));
            }

            Parallel.ForEach(stage.Nodes, node =>
            {
                if (!node.TryGetComponent(out Sprite sprite) || !sprite.isCollider)
                {
                    return;
                }
                hash.RegisterObject(node);
            });
        }


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
        
        public static void Execute()
        {
            Parallel.ForEach(CollisionQueue, collisionPair =>
            {
                Node A = collisionPair.Key;
                Parallel.ForEach(collisionPair.Value, B =>
                {
                    if (!A.TryGetComponent(out Rigidbody rbA) ||
                        !B.TryGetComponent(out Rigidbody rbB)) return;

                    Collide(ref rbA, ref rbB);
                    AttemptCallbacks(ref rbA, ref rbB);
                });
            }); 
            CollisionQueue.Clear(); 
        }
      
        private static void AttemptCallbacks(ref Rigidbody A, ref Rigidbody B)
        {
            if (A.IsTrigger || B.IsTrigger)
            {
                A.parentNode.OnTrigger(B);
                B.parentNode.OnTrigger(A);
                return;
            }
            A.parentNode.OnCollision(B);
            B.parentNode.OnCollision(A);
        }
        
        private static void Collide(ref Rigidbody A, ref Rigidbody B)
        {
            if (A.IsTrigger || B.IsTrigger) return;
            
            var velocityDifference = A.velocity.Distance(B.velocity) * 0.5f;
            
            Vec2 direction = (B.parentNode.position - A.parentNode.position).Normalize();
            // make sure not NaN after possibly dividing by zero in Normalize();
            direction = direction.sqrMagnitude is float.NaN ? Vec2.up : direction; 

            B.velocity += direction * velocityDifference;
            A.velocity += CMath.Negate(direction * velocityDifference);
            
        }
    }

}