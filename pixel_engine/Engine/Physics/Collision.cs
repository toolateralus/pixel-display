using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace pixel_renderer
{
    public static class Collision
    {
        private static readonly ConcurrentDictionary<Node, Node[]> CollisionQueue = new();
        private static readonly ConcurrentBag<ConcurrentBag<Node>> collisionMap = new();
        public static bool HasTasks => CollisionQueue.Count > 0;
        public static bool AllowEntries { get; private set; } = true; 
        public static void SetActive(bool value) => AllowEntries = value;
        private readonly static SpatialHash hash = new(Settings.ScreenH, Settings.ScreenW, Settings.CollisionCellSize);
        public static void ViewportCollision(Node node)
        {
             var hasSprite = node.TryGetComponent( out Sprite sprite);
             var hasRb = node.TryGetComponent(out Rigidbody rb);
            
            if (!hasSprite || !hasRb || sprite.isCollider) return;

            if (node.position.y > Settings.ScreenW - 4 - sprite.size.y)
            {
                node.position.y = Settings.ScreenW - 4 - sprite.size.y;
            }
            if (node.position.x > Settings.ScreenH - sprite.size.x)
            {
                node.position.x = Settings.ScreenH - sprite.size.x;
                rb.velocity.x = 0;
            }
            if (node.position.x < 0)
            {
                node.position.x = 0;
                rb.velocity.x = 0;
            }
        }
        private static bool CheckOverlap(this Node nodeA, Node nodeB)
        {
            var A = nodeA.TryGetComponent(out Sprite spriteA); 
            var B = nodeB.TryGetComponent(out Sprite spriteB);
            return A && B
                && GetBoxCollision(nodeA, nodeB, spriteA, spriteB);
        }
        private static bool GetBoxCollision(Node nodeA, Node nodeB, Sprite spriteA, Sprite spriteB)
        {
            if (nodeA.position.x < nodeB.position.x + spriteB.size.x &&
                nodeA.position.y < nodeB.position.y + spriteB.size.y &&
                       spriteA.size.x + nodeA.position.x > nodeB.position.x &&
                       spriteA.size.y + nodeA.position.y > nodeB.position.y)
                return true;
            return false;
        }
        public static void BroadPhase(Stage stage, ConcurrentBag<ConcurrentBag<Node>> collisionCells)
        {
            collisionCells.Clear();
            
            if(stage.Nodes is null ||
                stage.Nodes.Count == 0) return;
                
            Parallel.ForEach(stage.Nodes, node =>
            {
                List<Node> result = hash.GetNearby(node);
                ConcurrentBag<Node> nodes = new();
                foreach (var _node in result) nodes.Add(_node);
                collisionCells.Add(nodes);
            });
        }
        public static void NarrowPhase(ConcurrentBag<ConcurrentBag<Node>> collisionCells)
        {
            Parallel.For(0, collisionCells.Count, i =>
            {
                if (i >= collisionCells.Count) return;
                var cellArray = collisionCells.ToArray();
                if (i >= cellArray.Length) return;
                var cell = cellArray[i].ToArray();
                if (cell is null) return;
                if (cell.Length <= 0) return;
                Parallel.For(0, cell.Length, j =>
                {
                    var nodeA = cell[j];
                    if (nodeA is null) return;
                    var colliders = new ConcurrentBag<Node>();
                    Parallel.For(0, cell.Length, k =>
                    {
                        var nodeB = cell[k];
                        if (nodeB is null) return;
                        if (nodeA.UUID.Equals(nodeB.UUID)) return;
                        if (!colliders.Contains(nodeB)) colliders.Add(nodeB);
                    });
                    RegisterCollisionEvent(nodeA, colliders.ToArray());
                });
            });
        }
        public static async Task RegisterCollidersAsync(Stage stage)
        {
            hash.ClearBuckets();

            while (hash.busy)
                await Task.Delay(10);

            await Task.Run(() => Parallel.ForEach(stage.Nodes, node =>
            {
                if (!node.TryGetComponent<Sprite>(out _)
                    || !node.TryGetComponent<Rigidbody>(out _)) return;
                ViewportCollision(node);
                hash.RegisterObject(node);
            }));
           
            
        }
        private static void RegisterCollisionEvent(Node A, Node[] colliders)
        {
            if (A is null) return;
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] is null) continue;
                if (A.CheckOverlap(colliders[i]))
                {
                    _ = CollisionQueue.GetOrAdd(key: A, value: colliders);
                }
            }
        }
        public static void FinalPhase()
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
        public async static Task Run()
        {
            if (!AllowEntries)
            {
                if(HasTasks) FinalPhase();
                return; 
            }
            var stage = Runtime.Instance.stage; 
            await Task.Run(() => RegisterCollidersAsync(stage));
            BroadPhase(stage, collisionMap);
            NarrowPhase(collisionMap);
            FinalPhase(); 
        }
        private static void Collide(ref Rigidbody A, ref Rigidbody B)
        {
            if (A.IsTrigger || B.IsTrigger) return;

            var velocityDifference = A.velocity.Distance(B.velocity) * 0.5f;
            if (velocityDifference < 0.1f)
                velocityDifference = 1f;

            Vec2 direction = (B.parentNode.position - A.parentNode.position).Normalize();

            // make sure not NaN after possibly dividing by zero in Normalize();
            direction = direction.SqrMagnitude() is float.NaN ? Vec2.zero : direction;

            var depenetrationForce = direction * velocityDifference * 0.5f;

            Vec2.Clamp(depenetrationForce, Vec2.zero, Vec2.one * Settings.MaxDepenetrationForce);

            B.velocity = Vec2.zero;
            A.velocity = Vec2.zero;

            B.parentNode.position += depenetrationForce;
            A.parentNode.position += CMath.Negate(depenetrationForce);

            depenetrationForce *= 0.5f;
            return; 
            // remove bounciness from collision resolution
            if (A.usingGravity && A.drag != 0) A.velocity += CMath.Negate(depenetrationForce);
            if (B.usingGravity && B.drag != 0) B.velocity += depenetrationForce;
        }
    }

}