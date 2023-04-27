using pixel_core;
using pixel_core.types;
using pixel_core.types.Components;
using pixel_core.types.physics;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Numerics;
using System.Windows.Input;
using static pixel_core.Input;
using static pixel_core.Runtime;
using static pixel_core.ShapeDrawer;

namespace pixel_core
{
    public class CharacterController : Component
    {
        bool y_pressed = false;
        Rigidbody rb;
        Sprite sprite;
        public override void Dispose()
        {
            rb = null;
            sprite = null; 
        }
        // Called before first fixed update/update.
        public override void Awake()
        {

            rb = GetComponent<Rigidbody>();
            sprite = GetComponent<Sprite>();

            RegisterAction(this, KeyDownW, Key.W);
            RegisterAction(this, KeyUpW, Key.W, InputEventType.KeyUp);
        }
        public override void OnDestroy()
        {
            Log($"{node.Name}'s {nameof(CharacterController)} has been destroyed");
        }
        private void KeyUpW()
        {
            y_pressed = false;
        }
        private void KeyDownW()
        {
            if (y_pressed)
                return;

            Log("You're pressing Y.");
            y_pressed = true;
        }
        public override void OnDrawShapes()
        {
            if (!y_pressed)
                return;

            DrawLine(Position, Position + (Vector2.One * 5), Pixel.Red);
        }
        // Called every rendering frame.
        public override void Update()
        {

        }
        // Called every physics frame.
        public override void FixedUpdate(float delta)
        {

        }
    }
}
