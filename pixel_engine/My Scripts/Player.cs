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

namespace pixel_renderer.Scripts
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
            curve = Curve.Circlular(1, 16, magnitude : 64, looping: true);

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
                    child.Value.localPos = curve.Next();

                parent.Position = thisPos;
                moveVector = Vec2.zero;
                return;
            }

            if (CMouse.MouseWheelDelta != 0)
                camera.Size += Vec2.one * CMouse.MouseWheelDelta;

            Jump(moveVector);
            Move(moveVector);

            moveVector = Vec2.zero;
        }
        public override void OnTrigger(Collider other) 
        { 
        }
        public override void OnCollision(Collider collider)
        {
            if (JRandom.Bool())
                sprite?.Randomize();
            else sprite?.DrawSquare(sprite.size, JRandom.Color());
        }

        void MakeChildObject(object[]? e)
        {
            Node node = test_child_node();
            parent.Child(node);
        }

       

        void Up(object[]? e) => moveVector = new Vec2(moveVector.x, -inputMagnitude);
        void Down(object[]? e) => moveVector = new Vec2(moveVector.x, inputMagnitude);
        void Left(object[]? e) => moveVector = new Vec2(-inputMagnitude, moveVector.y);
        void Right(object[]? e) => moveVector = new Vec2(inputMagnitude, moveVector.y);

        void MakeTransparent(object[]? e) => sprite?.DrawSquare(sprite.size, Color.FromArgb(5, 76, 185, 99));

        void StartAnim(object[]? e)
        {
            if (parent.TryGetComponent<Animator>(out anim))
            {
                anim.Start();
                Runtime.Log("Animation Started.");
            }
            else
            {
                Runtime.Log("Animator not found. Creating one.");
                anim = parent.AddComponent<Animator>();
                anim.GetAnimation().looping = true;
            }
        }
        void StopAnim(object[]? e)
        {
            if (parent.TryGetComponent(out anim))
            {
                anim.Stop();
                Runtime.Log("Animation Stopped.");
            }
            else
            {
                Runtime.Log("Animator not found. Creating one.");
                anim = parent.AddComponent<Animator>();
            }
        }
       

        void IncreaseResolution(object[]? e)
        {
            RenderHost renderHost = Runtime.Current.renderHost;
            var renderer = renderHost.GetRenderer();

            if (renderer.Resolution.x < Constants.MaxResolution.x)
                renderHost.newResolution = renderer.Resolution + Vec2.one;
        }
        void DecreaseResolution(object[]? e)
        {
            var renderer = Runtime.Current.renderHost.GetRenderer();
            RenderHost renderHost = Runtime.Current.renderHost;

            if (renderer.Resolution.x > Constants.MaxResolution.x)
                renderHost.newResolution = renderer.Resolution - Vec2.one;
        }

        private void CreateInputEvents()
        {
            RegisterAction(Up,  Key.W);
            RegisterAction(Down,Key.S);

            RegisterAction(Left,  Key.A);
            RegisterAction(Right, Key.D);


            RegisterAction(StartAnim, Key.NumPad9);
            RegisterAction(StopAnim, Key.NumPad6);

            RegisterAction(IncreaseResolution, Key.NumPad2);
            RegisterAction(DecreaseResolution, Key.NumPad1);

            RegisterAction(MakeTransparent, Key.NumPad0);
            RegisterAction(MakeChildObject, Key.NumPad4);
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
            var mesh = parent.GetComponent<Collider>().Mesh;
            foreach (var child in parent?.children)
            {
                if (!child.Value.TryGetComponent(out Collider col)) return;
                var centroid = col.Mesh.centroid;
                ShapeDrawer.DrawLine(mesh.centroid, centroid, Color.LightCyan);
            }
        }

    }

}