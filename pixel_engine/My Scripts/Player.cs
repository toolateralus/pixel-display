using static pixel_renderer.Input;
using Key = System.Windows.Input.Key;
using Newtonsoft.Json;
using pixel_renderer.FileIO;
using pixel_renderer.ShapeDrawing;
using System.Drawing;
using System.Threading.Tasks;
using pixel_renderer.Assets;
using System.Numerics;

namespace pixel_renderer
{
    public class Player : Component
    {
        Ray targetRay = new(new(0,0), new(1,0));
        Line targetLine = new(new Vector2(5,0), new Vector2(5, -5));

        [Field][JsonProperty] public bool takingInput = true;
        [Field][JsonProperty] public float speed = 0.01f;
        [Field] private bool isGrounded;
        [Field] public float turnSpeed = 0.1f;
        
        Sprite sprite = new();
        Rigidbody rb = new();

        public Vector2 moveVector = default;
        public static Metadata? PlayerSprite
        {
            get
            {
                return AssetLibrary.FetchMeta("PlayerSprite");
            }
       
        }

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
            }

            Task task = new(async delegate
            {
                if (sprite is null)
                    return;

                while (sprite.texture is null)
                    await Task.Delay(25);

                sprite.texture.SetImage(PlayerSprite, sprite.Scale);

                var node = pixel_renderer.Text.Standard();

                this.node.Child(node.Item1);

            });
            task.Start();
            RegisterAction(Up, Key.Down);
            RegisterAction(Down, Key.Up);
            RegisterAction(Left, Key.Left);
            RegisterAction(Right, Key.Right);
            RegisterAction(LeftArrow, Key.Left);
            RegisterAction(RightArrow, Key.Right);
        }
        public override void OnCollision(Collision collider)
        {
            isGrounded = true;
        }
        public override void FixedUpdate(float delta)
        {
            if (!takingInput)
                return;
            if (isGrounded)
                isGrounded = false;
            Move(moveVector);
            moveVector = Vector2.Zero;
        }
        public override void OnDrawShapes()
        {
            targetRay.position = Position;
            ShapeDrawer.DrawLine(new(targetRay.position, targetRay.position + targetRay.direction * 10000), Pixel.Red);
            if (targetRay.CastToLine(targetLine) is float distance)
            {
                ShapeDrawer.DrawCircle(targetRay.position + (targetRay.direction * distance), 0.2f);
                ShapeDrawer.DrawLine(targetLine, Pixel.Red);
            }
            else
                ShapeDrawer.DrawLine(targetLine, Pixel.Blue);

            if (node.GetComponent<Collider>()?.Polygon is not Polygon poly)
                return;

            if(node.children != null)
            foreach (var child in node.children)
            {
                if (!child.TryGetComponent(out Collider col)) return;
                var centroid = col.Polygon.centroid;
                ShapeDrawer.DrawLine(poly.centroid, centroid, Color.LightCyan);
            }
        }

        #region Input
        private void RightArrow()
        {
            targetRay.direction.Rotate(turnSpeed);
        }
        private void LeftArrow()
        {
            targetRay.direction.Rotate(-turnSpeed);
        }
        void Up()
        {
            if (!Get(Key.LeftShift))
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
                Position = new Vector2(0, -20)
            };
           
            playerNode.AddComponent<Rigidbody>();
            
            playerNode.AddComponent<Player>().takingInput = true;

            var proj = playerNode.AddComponent<ProjectileSource>();
            AddSprite(playerNode);
            
            var col = playerNode.AddComponent<Collider>();

            col.SetModel(new Box().Polygon);

            playerNode.Scale = Constants.DefaultNodeScale;

            return playerNode;

        }
        public static Camera AddCamera(Node node, int height = 256, int width = 256)
        {
            var cam = node.AddComponent<Camera>();
            cam.Scale = new(width, height);
            return cam;
        }
        public static Sprite AddSprite(Node playerNode)
        {
            var sprite = playerNode.AddComponent<Sprite>();
            sprite.Scale = Vector2.One * 36;
            return sprite;
        }

    }
}
