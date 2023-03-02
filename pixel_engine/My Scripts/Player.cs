﻿using static pixel_renderer.Input;
using System;
using Key = System.Windows.Input.Key;
using System.Collections.Generic;
using Newtonsoft.Json;
using pixel_renderer.FileIO;
using System.Drawing;
using System.Threading.Tasks;
using pixel_renderer.Assets;
using System.Linq;
using System.Numerics;

namespace pixel_renderer
{

    public class Player : Component
    {
        [Field][JsonProperty] public bool takingInput = true;
        [Field][JsonProperty] public float speed = 3;
        [Field][JsonProperty] public float inputMagnitude = 1f;

        [Field] Sprite sprite = new();
        [Field] Rigidbody rb = new();
        [Field] public Vector2 moveVector;

        
        
        private bool freezeButtonPressedLastFrame = false;
        private Vector2 thisPos;
        private Curve curve;
        [Field] private bool isGrounded;

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
            CreateInputEvents();
            parent.TryGetComponent(out rb);
            if (parent.TryGetComponent(out sprite))
            {
                sprite.Type = SpriteType.Image;
                curve = Curve.Circlular(1, 16, radius: sprite.size.X /2, looping: true);
            }

            Task task = new(async delegate
            {
                if (sprite is null) return; 
                while (sprite.texture is null)
                    await Task.Delay(25);
                sprite.texture.SetImage(PlayerSprite, (Vector2)sprite.size);
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

            var freezeButtonPressed = Get(Key.LeftShift);

            if (freezeButtonPressed && !freezeButtonPressedLastFrame)
                thisPos = parent.Position;


            freezeButtonPressedLastFrame = freezeButtonPressed;

            if (freezeButtonPressed)
            {
                foreach (var child in parent.children)
                    child.Value.localPos += moveVector;

                parent.Position = thisPos;
                moveVector = Vector2.Zero;
                return;
            }

            if (isGrounded)
            {
                isGrounded = false;
                Jump(moveVector);
            }

            Move(moveVector);
            moveVector = Vector2.Zero;
        }

        private void DrawCircle()
        {
            Pixel[,] colors = new Pixel[(int)sprite.size.X, (int)sprite.size.Y];


            for(int i = 0; i < curve.points.Values.Count; ++i)
            {
                Vector2 pos = curve.points.Values.ElementAt(i);
                pos += sprite.size / 2; 

                if(pos.IsWithinMaxExclusive(Vector2.Zero, new(sprite.size.X, sprite.size.Y)))
                    colors[(int)pos.X, (int)pos.Y] = (Pixel)Color.Red;
                
            }
            sprite.Draw((Vector2)sprite.size, CBit.ByteArrayFromColorArray(colors));
        }

        void Up(object[]? e) => moveVector = new Vector2(moveVector.X -inputMagnitude);
        void Down(object[]? e) => moveVector = new Vector2(moveVector.X, inputMagnitude);
        void Left(object[]? e) => moveVector = new Vector2(-inputMagnitude, moveVector.Y);
        void Right(object[]? e) => moveVector = new Vector2(inputMagnitude, moveVector.Y);

        private void CreateInputEvents()
        {
            RegisterAction(Up,  Key.W);
            RegisterAction(Down,Key.S);

            RegisterAction(Left,  Key.A);
            RegisterAction(Right, Key.D);
        }
        private void Move(Vector2 moveVector)
        {
            rb.velocity.X += moveVector.X * speed;
        }
        private void Jump(Vector2 moveVector)
        {
            var jumpVel = speed * 2;
            rb.velocity.Y = moveVector.Y * jumpVel;
        }

        public static Node Standard()
        {
            Node playerNode = new("Player")
            {
                Position = new Vector2(0, -20)
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
            col.SetVertices(sprite.GetVertices());
            col.IsTrigger = false;
            return col;
        }
        public static Sprite AddSprite(Node playerNode)
        {
            var sprite = playerNode.AddComponent<Sprite>();
            sprite.size = Vector2.One * 36;
            return sprite;
        }

        public override void OnDrawShapes()
        {
            if(parent.GetComponent<Collider>()?.Polygon is not Polygon poly)
                return;
            foreach (var child in parent.children)
            {
                if (!child.Value.TryGetComponent(out Collider col)) return;
                var centroid = col.Polygon.centroid;
                ShapeDrawer.DrawLine(poly.centroid, centroid, Color.LightCyan);
            }
        }

    }

}