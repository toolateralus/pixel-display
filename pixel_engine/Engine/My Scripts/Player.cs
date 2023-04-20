using static pixel_renderer.Input;
using Key = System.Windows.Input.Key;
using pixel_renderer.FileIO;
using pixel_renderer.Assets;
using System.Numerics;
using Newtonsoft.Json;
using System.Windows.Input;

namespace pixel_renderer
{
    public class Player : Component
    {
        [Field] [JsonProperty] public float speed = 0.1f;
        [Field] [JsonProperty] private float jumpSpeed = 0.25f;
        
        [Field] [JsonProperty] public bool takingInput = true;
        [Field] [JsonProperty] private bool followPlayer;
        
        private bool isGrounded;
        private Vector2 moveVector = default;
        private int haltIterations = 16;
        
        Sprite sprite;
        Rigidbody rb;
        Animator anim;
        Camera cam;


        public override void Awake()
        {
            node.TryGetComponent(out rb);

            if (node.TryGetComponent(out sprite))
                sprite.Type = ImageType.Image;

            if (node.TryGetComponent(out anim))
                anim.Next();

            RegisterActions();
        }
        public override void FixedUpdate(float delta)
        {
            cam ??= Camera.First;

            if (!takingInput)
                return;

            Move(moveVector);

            if (isGrounded)
                isGrounded = false;
            
            bool? playing = anim.GetAnimation()?.playing;
            moveAnimation(playing);

            if (moveVector != Vector2.Zero)
                moveVector = Vector2.Zero;
            else rb?.ApplyImpulse((-rb.velocity / haltIterations));

            if (cam != null && followPlayer)
                cam.Position = Position;

            void moveAnimation(bool? playing)
            {
                if (rb is not null)
                    if (rb.velocity.Length() < 0.05f)
                    {
                        anim.Stop();
                        sprite.texture.SetImageRelative("\\Assets\\Animations\\Dog walking\\dog_standing.bmp");
                    }
                    else if (playing.HasValue && !playing.Value)
                        anim.Start();
            }
        }
        public override void OnCollision(Collision collider)
        {
            isGrounded = true;
        }

        private void RegisterActions()
        {
            RegisterAction(Jump, Key.Up);
            RegisterAction(Down, Key.Down);
            RegisterAction(Left, Key.Left);
            RegisterAction(Right, Key.Right);
        }
        private void Move(Vector2 moveVector)
        {
            if (sprite is not null)
            {
                if (moveVector.X < 0)
                    sprite.Scale = new(-1, 1);
                if (moveVector.X > 0)
                    sprite.Scale = new(1, 1);
            }
            if (isGrounded)
                rb?.ApplyImpulse(moveVector.WithValue(y: speed * moveVector.Y) * speed);
            else rb?.ApplyImpulse(moveVector * speed);
        }
        
        #region Input 
        private void Jump()
        {
            if (isGrounded)
            {
                string path = @"D:\Nightmare folder\Games\Documents\Pixel\Assets\Audio Assets\Cannon Sound Effect.mp3";
                Audio.Play(path, 0.35f);
                rb?.ApplyImpulse(-Vector2.UnitY * jumpSpeed * (1f + rb.velocity.Length()));
            }

        }
        private void Down()
        {
            moveVector.Y = 1 * speed;
        }
        private void Left()
        {
            moveVector.X = -1 * speed;
        }
        private void Right()
        {
            moveVector.X = 1 * speed;
        }
        #endregion

        public static Node Standard()
        {
            Node playerNode = Animator.Standard();
            playerNode.tag = "Player";
            playerNode.Position = new Vector2(-15, -20);
            playerNode.AddComponent<Player>().takingInput = true;
            playerNode.Scale = Constants.DefaultNodeScale;
            return playerNode;
        }
    }
}
