using System.Windows.Input;
using Color = System.Drawing.Color;
namespace PixelRenderer.Components
{
    public class Rigidbody : Component
    {
        
        public Vec2 velocity = new();
        public bool isGrounded = false;
        public bool TakingInput { get; set; } = false;
        bool jumpHeld = false;

        public string GetDebugs()
        {
            return $" \n VELOCITY__X = {velocity.x} \n VELOCITY__Y = {velocity.y} \n POSITION__X = {parentNode.position.x} \n POSITION__Y {parentNode.position.y} \n NODE : {parentNode.Name} \n IS__KINEMATIC :{!TakingInput} \n";
        }
        public override void Update()
        {
            ApplyPhysics(); 
            if (parentNode == null) return;
            if (TakingInput)
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
        }
        public void ApplyPhysics()
        {
            Sprite sprite = new(); 
            if (parentNode.sprite != null)
            {
                sprite = parentNode.sprite; 

            }
            velocity.y += Math.Gravity;
            velocity.y *= 0.4f;
            velocity.x *= 0.4f;

            parentNode.position.y += velocity.y;
            parentNode.position.x += velocity.x;

            if (sprite != null && sprite.isCollider)
            {
                if (parentNode.position.y > Pixel.screenHeight - 4 - sprite.size.y)
                {
                    isGrounded = true;
                    parentNode.position.y = Pixel.screenHeight - 4 - sprite.size.y;
                }
                else isGrounded = false;

                if (parentNode.position.x > Pixel.screenWidth - sprite.size.x)
                {
                    parentNode.position.x = Pixel.screenWidth - sprite.size.x;
                    velocity.x = 0;
                    var engine = (Engine)System.Windows.Application.Current.MainWindow;
                    var runtime = engine.runtime; 
                    StageManager.SetCurrentStage(new Stage("new", runtime.Backgrounds[runtime.BackroundIndex + 1], new Node[16]), engine);
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

