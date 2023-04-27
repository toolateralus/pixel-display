using System.Collections.Generic;
using System;
using System.Linq;
using System.Windows.Input;
using System.Numerics;
using Pixel;

namespace pixel_editor
{
    public enum StageCameraState : byte { Following, Idle, Inactive};

    public class StageCameraTool : Tool
    {
        private const Key StopFollowingNodeKey = Key.Escape;
        /// <summary>
        /// when held during a FocusNodeKey press, will follow.
        /// </summary>
        private const Key FollowModifier = Key.LeftCtrl;
        private const Key FocusNodeKey = Key.F;

        public Camera camera;
        public Node? selected;
        private static bool followNode;
        private bool draggingCam;
        private Vector2 mouseGlobalPos;

        public override void Awake()
        {
            CMouse.OnMouseMove += UpdateCamPosition;
        }

        public override void Update(float delta)
        {
            selected = Editor.Current.LastSelected;
            TryFollowNode(selected);
            TryFocusNode();
            TryZoomCamera();
        }

        public static void TryFollowNode(Node node)
        {
            if (node is null || !followNode)
                return;
            
            

            Camera.First.node.Position = node.Position;
            
            if (!Input.Get(StopFollowingNodeKey))
                return;

            followNode = true;
        }

        private void TryFocusNode()
        {
            if (selected == null)
                return;

            bool selectNode = Input.Get(FocusNodeKey);

            if (!selectNode)
                return;

            Camera.First.node.Position = selected.Position;

            if (!Input.Get(FollowModifier))
                return;

            followNode = false; 
        }
        public static void TryFocusNode(Node node)
        {
            if (node == null)
                return;
            
            

            bool selectNode = Input.Get(Key.LeftShift) && Input.Get(Key.Space);

            if (!selectNode)
                return;

            Camera.First.node.Position = node.Position;

            if (!Input.Get(FollowModifier))
                return;

            followNode = false; 
        }

        private void TryZoomCamera()
        {
            if (CMouse.MouseWheelDelta == 0)
                return;
        
            Camera.First.Scale *= MathF.Pow(Editor.Current.settings.MouseZoomSensitivityFactor, -CMouse.MouseWheelDelta);
        }

        private void UpdateCamPosition()
        {
            if (!CMouse.Right)
            {
                draggingCam = false;
                return;
            }
            if (!draggingCam)
            {
                draggingCam = true;
                mouseGlobalPos = CMouse.GlobalPosition;
            }
            var offset = CMouse.GlobalPosition - mouseGlobalPos;
            Camera.First.node.Position -= offset;
        }

      
    }
}
