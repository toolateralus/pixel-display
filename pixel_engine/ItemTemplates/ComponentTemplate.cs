using System;
using System.Collections.Generic;
using pixel_core;
using static pixel_core.Input; 
using static pixel_core.Runtime; 
using static pixel_core.ShapeDrawer; 
using pixel_core.types.Components;
using System.Numerics;
using pixel_core.types.physics;
using pixel_core.types;
using System.Windows.Input;
using pixel_editor;

namespace pixel_core
{
    /// <summary>
    /// This just prevents this class from being instantiated by the add component menu, you should remove this.
    /// </summary>
    [HideFromEditor]
    public class ComponentTemplate : Component
    {
        // If you have any Nodes or Components references, you must at least set them null in Dispose().
        // this is a temporary and hacky fix.
        // example :
        Node? other_node = null; 
        bool y_pressed = false; 
        public override void Dispose()
        {
            other_node = null;
        }
        // Called before first fixed update/update.
        public override void Awake()
        {
            RegisterAction(this, OnKeyDown_Y, Key.Y);
            RegisterAction(this, OnKeyUp_Y, Key.Y, InputEventType.KeyUp);
        }
        public override void OnDestroy()
        {
            Log($"{node.Name} has been destroyed");
        }
        private void OnKeyUp_Y()
        {
            y_pressed = false; 
        }
        private void OnKeyDown_Y()
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
