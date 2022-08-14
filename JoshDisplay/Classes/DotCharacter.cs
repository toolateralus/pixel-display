using System.Windows.Input;
namespace JoshDisplay
{
    public class DotCharacter
    {
        public Vec2 pos = new Vec2();
        public Vec2 vel = new Vec2();
        public bool isGrounded = false;
        bool jumpHeld = false;
        int framesSinceJump = 0;
        public void Update()
        {
            if (Keyboard.IsKeyDown(Key.W))
            {
                if (isGrounded && !jumpHeld) vel.y = -5;
                jumpHeld = true;
            }
            else if(isGrounded) jumpHeld = false;
            if (Keyboard.IsKeyDown(Key.S)) vel.y = 1;

            if (Keyboard.IsKeyDown(Key.A)) vel.x += -1;
            if (Keyboard.IsKeyDown(Key.D)) vel.x += 1;
        }
    }
    public class Vec2 { public float x; public float y; }
}
