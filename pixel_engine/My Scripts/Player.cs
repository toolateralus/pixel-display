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

namespace pixel_renderer.Scripts
{

    public class Player : Component
    {
        [Field] [JsonProperty] public bool takingInput = true;
        [Field] [JsonProperty] public int speed = 8;
        [Field] [JsonProperty] public float inputMagnitude = 1f;

        [Field] Sprite sprite = new();
        [Field] Rigidbody rb = new();
        [Field] public Vec2 moveVector;

        private Animator? anim;
        private int resolution_increment; 
        
        public static Metadata test_image_data = new("test_sprite_image", Constants.WorkingRoot + Constants.ImagesDir + "\\sprite_24x24.bmp", Constants.BitmapFileExtension);
        public static Metadata test_animation_data(int index) => new("test animation image data", Constants.WorkingRoot + Constants.ImagesDir + $"\\sprite_24x24 {index}.bmp", Constants.BitmapFileExtension);
        public static Node test_child_node(Node? parent = null)
        {
            Node node = new("Player Child");

            if (parent != null)
                node.Position = parent.Position + Vec2.up * 16;
            else node.Position = JRandom.Vec2(Vec2.zero, Vec2.one * Constants.ScreenW); 

            var sprite = AddSprite(node);

            AddCollider(node, sprite);

            Runtime.Instance.GetStage().AddNode(node);

            return node;
        }
        public override void Awake()
        {
            CreateInputEvents();
            parent.TryGetComponent(out rb);
            parent.TryGetComponent(out sprite);
            ShapeDrawer.TryRegister(this, OnDrawShapes);
        }
        public void OnDrawShapes()
        {
            ShapeDrawer.DrawLine(parent.Position - moveVector, parent.Position + (Vec2.right * 100), Color.Orange);
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
            var renderer = Runtime.Instance.renderHost.GetRenderer();
            var incrementAmt = 1;

            for (int i = 0; i < 2; i++)
            {
                if (renderer.Resolution[i] < Constants.MaxResolution[i])
                    renderer.Resolution[i] += incrementAmt;
            }
            resolution_increment++;

            if(resolution_increment % 25 == 0)
                Runtime.Log(renderer.Resolution.AsString());

        }
        void DecreaseResolution(object[]? e)
        {
            var renderer = Runtime.Instance.renderHost.GetRenderer();
            var decrementAmt = 1;

            for (int i = 0; i < 2; i++)
                if (renderer.Resolution[i] > Constants.MinResolution[i])
                    renderer.Resolution[i] -= decrementAmt;

            if (resolution_increment % 25 == 0)
                Runtime.Log(((Vec2)renderer.Resolution).AsString());
            resolution_increment--;
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

        public override void FixedUpdate(float delta)
        {
            if (!takingInput) 
                return;
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

        private void Move(Vec2 moveVector)
        {
            rb.velocity.x += moveVector.x * speed;
        }
        private void Jump(Vec2 moveVector)
        {
            var jumpVel = speed * 2;
            rb.velocity.y = moveVector.y * jumpVel;
        }

        public static void AddPlayer(List<Node> nodes)
        {
            Node playerNode = new("Player")
            {
                Position = JRandom.Vec2(Vec2.zero, Vec2.one * 256)
            };

            playerNode.AddComponent<Rigidbody>();
            playerNode.AddComponent<Player>().takingInput = true;
            
            Sprite sprite = AddSprite(playerNode);
            AddCollider(playerNode, sprite);
            AddCamera(playerNode);

            nodes.Add(playerNode);

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
            AddCamera(playerNode);

            return playerNode;
        }

        public static void AddCamera(Node playerNode, int height = 256, int width = 256, DrawingType type = DrawingType.Wrapped)
        {
            var cam = playerNode.AddComponent<Camera>();
            cam.DrawMode = type;
            cam.Size = new(width, height);
        }
        public static void AddCollider(Node playerNode, Sprite sprite)
        {
            var col = playerNode.AddComponent<Collider>();

            col.IsTrigger = false;
            col.size = sprite.size;
        }
        public static Sprite AddSprite(Node playerNode)
        {
            var sprite = playerNode.AddComponent<Sprite>();
            sprite.size = Vec2.one * 36;
            return sprite;
        }
    }

}