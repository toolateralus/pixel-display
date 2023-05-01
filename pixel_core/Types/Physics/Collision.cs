using Pixel;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using BoundingBox2D = Pixel.Types.Physics.BoundingBox2D;

namespace Pixel.Types.Physics
{
    /// <summary>
    /// Stores data representing a single collision between two colliders.
    /// </summary>
    public record Collision
    {
        public SATProjection thisProjection;
        public SATProjection otherProjection;
        public Collider collider = null;
        public Vector2 contact = default;
        public Vector2 normal = default;
        public float depth = default;
        public Collision() { }
        public Collision(Collider collider, Vector2 contact, Vector2 normal, float depth)
        {
            this.collider = collider;
            this.contact = contact;
            this.normal = normal;
        }
    }
    public class Physics
    {
        static QuadTree quadTree = null;
        /// <summary>
        /// Moves the physics simulation one step forward.
        /// </summary>
        public static void Step()
        {
            if (Interop.Stage is not Stage stage)
                return;


            var area = ProjectSettings.PhysicsArea;
            quadTree ??= new QuadTree(new BoundingBox2D(-area.X, -area.Y, area.X, area.Y));
            
            BoundingBox2D range = new(area / -2, area / 2);
            List<Node> foundNodes = new();

            for( int i = 0; i < stage.nodes.Count; ++i)
            {
                if (stage.nodes.Count <= i)
                    return;
                Node? node = stage.nodes[i];
                var hasCollider = node.col != null;
                if (hasCollider)
                    quadTree.Insert(node);
            };
            

            quadTree.Query(range, foundNodes);

            Parallel.For(0, foundNodes.Count, (i) => {
                var A = foundNodes[i];
                for (int j = 0; j < i; ++j)
                {
                    var B = foundNodes[j];

                    if (A is null || B is null)
                        continue;

                    if (B == A)
                        continue;

                    bool a_has_rb = A.rb != null;
                    bool b_has_rb = B.rb != null;

                    if (a_has_rb && b_has_rb)
                        Collide(B.rb, A.rb);

                    if (a_has_rb && !b_has_rb)
                        Collide(A.rb, B.col);

                    if (!a_has_rb && b_has_rb)
                        Collide(B.rb, A.col);

                }
            });
            quadTree.Clear();
        }
        private static void Collide(Rigidbody rigidBody, Collider staticCollider)
        {
            if (rigidBody is null || staticCollider is null)
                return;

            var rbCollider = rigidBody.node.col;

            if (rbCollider is null || rbCollider.UUID == staticCollider.UUID)
                return;

            Polygon polyA = rbCollider.Polygon;
            Polygon polyB = staticCollider.Polygon;

            Collision? collision = SATCollision.GetCollisionData(polyA, polyB, rigidBody.velocity);

            if (collision == null)
                return;

            if (!rbCollider.IsTrigger && !staticCollider.IsTrigger)
            {
                var dot = Vector2.Dot(rigidBody.velocity, collision.normal);

                rigidBody.Position += collision.normal * collision.depth;
                rigidBody.velocity -= collision.normal * dot;
            }
            GetCollisionObjects(staticCollider, rbCollider, collision.normal, collision.depth, out var collisionA, out var collisionB);
           
            if (collisionA.collider.IsTrigger || collisionB.collider.IsTrigger)
            {
                collisionA.collider.node.OnTriggered?.Invoke(collisionB);
                collisionB.collider.node.OnTriggered?.Invoke(collisionA);
                return;
            }
            collisionA.collider.node.OnCollided?.Invoke(collisionB);
            collisionB.collider.node.OnCollided?.Invoke(collisionA);
        }
        private static void Collide(Rigidbody A, Rigidbody B)
        {
            if (IsNull(A, B))
                return;

            Polygon polyA = A.node.col.Polygon;
            Polygon polyB = B.node.col.Polygon;

            Collision? collision = SATCollision.GetCollisionData(polyA, polyB, A.velocity - B.velocity);

            if (collision == null)
                return;


            if (IsNull(A, B))
                return;


            if (!(A.node.col.IsTrigger || B.node.col.IsTrigger))
            {
                Vector2 correction = collision.normal * collision.depth;
                A.Position += correction / 2;
                B.Position -= correction / 2;

                Vector2 aRelVelocity = A.velocity - B.velocity;

                float aRelInertia = Vector2.Dot(aRelVelocity, collision.normal) * (A.mass + B.mass);

                float minRestitution = MathF.Min(A.restitution, B.restitution);
                float totalEnergy = aRelInertia * minRestitution;

                Vector2 impulse = totalEnergy * collision.normal;
                A.ApplyImpulse(-impulse);
                B.ApplyImpulse(impulse);
            }

            if (IsNull(A, B))
                return;

            Collision collisionA, collisionB;
            GetCollisionObjects(A.node.col, B.node.col, collision.normal, collision.depth, out collisionA, out collisionB);

            if (IsNull(A, B))
                return;

            if (collisionA.collider.IsTrigger || collisionB.collider.IsTrigger)
            {
                collisionA.collider.node.OnTriggered?.Invoke(collisionB);
                collisionB.collider.node.OnTriggered?.Invoke(collisionA);
                return;
            }
            collisionA.collider.node.OnCollided?.Invoke(collisionB);
            collisionB.collider.node.OnCollided?.Invoke(collisionA);

            static bool IsNull(Rigidbody A, Rigidbody B)
            {
                return A == null || B == null || A.node is null || B.node is null || A.node.col is null || B.node.col is null;
            }
        }
        private static void GetCollisionObjects(Collider aCol, Collider bCol, Vector2 normal, float depth, out Collision collisionA, out Collision collisionB)
        {
            var centroidA = aCol.Polygon.centroid;
            var centroidB = bCol.Polygon.centroid;

            var aDistance = Vector2.Dot(centroidA, normal);
            var bDistance = Vector2.Dot(centroidB, normal);

            Vector2 aCollisionPoint = centroidA - normal * aDistance;
            Vector2 bCollisionPoint = centroidB - normal * bDistance;

            aCollisionPoint += normal * (depth / 2);
            bCollisionPoint -= normal * (depth / 2);

            aCollisionPoint.Transform(aCol.Transform);
            bCollisionPoint.Transform(bCol.Transform);

            collisionA = new(bCol, aCollisionPoint, normal, depth);
            collisionB = new(aCol, bCollisionPoint, -normal, depth);
        }
    }
}
public class QuadTree
{
    private readonly int capacity;
    private readonly BoundingBox2D boundary;
    private readonly List<Node> nodes;
    private readonly QuadTree[] children;

