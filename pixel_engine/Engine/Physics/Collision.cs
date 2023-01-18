using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using static pixel_renderer.Collision;

namespace pixel_renderer
{
    public enum TriggerInteraction { Colliders, Triggers, None, All };
    public static partial class Collision
    {
        private static readonly Dictionary<Node, Node[]> CollisionQueue = new();
        private static readonly ConcurrentBag<ConcurrentBag<Node>> collisionMap = new();
        public static bool HasTasks => CollisionQueue.Count > 0;
        public static bool AllowEntries { get; private set; } = true;

        public static void SetActive(bool value) => AllowEntries = value;
        private readonly static SpatialHash hash = new(Constants.ScreenW, Constants.ScreenH, Constants.CollisionCellSize);
        public static void ViewportCollision(Node node)
        {
            var hasCollider = node.TryGetComponent(out Collider col);
            if (!hasCollider) return;

            if (node.position.y > Constants.ScreenW - 4 - col.size.y)
                 node.position.y = Constants.ScreenW - 4 - col.size.y;
            if (node.position.x > Constants.ScreenH - col.size.x)
                 node.position.x = Constants.ScreenH - col.size.x;
            if (node.position.x < 0)
                 node.position.x = 0;
        }
        private static bool CheckOverlap(this Node nodeA, Node nodeB)
        {
            var colA = nodeA.TryGetComponent(out Collider col_A);
            var colB = nodeB.TryGetComponent(out Collider col_B);
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

            if (stage.Nodes is null ||
                stage.Nodes.Count == 0) return;

            List<Node> nodes;
            
            lock (stage.Nodes)
                nodes = new List<Node>(stage.Nodes);

            if (nodes is null) return;
            foreach (var stage_node in nodes)
            {
                List<Node> nearbyNodes = hash.GetNearby(stage_node);
                ConcurrentBag<Node> colliders = new();

                foreach(var node in nodes)
                    colliders.Add(node);
                collisionCells.Add(colliders);
            }
        }
        public static void NarrowPhase(ConcurrentBag<ConcurrentBag<Node>> collisionCells)
        {
            for (int i = 0; i < collisionCells.Count; i++)
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
            }
        }
        public static void RegisterColliders(Stage stage)
        {
            hash.ClearBuckets();
            Action<Node> RegisterAction = (node) =>
            {
                if (!node.TryGetComponent<Collider>(out _))
                    return;
                ViewportCollision(node);
                hash.RegisterObject(node);
            };

            List<Node> nodes = new(stage.Nodes);
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

                    bool has_rb = A.TryGetComponent<Rigidbody>(out var rbA);
                    bool has_rb_b = B.TryGetComponent<Rigidbody>(out var rbB);

                    if (has_rb && has_rb_b)
                        RigidbodyCollide(rbA, rbB);


                    else if (has_rb || has_rb_b)
                        if (has_rb) Collide(rbA, col_B);
                        else Collide(rbB, col_A);
                    AttemptCallbacks(col_A, col_B);
                }
                CollisionQueue.Clear();
            }
        }
        private static void Collide(Rigidbody A, Collider B)
        {
            var aCol = A.GetComponent<Collider>();
            if (aCol.IsTrigger || B.IsTrigger) return;

            var a_vertices = aCol.GetVertices(); 
            var b_vertices = B.GetVertices();

            var a_normals = aCol.normals;
            var b_normals = B.normals;

            var a_centroid = aCol.GetCentroid();
            var b_centroid = B.GetCentroid();
            Polygon pA = new()
            {
                vertices = a_vertices,
                centroid = a_centroid,
                normals = a_normals,
            };
            Polygon pB = new()
            {
                vertices = b_vertices,
                centroid = b_centroid,
                normals = b_normals,
            };
            var minDepth = SATCollision.GetMinimumDepthVector(pA, pB);
            A.parent.position += minDepth;
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
            var stage = Runtime.Instance.GetStage();

            RegisterColliders(stage);
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
