﻿using static pixel_renderer.Input;
using Key = System.Windows.Input.Key;
using Newtonsoft.Json;
using pixel_renderer.FileIO;
using pixel_renderer.ShapeDrawing;
using System.Drawing;
using System.Threading.Tasks;
using pixel_renderer.Assets;
using System.Numerics;
using System.Windows.Controls;

namespace pixel_renderer
{
    public class Player : Component
    {
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

            if (sprite is null)
                return;
            sprite.texture.SetImage(PlayerSprite, sprite.Scale);
        
            RegisterAction(Up, Key.Down);
            RegisterAction(Down, Key.Up);
            RegisterAction(Left, Key.Left);
            RegisterAction(Right, Key.Right);
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
        public static Node Standard()
        {
            Node playerNode = new("Player")
            {
                Position = new Vector2(0, -20)
            };
           
            playerNode.AddComponent<Rigidbody>();
            playerNode.AddComponent<Player>().takingInput = true;
            playerNode.AddComponent<ProjectileSource>();
            playerNode.AddComponent<Sprite>();
            //playerNode.AddComponent<Text>(); 

            var col = playerNode.AddComponent<Collider>();

            col.model = new Box().Polygon;

            playerNode.Scale = Constants.DefaultNodeScale;

            return playerNode;

        }
    }
}
