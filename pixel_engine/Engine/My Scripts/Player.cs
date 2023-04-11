using static pixel_renderer.Input;
using Key = System.Windows.Input.Key;
using pixel_renderer.FileIO;
using pixel_renderer.Assets;
using System.Numerics;
using Newtonsoft.Json;

namespace pixel_renderer
{
    public class Player : Component
    {
        [Field][JsonProperty] public bool takingInput = true;
        [Field][JsonProperty] public float speed = 0.1f;
        [Field] private bool isGrounded;
        [Field] public float turnSpeed = 0.1f;
        
        public Vector2 moveVector = default;
        Sprite sprite = new();
        Rigidbody rb = new();

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
            {
                sprite.Type = SpriteType.Image;
                sprite.texture.SetImage(PlayerSprite, sprite.Scale);
            }
            RegisterActions();
        }
        public override void FixedUpdate(float delta)
        {
            if (!takingInput)
                return;
            if (isGrounded)
                isGrounded = false;
            Move(moveVector);
            TryManipulateSoftbody();
            moveVector = Vector2.Zero;
           
        }
        public override void OnCollision(Collision collider)
        {
            isGrounded = true;
        }

        #region TESTING/TO BE REMOVED 
        private void TryManipulateSoftbody()
        {
            if (node.TryGetComponent(out Softbody sb))
            {
                if (moveVector.Y != 0)
                    sb.UniformDeformation(1);
                if (moveVector.X != 0)
                    sb.UniformDeformation(-1);
            }
        }
        #endregion
        #region Input / Controller
        void Up()
        {
            if (!selected_by_editor)
                return;
            moveVector = new Vector2(moveVector.X, 1 * speed);
        }
        void Down()
        {
            if (!Get(Key.LeftShift))
                return;
            moveVector = new Vector2(moveVector.X, -1 * speed);
        }
        void Left()
        {
            if (!Get(Key.LeftShift))
                return;
            moveVector = new Vector2(-1 * speed, moveVector.Y);
        }
        void Right()
        {
            if (!Get(Key.LeftShift))
                return;
            moveVector = new Vector2(1 * speed, moveVector.Y);
        }
        private void RegisterActions()
        {
            RegisterAction(Up, Key.Down);
            RegisterAction(Down, Key.Up);
            RegisterAction(Left, Key.Left);
            RegisterAction(Right, Key.Right);
        }
        private void Move(Vector2 moveVector)
        {
            if (isGrounded)
                rb?.ApplyImpulse(moveVector.WithValue(y: speed * moveVector.Y) * speed);
            else rb?.ApplyImpulse(moveVector * speed);
        }
        #endregion

        public static Node Standard()
        {
            Node playerNode = new("Player")
            {
                Position = new Vector2(-15, -20)
            };
            playerNode.AddComponent<Rigidbody>();
            playerNode.AddComponent<Player>().takingInput = true;
            playerNode.AddComponent<Sprite>();
            playerNode.AddComponent<Collider>();
            playerNode.Scale = Constants.DefaultNodeScale;
            return playerNode;
        }
    }
}
