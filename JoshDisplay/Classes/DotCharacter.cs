using System.Windows.Input;
using JoshDisplay.Classes; 
namespace JoshDisplay
{
    public class DotCharacter
    {
        public Vec2 pos = new Vec2();
        public Vec2 vel = new Vec2();
        public bool isGrounded = false;
        bool jumpHeld = false;

        public void Update()
        {
            if (Input.GetKeyDown(Key.W))
            {
                if (isGrounded && !jumpHeld) vel.y = -5;
                jumpHeld = true;
            }
            else if (isGrounded) jumpHeld = false;

            if (Input.GetKeyDown(Key.S)) vel.y = 1;
            if (Input.GetKeyDown(Key.A)) vel.x += -1;
            if (Input.GetKeyDown(Key.D)) vel.x += 1;
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
