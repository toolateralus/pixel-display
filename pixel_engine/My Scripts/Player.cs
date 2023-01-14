using pixel_renderer;
using static pixel_renderer.Input;
using System;
using Key = System.Windows.Input.Key;
using System.Collections.Generic;
using Newtonsoft.Json;
using pixel_renderer.FileIO;
using System.DirectoryServices;
using System.Runtime.CompilerServices;

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

        public override void Awake()
        {
            parent.TryGetComponent(out sprite);
            parent.TryGetComponent(out rb);
            CreateInputEvents();

        }

        void Up(object[] e) => moveVector = new Vec2(0, -inputMagnitude);
        void Down(object[] e) => moveVector = new Vec2(0, inputMagnitude);
        void Left(object[] e) => moveVector = new Vec2(-inputMagnitude, 0);
        void Right(object[] e) => moveVector = new Vec2(inputMagnitude, 0);

        private void CreateInputEvents()
        {
            RegisterAction(false, Up, null, Key.W, InputEventType.KeyDown);
            RegisterAction(false, Down, null, Key.S, InputEventType.KeyDown);
            RegisterAction(false, Left, null, Key.A, InputEventType.KeyDown);
            RegisterAction(false, Right, null, Key.D, InputEventType.KeyDown);
        }
        public override void FixedUpdate(float delta)
        {
            if (!takingInput) 
                return;
            Jump(moveVector);
            Move(moveVector);
            moveVector = Vec2.zero; 
        }
        public override void OnTrigger(Rigidbody other) { }
        public override void OnCollision(Rigidbody collider) => sprite.Randomize();
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
            Vec2 playerStartPosition = new Vec2(12, 24);
            Node playerNode = new("Player", playerStartPosition, Vec2.one);
            Rigidbody rb = new()
            {
                IsTrigger = false,
            };
            var imgData = new Metadata("test_sprite_image", Constants.WorkingRoot + Constants.ImagesDir + "\\sprite_24x24.bmp", ".bmp");
            Sprite sprite = new()
            {
                texture = new(imgData),
                size = new(24, 24),
                isCollider = true,
                camDistance = 1,
                dirty = true,
                Type = SpriteType.Image,
                Enabled = true,
                Name = "Player sprite",
            };
            Player player_obj = new()
            {
                takingInput = true
            };
            playerNode.AddComponent(rb);
            playerNode.AddComponent(player_obj);
            playerNode.AddComponent(sprite);
            var cam = playerNode.AddComponent<Camera>();
            cam.Size = new(256, 256);
            
            nodes.Add(playerNode);
        }
    }

}