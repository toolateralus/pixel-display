using pixel_renderer;
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

namespace pixel_renderer.Scripts
{

    public class Player : Component
    {
        [Field] [JsonProperty] public bool takingInput = true;
        [Field] [JsonProperty] public int speed = 4;
        [Field] [JsonProperty] public float inputMagnitude = 1f;

        [Field] Sprite sprite = new();
        [Field] Rigidbody rb = new();
        [Field] public Vec2 moveVector;

        public static Metadata test_image_data = new("test_sprite_image", Constants.WorkingRoot + Constants.ImagesDir + "\\sprite_24x24.bmp", ".bmp");

        public override void Awake()
        {
            parent.TryGetComponent(out sprite);
            parent.TryGetComponent(out rb);
            CreateInputEvents();
        }

        void Up(object[]? e) => moveVector = new Vec2(0, -inputMagnitude);
        void Down(object[]? e) => moveVector = new Vec2(0, inputMagnitude);
        void Left(object[]? e) => moveVector = new Vec2(-inputMagnitude, 0);
        void Right(object[]? e) => moveVector = new Vec2(inputMagnitude, 0);

        int res_incrementer; 
        void IncreaseResolution(object[]? e)
        {
            var renderer = Runtime.Instance.renderHost.GetRenderer();
            var incrementAmt = 1;

            for (int i = 0; i < 2; i++)
            {
                if (renderer.Resolution[i] < Constants.MaxResolution[i])
                    renderer.Resolution[i] += incrementAmt;
            }
            res_incrementer++;

            if(res_incrementer % 25 == 0)
                Runtime.Log(((Vec2)renderer.Resolution).AsString());

        }
        void DecreaseResolution(object[]? e)
        {
            var renderer = Runtime.Instance.renderHost.GetRenderer();
            var decrementAmt = 1;

            for (int i = 0; i < 2; i++)
                if (renderer.Resolution[i] > Constants.MinResolution[i])
                    renderer.Resolution[i] -= decrementAmt;

            if (res_incrementer % 25 == 0)
                Runtime.Log(((Vec2)renderer.Resolution).AsString());
            res_incrementer--;
        }

        private void CreateInputEvents()
        {
            RegisterAction(Up,  Key.W);
            RegisterAction(Down,Key.S);
            RegisterAction(Left,  Key.A);
            RegisterAction(Right, Key.D);

            RegisterAction(IncreaseResolution, Key.OemPlus);
            RegisterAction(DecreaseResolution, Key.OemMinus);
        }
        public override void FixedUpdate(float delta)
        {
            if (!takingInput) 
                return;
            Jump(moveVector);
            Move(moveVector);
            moveVector = Vec2.zero; 
        }
        public override void OnTrigger(Collider other) { }
        public override void OnCollision(Collider collider)
        {
            if (JRandom.Bool())
                sprite.Randomize();
            else sprite.DrawSquare(sprite.size, JRandom.Color());
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
                position = JRandom.Vec2(Vec2.zero, Vec2.one * 256)
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
                position = JRandom.Vec2(Vec2.zero, Vec2.one * 256)
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