﻿using pixel_renderer;
using static pixel_renderer.Input;
using System;
using Key = System.Windows.Input.Key;
using System.Collections.Generic;
using Newtonsoft.Json;
using pixel_renderer.FileIO;
using System.DirectoryServices;
using System.Runtime.CompilerServices;
using System.Drawing;

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

        private void CreateInputEvents()
        {
            RegisterAction(Up,  Key.W);
            RegisterAction(Down,Key.S);
            RegisterAction(Left,  Key.A);
            RegisterAction(Right, Key.D);
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
        public override void OnCollision(Collider collider) => sprite.Randomize();
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
         
            playerNode.AddComponent<Rigidbody>();
            var sprite = playerNode.AddComponent<Sprite>();
            
            playerNode.AddComponent<Player>().takingInput = true;

            var col = playerNode.AddComponent<Collider>();

            col.IsTrigger = false;
            col.size = sprite.size;

            var cam = playerNode.AddComponent<Camera>();
            cam.Size = new(256, 256);
            
            nodes.Add(playerNode);
        }
    }

}