using System.Windows.Input;
using Color = System.Drawing.Color;

namespace PixelRenderer.Components
{

    public abstract class Component
    {
        public Node parentNode = new(); 
        public virtual void Update()
        {

        }
        public virtual void Awake()
        {

        }
    }
    public class Rigidbody : Component
    {
        public Vec2 position = new Vec2();
        public Vec2 velocity = new Vec2();
        public bool isGrounded = false;
        public bool takingInput { get; set; } = false;
        bool jumpHeld = false;

        public string GetDebugs()
        {
            return $" \n VELOCITY__X = {velocity.x} \n VELOCITY__Y = {velocity.y} \n POSITION__X = {position.x} \n POSITION__Y {position.y} \n NODE : {parentNode.Name} \n IS__KINEMATIC :{!takingInput} \n";
        }
        public override void Update()
        {
            this.position = parentNode.position;
            this.velocity = parentNode.velocity;

            if (parentNode == null) return;
            if (takingInput)
            {
                if (Input.GetKeyDown(Key.W) || Input.GetKeyDown(Key.Space))
                {
                    if (isGrounded && !jumpHeld) velocity.y = -20;
                    jumpHeld = true;
                }
                else if (isGrounded) jumpHeld = false;

                if (Input.GetKeyDown(Key.S)) velocity.y = 1;
                if (Input.GetKeyDown(Key.A)) velocity.x += -1;
                if (Input.GetKeyDown(Key.D)) velocity.x += 1;

                // change guy's color based on velocity + position on 64x64
                
                if (parentNode.sprite != null)
                {
                    parentNode.sprite = new Sprite(new Vec2(4, 4), Color.FromArgb(255, (byte)(velocity.x), (byte)(velocity.y), (byte)(velocity.y)), true);  ;
                }
            }
            parentNode.position = this.position;
            parentNode.velocity = this.velocity;
        }
    }
    public class Sprite : Component
    {
        public Vec2 size = new Vec2();
        public Color[,] colorData;
        public bool isCollider = false;
        public Sprite(Vec2 size, Color color, bool isCollider)
        {
            colorData = new Color[(int)size.x, (int)size.y];
            for (int x = 0; x < size.x; x++)
            for (int y = 0; y < size.y; y++)
            {
                this.colorData[x, y] = color;
            }
            this.size = size;
            this.isCollider = isCollider;
        }
        public Sprite(Color[,] colorData)
        {
            this.colorData = colorData;
        }
        public Sprite(Vec2 size)
        {
            this.size = size;
        }
        public Sprite(Vec2 size, bool isCollider)
        {
            this.size = size;
            this.isCollider = isCollider;
        }
        public Sprite()
        {

        }
    }
}
