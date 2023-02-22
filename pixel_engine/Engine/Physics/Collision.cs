using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

            if (node.Position.y > Constants.PhysicsArea.y - col.size.y)
                node.Position = node.Position.WithValue(y: Constants.PhysicsArea.y - col.size.y);
            if (node.Position.x > Constants.PhysicsArea.x - col.size.x)
                node.Position = node.Position.WithValue(x: Constants.PhysicsArea.x - col.size.x);
            if (node.Position.x < 0)
                node.Position = node.Position.WithValue(x: 0);
        }
        public static void FinalPhase()
        {
            if (Runtime.Current.GetStage() is not Stage stage)
                return;
            foreach(Collider A in stage.GetAllComponents<Collider>())
            {
                foreach(Collider B in stage.GetAllComponents<Collider>())
                {
                    if (B == A)
                        continue;

                    bool a_has_rb = A.parent.TryGetComponent<Rigidbody>(out var rbA);
                    bool b_has_rb = B.parent.TryGetComponent<Rigidbody>(out var rbB);

                    if (a_has_rb && b_has_rb)
                        Collide(rbB, rbA);
                    if (a_has_rb && !b_has_rb)
                        Collide(rbA, B);
                    if (!a_has_rb && b_has_rb)
                        Collide(rbB, A);

                    AttemptCallbacks(A, B);
                }
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
            
            var stage = Runtime.Current.GetStage();
            FinalPhase();
        }
    }

}
