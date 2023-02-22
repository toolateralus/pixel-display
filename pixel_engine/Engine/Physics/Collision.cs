using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
namespace pixel_renderer
{
    public enum TriggerInteraction { Colliders, Triggers, None, All };
    public static partial class Collision
    {
        private static readonly Dictionary<Node, Node[]> CollisionQueue = new();
        private static readonly List<List<Node>> collisionMap = new();
        public static bool HasTasks => CollisionQueue.Count > 0;
        public static bool AllowEntries { get; private set; } = true;

        private readonly static SpatialHash Hash = new((int)Constants.PhysicsArea.x, (int)Constants.PhysicsArea.y, Constants.CollisionCellSize);
       
        public static void SetActive(bool value) => AllowEntries = value;
        public static void ViewportCollision(Node node)
        {
            var hasCollider = node.TryGetComponent(out Collider col);
            if (!hasCollider) return;

            if (node.Position.y > Constants.PhysicsArea.y - col.size.y)
                node.Position = node.Position.WithValue(y: Constants.PhysicsArea.y - col.size.y);
            if (node.Position.x > Constants.PhysicsArea.x - col.size.x)
                node.Position = node.Position.WithValue(x: Constants.PhysicsArea.x - col.size.x);
            if (node.Position.x < 0)
                node.Position = node.Position.WithValue(x: 0);
        }
        private static bool CheckOverlap(this Node nodeA, Node nodeB)
        {
            var colA = nodeA.TryGetComponent(out Collider col_A);
            var colB = nodeB.TryGetComponent(out Collider col_B);
            return colA && colB && GetBoxCollision(nodeA, nodeB, col_A, col_B);
        }
        private static bool GetBoxCollision(Node nodeA, Node nodeB, Collider col_A, Collider col_B)
        {
            if (nodeA.Position.x < nodeB.Position.x + col_B.size.x &&
                nodeA.Position.y < nodeB.Position.y + col_B.size.y &&
                       col_A.size.x + nodeA.Position.x > nodeB.Position.x &&
                       col_A.size.y + nodeA.Position.y > nodeB.Position.y)
                return true;
            return false;
        }
        public static void BroadPhase(Stage stage, List<List<Node>> collisionCells)
        {
            collisionCells.Clear();
            lock (stage.nodes)
                foreach (var node in stage.nodes)
                {
                    List<Node> nearby = Hash.GetNearby(node);
                    collisionCells.Add(nearby);
                }

        }
        public static void NarrowPhase(List<List<Node>> collisionCells)
        {
            lock(collisionCells)
            for (int i = 0; i < collisionCells.Count; i++)
            {
                if (collisionCells is null || i > collisionCells.Count - 1) continue;
                
                 var cellArray = collisionCells.ToArray();

                if (cellArray is null || i > cellArray.Length - 1)
                        return;

                if (cellArray[i] == null || cellArray[i].Count == 0) continue;


                var cell = cellArray[i].ToArray();
               

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

                        bool hittingItself = nodeA.UUID.Equals(nodeB.UUID) 
                                || nodeA.parentNode == nodeB 
                                || nodeB.parentNode == nodeA;

                        if (hittingItself)
                            continue;

                        if (!colliders.Contains(nodeB))
                            colliders.Add(nodeB);
                    }
                    RegisterCollisionEvent(nodeA, colliders);
                }
            }
        }
        public static void RegisterColliders(Stage stage)
        {
            Hash.ClearBuckets();
            Action<Node> RegisterAction = (node) =>
            {
                if (!node.TryGetComponent<Collider>(out _))
                    return;
                ViewportCollision(node);
                Hash.RegisterNode(node);
            };

            List<Node> nodes = new(stage.nodes);
                foreach (var node in nodes)
                    RegisterAction(node);

        }
        private static void RegisterCollisionEvent(Node A, List<Node> colliders)
        {
                if (A is null)
                    return;
                ConcurrentBag<Node> collidersCopy = new();
                
                lock (colliders)
                     collidersCopy = new(colliders.AsReadOnly());

                for (int i = 0; i < colliders.Count; i++)
                {
                    if (colliders[i] is null)
                        continue;
                    if (A.CheckOverlap(colliders[i]))
                    {
                        lock (CollisionQueue)
                        {
                            var nodesArray = colliders.ToArray();
                            if (CollisionQueue.ContainsKey(A))
                                CollisionQueue[A] = nodesArray;
                            else CollisionQueue.Add(A, nodesArray);
                        }
                    }
                }
        }
        public static void FinalPhase()
        {
            lock(CollisionQueue)
            for (int i = 0; i < CollisionQueue.Count; ++i)
            {
                Node A = CollisionQueue.Keys.ElementAt(i);
                if (!CollisionQueue.TryGetValue(A, out var nodes)) return;
                for (int j = 0; j < nodes.Length; ++j)
                {
                    Node B = nodes[j];
                    if (!A.TryGetComponent(out Collider col_A) ||
                        !B.TryGetComponent(out Collider col_B))
                        return;

                    bool a_has_rb = A.TryGetComponent<Rigidbody>(out var rbA);
                    bool b_has_rb = B.TryGetComponent<Rigidbody>(out var rbB);

                    if (a_has_rb && b_has_rb)
                        Collide(rbB, rbA);
                    if (a_has_rb && !b_has_rb)
                        Collide(rbA, col_B);
                    if (!a_has_rb && b_has_rb)
                        Collide(rbB, col_A);

                    AttemptCallbacks(col_A, col_B);
                }
                CollisionQueue.Clear();
            }
        }
        private static void Collide(Rigidbody A, Collider B)
        {
            if (A is null || B is null)
                return; 

            var aCol = A.GetComponent<Collider>();
            if (aCol.IsTrigger || B.IsTrigger) return;

            var minDepth = SATCollision.GetMinimumDepthVector(aCol.Polygon, B.Polygon);

            A.parent.Position += minDepth;
        }
        private static void Collide(Rigidbody A, Rigidbody B)
        {
            if (A is null || B is null)
                return; 

            var aCol = A.GetComponent<Collider>();
            var bCol = B.GetComponent<Collider>();
            if (aCol.IsTrigger || bCol.IsTrigger) return;

            //depenetrate
            var minDepth = SATCollision.GetMinimumDepthVector(aCol.Polygon, bCol.Polygon);
            if (minDepth == Vec2.zero)
                return;
            A.parent.Position += minDepth / 2;
            B.parent.Position -= minDepth / 2;

            //flatten velocities
            Vec2 colNormal = minDepth.Normalized();
            float colSpeedA = Vec2.Dot(A.velocity, colNormal);
            float colSpeedB = Vec2.Dot(B.velocity, colNormal);
            float averageSpeed = (colSpeedA + colSpeedB) / 2;
            A.velocity -= colNormal * (averageSpeed - colSpeedA);
            B.velocity -= colNormal * (averageSpeed - colSpeedB);
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
        public static void Run()
        {
            if (!AllowEntries)
            {
                if (HasTasks) FinalPhase();
                return;
            }

            var stage = Runtime.Current.GetStage();
            RegisterColliders(stage);
            BroadPhase(stage, collisionMap);
            NarrowPhase(collisionMap);
            FinalPhase();
        }
    }

}
