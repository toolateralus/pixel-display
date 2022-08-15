using System;
using System.Windows.Input;
using JoshDisplay.Classes; 
namespace JoshDisplay
{
    public class Rigidbody 
    {
        public Vec2 pos = new Vec2();
        public Vec2 velocity = new Vec2();
        public bool isGrounded = false;
        bool jumpHeld = false;
        public Node node;
        int ticks = 0; 
        public string GetDebugs()
        {
            return $" \n VELOCITY__X = {velocity.x} \n VELOCITY__Y = {velocity.y} \n POSITION__X = {pos.x} \n POSITION__Y {pos.y} \n RIGIDBODY__UPDATE__TICKS MINUS W__KEY__HELD__FRAMES {ticks} \n";
        }
        public void Update()
        {
            if (node == null) return;
            if(ticks<10000)ticks++; 
            if (Input.GetKeyDown(Key.W))
            {
                ticks--; 
                if (isGrounded && !jumpHeld) velocity.y = -5;
                jumpHeld = true;
            }
            else if (isGrounded) jumpHeld = false;

            if (Input.GetKeyDown(Key.S)) velocity.y = 1;
            if (Input.GetKeyDown(Key.A)) velocity.x += -1;
            if (Input.GetKeyDown(Key.D)) velocity.x += 1;
            node.position = this.pos;
            node.velocity = this.velocity; 
        }

      
    }
    public class Vec2 
    {
        public float x; public float y;  
        public Vec2(float x, float y)
        {
            this.x = x;
            this.y = y; 
        }
        public Vec2()
        {

        }
    }
}
