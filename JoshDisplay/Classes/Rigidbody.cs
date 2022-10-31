using System;
using System.Windows.Input;
using Color = System.Drawing.Color;
namespace PixelRenderer.Components
{
    public class Rigidbody : Component
    {
        float mass = .02f;
        public float GetDrag()
        {
            var sprite = parentNode.sprite ?? new Sprite(Vec2.one * 2, JRandom.GetRandomColor(), true);
            var area = sprite.size.x * sprite.size.y;
            var velocity = this.velocity.Length;
            var drag = mass * velocity * area * 0.5f;
            return drag; 
        }
        public float drag = 0.0f; 
        public Vec2 velocity = new();
        public bool isGrounded = false;
        public string GetDebugs()
        {
            return $" \n VELOCITY__X = {velocity.x} \n VELOCITY__Y = {velocity.y} \n POSITION__X = {parentNode.position.x} \n POSITION__Y {parentNode.position.y} \n NODE : {parentNode.Name}";
        }
        public override void FixedUpdate()
        {
            drag = GetDrag();
            ApplyPhysics();
        }
        public void ApplyPhysics()
        {
            Sprite sprite = new();
            if (parentNode.sprite != null)
            {
                sprite = parentNode.sprite;
            }
            velocity.y += CMath.Gravity;
            velocity.y *= drag;
            velocity.x *= drag;
            

            parentNode.position.y += velocity.y;
            parentNode.position.x += velocity.x;

            if (sprite != null && sprite.isCollider)
            {
                if (parentNode.position.y > Constants.screenHeight - 4 - sprite.size.y)
                {
                    isGrounded = true;
                    parentNode.position.y = Constants.screenHeight - 4 - sprite.size.y;
                }
                else isGrounded = false;

                if (parentNode.position.x > Constants.screenWidth - sprite.size.x)
                {
                    parentNode.position.x = Constants.screenWidth - sprite.size.x;
                    velocity.x = 0;
                    
                }

                if (parentNode.position.x < 0)
                {
                    parentNode.position.x = 0;
                    velocity.x = 0;
                }
            }
        }
    }
}

