using static pixel_renderer.Input;
using Key = System.Windows.Input.Key;
using Newtonsoft.Json;
using pixel_renderer.FileIO;
using System.Drawing;
using System.Threading.Tasks;
using pixel_renderer.Assets;
using System.Linq;
using System.Numerics;
using pixel_renderer;
using pixel_renderer.ShapeDrawing;
using System;

namespace pixel_renderer
{
    public class Player : Component
    {
        [Field][JsonProperty] public bool takingInput = true;
        [Field][JsonProperty] public float speed = 0.1f;
        [Field][JsonProperty] public float inputMagnitude = 0.5f;
        [Field] private bool isGrounded;
        Sprite sprite = new();
        Rigidbody rb = new();
        
        private Vector2 thisPos;
        private bool freezeButtonPressedLastFrame = false;
        private Curve curve = null; 

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
        public static Node test_child_node(Node? parent = null)
        {
            Node node = new("Player Child");
            
            if (parent != null)
                node.Position = parent.Position + Vector2.UnitY * 16;

            else node.Position = JRandom.Vec2(Vector2.Zero, Vector2.One * Constants.ScreenW); 
            AddCamera(node);
            return node;
        }

        public override void Awake()
        {
            node.TryGetComponent(out rb);
            if (node.TryGetComponent(out sprite))
            {
                sprite.Type = SpriteType.Image;
                curve = Curve.Circlular(1, 16, radius: sprite.Scale.X /2, looping: true);
            }
            Task task = new(async delegate
            {
                if (sprite is null) 
                    return; 

                while (sprite.texture is null)
                    await Task.Delay(25);

                sprite.texture.SetImage(PlayerSprite, sprite.Scale);

                await Task.Delay(2500);

                node.AddComponent<Text>(); 

            });
            task.Start();
            DrawCircle();


        }
        public override void OnCollision(Collider collider)
        {
            isGrounded = true; 
        }
        public override void FixedUpdate(float delta)
        {

            if (!takingInput)
                return;

            if (isGrounded)
                isGrounded = false;

            Move(MoveVector * 0.01f);
        }
        
        private void Move(Vector2 moveVector)
        {
            rb.ApplyImpulse(moveVector.WithValue(y:0) * speed);
        }
        public void FreezePlayer()
        {
            bool freezeButtonPressed = Get(Key.LeftShift);

            if (freezeButtonPressed && !freezeButtonPressedLastFrame)
                thisPos = node.Position;


            freezeButtonPressedLastFrame = freezeButtonPressed;

            if (freezeButtonPressed)
            {
                foreach (var child in node.children)
                    child.Value.localPos += MoveVector;

                node.Position = thisPos;
            }
        }
        private void DrawCircle()
        {
            var size = sprite.texture.scale;
            Pixel[,] colors = new Pixel[(int)size.X, (int)size.Y];

            for(int i = 0; i < curve.points.Values.Count; ++i)
            {
                Vector2 pos = curve.points.Values.ElementAt(i);
                pos += size / 2; 

                if(pos.IsWithinMaxExclusive(Vector2.Zero, size))
                    colors[(int)pos.X, (int)pos.Y] = (Pixel)Color.Red;
                
            }
            sprite?.SetImage(colors);
        }

        public static Node Standard()
        {
            Node playerNode = new("Player")
            {
                Position = new Vector2(0, -20)
            };
           
            playerNode.AddComponent<Rigidbody>();
            playerNode.AddComponent<Player>().takingInput = true;
            playerNode.AddComponent<ProjectileSource>(); 

            AddSprite(playerNode);
            var col = playerNode.AddComponent<Collider>();

            

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
        public override void OnDrawShapes()
        {
            if(node.GetComponent<Collider>()?.Polygon is not Polygon poly)
                return;
            foreach (var child in node.children)
            {
                if (!child.Value.TryGetComponent(out Collider col)) return;
                var centroid = col.Polygon.centroid;
                ShapeDrawer.DrawLine(poly.centroid, centroid, Color.LightCyan);
            }
        }
    }
}
