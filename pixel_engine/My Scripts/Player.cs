using static pixel_renderer.Input;
using System;
using Key = System.Windows.Input.Key;
using System.Collections.Generic;
using Newtonsoft.Json;
using pixel_renderer.FileIO;
using System.DirectoryServices;
using System.Runtime.CompilerServices;
using System.Drawing;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Windows.Data;

namespace pixel_renderer
{

    public class Player : Component
    {
        [Field][JsonProperty] public bool takingInput = true;
        [Field][JsonProperty] public float speed = 8;
        [Field][JsonProperty] public float inputMagnitude = 1f;
        [Field] [JsonProperty] private Animator? anim;

        [Field] Sprite sprite = new();
        [Field] Rigidbody rb = new();
        [Field] public Vec2 moveVector;

        
        public static Metadata test_image_data = new("test_sprite_image", Constants.WorkingRoot + Constants.ImagesDir + "\\sprite_24x24.bmp", Constants.BitmapFileExtension);
        
        private bool freezeButtonPressedLastFrame = false;
        private Vec2 thisPos;

        private Node cameraNode;
        private Camera camera;
        Curve curve;
        private int scrollSpeed;

        public static Metadata test_animation_data(int index) => new("test animation image data", Constants.WorkingRoot + Constants.ImagesDir + $"\\sprite_24x24 {index}.bmp", Constants.BitmapFileExtension);
        public static Node test_child_node(Node? parent = null)
        {
            Node node = new("Player Child");

            if (parent != null)
                node.Position = parent.Position + Vec2.up * 16;
            else node.Position = JRandom.Vec2(Vec2.zero, Vec2.one * Constants.ScreenW); 

            var sprite = AddSprite(node);
            sprite.DrawSquare(Vec2.one * 16, Color.Red);
            AddCollider(node, sprite);

            Runtime.Current.GetStage().AddNode(node);

            return node;
        }
     
        public override void Awake()
        {
            CreateInputEvents();
            parent.TryGetComponent(out rb);
            parent.TryGetComponent(out sprite);

            cameraNode = test_child_node();
            parent.Child(cameraNode);

            cameraNode.localPos = Vec3.zero;
            cameraNode.Position = parent.Position; 

            camera = Player.AddCamera(cameraNode);
            curve = Curve.Circlular(1, 16, radius : 64, looping: true);

        }

     


        public override void FixedUpdate(float delta)
        {


            if (!takingInput) 
                return;

            var freezeButtonPressed = GetInputValue(Key.LeftShift);

            if (freezeButtonPressed && !freezeButtonPressedLastFrame)
                thisPos = parent.Position;
        

            freezeButtonPressedLastFrame = freezeButtonPressed;

            if (freezeButtonPressed)
            {
                foreach (var child in parent.children)
                    child.Value.localPos += moveVector;

                if (CMouse.MouseWheelDelta > 0)
                    scrollSpeed += 1;
                else scrollSpeed -= 1;

                parent.Position = thisPos;
                moveVector = Vec2.zero;
                return;
            }


            Jump(moveVector);
            Move(moveVector);

            moveVector = Vec2.zero;
        }
        void Up(object[]? e) => moveVector = new Vec2(moveVector.x, -inputMagnitude);
        void Down(object[]? e) => moveVector = new Vec2(moveVector.x, inputMagnitude);
        void Left(object[]? e) => moveVector = new Vec2(-inputMagnitude, moveVector.y);
        void Right(object[]? e) => moveVector = new Vec2(inputMagnitude, moveVector.y);

        private void CreateInputEvents()
        {
            RegisterAction(Up,  Key.W);
            RegisterAction(Down,Key.S);

            RegisterAction(Left,  Key.A);
            RegisterAction(Right, Key.D);
        }
        private void Move(Vec2 moveVector)
        {
            rb.velocity.x += moveVector.x * speed;
        }
        private void Jump(Vec2 moveVector)
        {
            var jumpVel = speed * 2;
            rb.velocity.y = moveVector.y * jumpVel;
        }

        public static Node Standard()
        {
            Node playerNode = new("Player")
            {
                Position = JRandom.Vec2(Vec2.zero, Vec2.one * 256)
            };
           

            playerNode.AddComponent<Rigidbody>();
            playerNode.AddComponent<Player>().takingInput = true;

            Sprite sprite = AddSprite(playerNode);

            AddCollider(playerNode, sprite);

            return playerNode;
        }

        public static Camera AddCamera(Node node, int height = 256, int width = 256, DrawingType type = DrawingType.Wrapped)
        {
            var cam = node.AddComponent<Camera>();
            cam.DrawMode = type;
            cam.Size = new(width, height);
            return cam;
        }
        public static Collider AddCollider(Node playerNode, Sprite sprite)
        {
            var col = playerNode.AddComponent<Collider>();

            col.IsTrigger = false;
            col.size = sprite.size;
            return col;
        }
        public static Sprite AddSprite(Node playerNode)
        {
            var sprite = playerNode.AddComponent<Sprite>();
            sprite.size = Vec2.one * 36;
            return sprite;
        }

        public override void OnDrawShapes()
        {
            var mesh = parent.GetComponent<Collider>().Polygon;
            foreach (var child in parent?.children)
            {
                if (!child.Value.TryGetComponent(out Collider col)) return;
                var centroid = col.Polygon.centroid;
                ShapeDrawer.DrawLine(mesh.centroid, centroid, Color.LightCyan);
            }
        }

    }

}