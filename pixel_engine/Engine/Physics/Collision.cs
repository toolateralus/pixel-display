﻿using pixel_renderer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;

namespace pixel_renderer
{
    public enum TriggerInteraction { Colliders, Triggers, None, All };
    public static partial class Collision
    {
        public static bool AllowEntries { get; private set; } = true;
       
        public static void SetActive(bool value) => AllowEntries = value;
        public static void ViewportCollision(Node node)
        {
            var hasCollider = node.TryGetComponent(out Collider col);
            if (!hasCollider) return;

            if (node.Position.Y > Constants.PhysicsArea.Y - col.Scale.Y)
                node.Position = node.Position.WithValue(y: Constants.PhysicsArea.Y - col.Scale.Y);
            if (node.Position.X > Constants.PhysicsArea.X - col.Scale.X)
                node.Position = node.Position.WithValue(x: Constants.PhysicsArea.X - col.Scale.X);
            if (node.Position.X < 0)
                node.Position = node.Position.WithValue(x: 0);
        }

        static Vector2 position = new Vector2(0, 0);
        static Vector2 size = new Vector2(Constants.PhysicsArea.X, Constants.PhysicsArea.Y);

        public static void FinalPhase()
        {
            if (Runtime.Current.GetStage() is not Stage stage)
                return;

            var quadTree = new QuadTree(new BoundingBox2D(-Constants.PhysicsArea.X, -Constants.PhysicsArea.Y, Constants.PhysicsArea.X, Constants.PhysicsArea.Y));

            foreach (var node in stage.nodes)
            {
                var hasCollider = node.HasComponent<Collider>();
                if (hasCollider)
                    quadTree.Insert(node);
            }
            BoundingBox2D range = new BoundingBox2D(position, size);
            // Create a list to store the nodes found within the range
            List<Node> foundNodes = new List<Node>();
            // Query the quadtree with the range
            quadTree.Query(range, foundNodes);

            for (int i = 1; i < foundNodes.Count; i++)
            {
                var A = foundNodes[i];
                for (int j = 0; j < i; ++j)
                {
                    var B = foundNodes[j];

                    if (A is null || B is null)
                        continue;
                    if (B == A)
                        continue;

                    bool a_has_rb = A.TryGetComponent<Rigidbody>(out var rbA);
                    bool b_has_rb = B.TryGetComponent<Rigidbody>(out var rbB);

                    if (a_has_rb && b_has_rb)
                        Collide(rbB, rbA);
                    if (a_has_rb && !b_has_rb)
                        Collide(B.GetComponent<Collider>(), rbA);
                    if (!a_has_rb && b_has_rb)
                        Collide(A.GetComponent<Collider>(), rbB);

                }
            }

        }
        private static void Collide(Collider A, Rigidbody B)
        {
            if (A is null || B is null)
                return;

            var bCol = B.GetComponent<Collider>();

            if (A.IsTrigger || bCol.IsTrigger) return;

            (Vector2 normal, float depth) = SATCollision.GetCollisionData(A.Polygon, bCol.Polygon);

            if (normal == Vector2.Zero)
                return;

            B.node.Position -= normal * depth;

            var dot = Vector2.Dot(B.velocity, normal);

            B.velocity -= normal * dot; 

            AttemptCallbacks(A, bCol);
        }
        private static void Collide(Rigidbody A, Rigidbody B)
        {
            if (A == null || B == null)
                return;

            Collider aCol = A.GetComponent<Collider>();
            Collider bCol = B.GetComponent<Collider>();
            if (aCol.IsTrigger || bCol.IsTrigger)
                return;

            (Vector2 normal, float depth) = SATCollision.GetCollisionData(aCol.Polygon, bCol.Polygon);
            if (normal == Vector2.Zero)
                return;

            float depenetration = depth / 2f;

            A.Position += normal * depenetration;
            B.Position -= normal * depenetration;

            ComputeImpulse(A, B, normal, depth);
            
            AttemptCallbacks(aCol, bCol);

        }
        static void ComputeImpulse(Rigidbody a, Rigidbody b, Vector2 normal, float depth)
        {
            Vector2 rv = b.velocity - a.velocity;

            float velAlongNormal = Vector2.Dot(rv, normal);

            if (velAlongNormal > 0) return;

            float minRestitution = MathF.Min(a.restitution, b.restitution);
            float force = -(1 + minRestitution) * velAlongNormal / (a.invMass + b.invMass);

            Vector2 impulse = force * normal;
            a.ApplyImpulse(-impulse);
            b.ApplyImpulse(impulse);

            PositionalCorrection(a, b, normal, depth);
        }

        static void PositionalCorrection(Rigidbody a, Rigidbody b, Vector2 normal, float depth)
        {
            const float k_slop = 0.01f;
            const float percent = 0.2f;

            float correction = MathF.Max(depth - k_slop, 0.0f) / (a.invMass + b.invMass) * percent;
            Vector2 correctionVector = normal * correction;

            a.Position -= a.invMass * correctionVector;
            b.Position += b.invMass * correctionVector;
        }

        private static void AttemptCallbacks(Collider A, Collider B)
        {
            if (A.IsTrigger || B.IsTrigger)
            {
                A.node.OnTrigger(B);
                B.node.OnTrigger(A);
                return;
            }
            A.node.OnCollision(B);
            B.node.OnCollision(A);
        }
        public static void Run()
        {
            FinalPhase();
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
        bool topQuadrant = (bounds.min.Y < midpoint.Y && bounds.max.Y < midpoint.Y);
        bool bottomQuadrant = (bounds.min.Y > midpoint.Y);

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
        {
            if (range.Intersects(node.GetComponent<Collider>().BoundingBox))
                found.Add(node);
        }

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