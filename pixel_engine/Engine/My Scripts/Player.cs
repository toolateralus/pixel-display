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
        [Field] [JsonProperty] public bool takingInput = true;
        [Field] [JsonProperty] public float speed = 0.1f;
        [Field] [JsonProperty] private float jumpSpeed = 0.5f;
        [Field] private bool isGrounded;
        
        public Vector2 moveVector = default;
        Sprite sprite;
        Rigidbody rb;
        Animator anim;

        public static Metadata? PlayerSprite => AssetLibrary.FetchMeta("PlayerSprite");
        public static Metadata test_animation_data(int index)
        {
            string name = $"Animation{index}"; 
            return AssetLibrary.FetchMeta(name);
        }

        public override void Awake()
        {
            node.TryGetComponent(out rb);

            if (node.TryGetComponent(out sprite))
                sprite.Type = SpriteType.Image;

            if (node.TryGetComponent(out anim))
                anim.Next();

            RegisterActions();
        }
        public override void FixedUpdate(float delta)
        {
            if (!takingInput)
                return;

            Move(moveVector);
            TryManipulateSoftbody();

            if (isGrounded)
                isGrounded = false;
            
            bool? playing = anim.GetAnimation()?.playing;
            moveAnimation(playing);

            moveVector = Vector2.Zero;

            void moveAnimation(bool? playing)
            {
                if (rb is not null)
                    if (rb.velocity.Length() < 0.1f)
                        anim.Stop();
                    else if (playing.HasValue && !playing.Value)
                        anim.Start();
            }
        }
        public override void OnCollision(Collision collider)
        {
            isGrounded = true;
        }
        public static Node Standard()
        {
            Node playerNode = Animator.Standard();
            playerNode.tag = "Player";
            playerNode.Position = new Vector2(-15, -20);
            playerNode.AddComponent<Player>().takingInput = true;
            playerNode.Scale = Constants.DefaultNodeScale;
            return playerNode;
        }

        #region TESTING/TO BE REMOVED 
        private void TryManipulateSoftbody()
        {
            if (node.TryGetComponent(out Softbody sb))
            {
                if (moveVector.Y != 0)
                    sb.UniformDeformation(1);
                if (moveVector.Y != 0)
                    sb.UniformDeformation(-1);
            }
        }
        #endregion
        #region Input / Controller

        private void Jump()
        {
            if (isGrounded)
                rb?.ApplyImpulse(-Vector2.UnitY * jumpSpeed);
        }

        void Down()
        {
            moveVector.Y = 1 * speed;
        }
        void Left()
        {
            moveVector.X = -1 * speed;
        }
        void Right()
        {
            moveVector.X = 1 * speed;
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

        #endregion
    }
}
