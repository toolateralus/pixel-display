using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace pixel_renderer
{
    public enum TriggerInteraction { Colliders, Triggers, None, All }; 
    public static class Collision
    {
        private static readonly ConcurrentDictionary<Node, Node[]> CollisionQueue = new();
        private static readonly ConcurrentBag<ConcurrentBag<Node>> collisionMap = new();
        public static bool HasTasks => CollisionQueue.Count > 0;
        public static bool AllowEntries { get; private set; } = true;

        public static void SetActive(bool value) => AllowEntries = value;
        private readonly static SpatialHash hash = new(Constants.ScreenH, Constants.ScreenW, Constants.CollisionCellSize);
        public static void ViewportCollision(Node node)
        {
            var hasCollider = node.TryGetComponent(out Collider col);
            if (!hasCollider) return;

            if (node.position.y > Constants.ScreenW - 4 - col.size.y)
            {
                node.position.y = Constants.ScreenW - 4 - col.size.y;
            }
            if (node.position.x > Constants.ScreenH - col.size.x)
            {
                node.position.x = Constants.ScreenH - col.size.x;
            }
            if (node.position.x < 0)
            {
                node.position.x = 0;
            }
        }
        private static bool CheckOverlap(this Node nodeA, Node nodeB)
        {
            var colA = nodeA.TryGetComponent<Collider>(out Collider col_A);
            var colB = nodeB.TryGetComponent<Collider>(out Collider col_B);
           return colA && colB && GetBoxCollision(nodeA, nodeB, col_A, col_B);
        }
        private static bool GetBoxCollision(Node nodeA, Node nodeB, Collider col_A, Collider col_B)
        {
            if (nodeA.position.x < nodeB.position.x + col_B.size.x &&
                nodeA.position.y < nodeB.position.y + col_B.size.y &&
                       col_A.size.x + nodeA.position.x > nodeB.position.x &&
                       col_A.size.y + nodeA.position.y > nodeB.position.y)
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
                foreach (var _node in result)
                    nodes.Add(_node);
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

                for (int j = 0; j < cell.Length; j++)
                {
                    var nodeA = cell[j];
                    if (nodeA is null) continue;

                    var colliders = new List<Node>(); 

                    for (int k = 0; k < cell.Length; ++k)
                    {
                        var nodeB = cell[k];

                        if (nodeB is null)
                            continue;

                        bool hittingItself = nodeA.UUID.Equals(nodeB.UUID);

                        if (hittingItself)
                            continue;

                        if (!colliders.Contains(nodeB))
                            colliders.Add(nodeB);
                    }
                    RegisterCollisionEvent(nodeA, colliders);
                }
            });
        }
        public static async Task RegisterCollidersAsync(Stage stage)
        {
            hash.ClearBuckets();

            while (hash.busy)
                await Task.Delay(10);

            Action<Node> RegisterAction = (node) =>
            {
                if(!node.TryGetComponent<Collider>(out _)) 
                    return;
                ViewportCollision(node);
                hash.RegisterObject(node);
            };
            _ = Parallel.ForEach(stage.Nodes, RegisterAction);
            
        }
        private static void RegisterCollisionEvent(Node A, List<Node> colliders)
        {
            if (A is null)
                return;

            for (int i = 0; i < colliders.Count; i++)
            {
                if (colliders[i] is null)
                    continue;
                if (A.CheckOverlap(colliders[i]))
                    _ = CollisionQueue.GetOrAdd(key: A, value: colliders.ToArray());
            }
        }
        public static void FinalPhase()
        {
            Action<KeyValuePair<Node, Node[]>> forBody = collisionPair =>
                        {
                            Node A = collisionPair.Key;
                            _ = Parallel.ForEach(collisionPair.Value, B =>
                            {
                                if (!A.TryGetComponent(out Collider col_A) ||
                                    !B.TryGetComponent(out Collider col_B))
                                    return;

                                if (A.TryGetComponent<Rigidbody>(out var rbA)
                                && B.TryGetComponent<Rigidbody>(out var rbB))
                                {
                                    RigidbodyCollide(rbA, rbB);
                                }
                                else
                                    Collide(col_A, col_B);

                                AttemptCallbacks(col_A, col_B);
                            });
                        };
            _ = Parallel.ForEach(CollisionQueue, forBody);
            CollisionQueue.Clear();
        }
        private static void Collide(Collider A, Collider B)
        {
            if (A.IsTrigger || B.IsTrigger) return;

            Vec2 direction = (B.parent.position - A.parent.position).Normalize();

            B.parent.position += direction;
            A.parent.position += CMath.Negate(direction);

        }

        private static void AttemptCallbacks(Collider A, Collider B)
        {
            if (A.IsTrigger || B.IsTrigger)
            {
                A.parent.OnTrigger(B);
                B.parent.OnTrigger(A);
                return;
            }
            A.parent.OnCollision(B);
            B.parent.OnCollision(A);
        }
       
        public async static Task Run()
        {
            if (!AllowEntries)
            {
                if(HasTasks) FinalPhase();
                return; 
            }
            var stage = Runtime.Instance.GetStage();
            
            await RegisterCollidersAsync(stage);
            
            BroadPhase(stage, collisionMap);
            NarrowPhase(collisionMap);
            FinalPhase(); 
        }

        private static void RigidbodyCollide(Rigidbody A, Rigidbody B)
        {
            // check collider for trigger
            ///if (A.IsTrigger || B.IsTrigger) return;

            var velocityDifference = A.velocity.Distance(B.velocity) * 0.5f;
            if (velocityDifference < 0.1f)
                velocityDifference = 1f;

            Vec2 direction = (B.parent.position - A.parent.position).Normalize();

            var depenetrationForce = direction * velocityDifference * 0.5f;

            Vec2.Clamp(ref depenetrationForce, Vec2.zero, Vec2.one * Constants.MaxDepenetrationForce);

            B.velocity = Vec2.zero;
            A.velocity = Vec2.zero;

            B.parent.position += depenetrationForce;
            A.parent.position += CMath.Negate(depenetrationForce);

            depenetrationForce *= 0.5f;

        }
    }

}