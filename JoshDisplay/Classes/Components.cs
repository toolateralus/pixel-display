using System;
using System.Windows.Input;
using System.Windows.Media;
namespace PixelRenderer.Components
{
    
    public class Component
    {
        public Node parentNode;

        
    }

    public class Rigidbody : Component
    {
        public Vec2 pos = new Vec2();
        public Vec2 velocity = new Vec2();
        public bool isGrounded = false; 
        public bool takingInput { get;  set; } = false; 
        bool jumpHeld = false;

        public string GetDebugs()
        {
            return $" \n VELOCITY__X = {velocity.x} \n VELOCITY__Y = {velocity.y} \n POSITION__X = {pos.x} \n POSITION__Y {pos.y} \n NODE : {parentNode.Name} \n IS__KINEMATIC :{!takingInput} \n";
        }
        public void Update()
        {
            this.pos = parentNode.position;
            this.velocity = parentNode.velocity; 

            if (parentNode == null) return;
            if (takingInput)
            {
                if (Input.GetKeyDown(Key.W))
                {
                    if (isGrounded && !jumpHeld) velocity.y = -5;
                    jumpHeld = true;
                }
                else if (isGrounded) jumpHeld = false;

                if (Input.GetKeyDown(Key.S)) velocity.y = 1;
                if (Input.GetKeyDown(Key.A)) velocity.x += -1;
                if (Input.GetKeyDown(Key.D)) velocity.x += 1;
            }

            parentNode.position = this.pos;
            parentNode.velocity = this.velocity;  
            
           
        }
      
    }
    public class Sprite : Component
    {
        public Vec2 size = new Vec2();
        public Color color;
        public bool isCollider = false;

        public Sprite(Vec2 size, Color color, bool isCollider)
        {
            this.size = size;
            this.color = color;
            this.isCollider = isCollider;
        }
    }

}