    public QuadTree(BoundingBox2D boundary, int capacity = 4)
    {
        this.capacity = capacity;
        this.boundary = boundary;
        nodes = new List<Node>();
        children = new QuadTree[4];
    }

    public QuadTree()
    {

    }

    public void Clear()
    {
        nodes.Clear();
        for (int i = 0; i < children.Length; i++)
        {
            children[i]?.Clear();
            children[i] = null;
        }
    }
    private void Split()
    {
        var subWidth = (int)boundary.Width / 2;
        var subHeight = (int)boundary.Height / 2;
        var x = (int)boundary.X;
        var y = (int)boundary.Y;

        children[0] = new QuadTree(new BoundingBox2D(x + subWidth, y, subWidth, subHeight), capacity);
        children[1] = new QuadTree(new BoundingBox2D(x, y, subWidth, subHeight), capacity);
        children[2] = new QuadTree(new BoundingBox2D(x, y + subHeight, subWidth, subHeight), capacity);
        children[3] = new QuadTree(new BoundingBox2D(x + subWidth, y + subHeight, subWidth, subHeight), capacity);
    }
    private int GetChildIndex(BoundingBox2D bounds)
    {
        int index = -1;
        Vector2 midpoint = (bounds.max + bounds.min) / 2;
        bool topQuadrant = bounds.min.Y < midpoint.Y && bounds.max.Y < midpoint.Y;
        bool bottomQuadrant = bounds.min.Y > midpoint.Y;

        if (bounds.min.X < midpoint.X && bounds.max.X < midpoint.X)
        {
            if (topQuadrant)
                index = 1;
            else if (bottomQuadrant)
                index = 2;
        }
        else if (bounds.min.X > midpoint.X)
        {
            if (topQuadrant)
                index = 0;
            else if (bottomQuadrant)
                index = 3;
        }
        return index;
    }
    public void Insert(Node node)
    {
        if (!boundary.Contains(node.Position))
            return;

        if (nodes.Count < capacity)
        {
            nodes.Add(node);
            return;
        }

        if (children[0] == null)
            Split();

        int index = GetChildIndex(node.GetComponent<Collider>().BoundingBox);
        if (index != -1)
            children[index].Insert(node);
        else
            nodes.Add(node);
    }
    public void Query(BoundingBox2D range, List<Node> found)
    {
        if (!boundary.Intersects(range))
            return;

        foreach (var node in nodes)
            if (node != null && node.col != null)
                if (range.Intersects(node.col.BoundingBox))
                    found.Add(node);

        if (children[0] == null)
            return;

        int index = GetChildIndex(range);
        if (index != -1)
            children[index].Query(range, found);
        else
        {
            for (int i = 0; i < children.Length; i++)
                children[i].Query(range, found);
        }
    }
}