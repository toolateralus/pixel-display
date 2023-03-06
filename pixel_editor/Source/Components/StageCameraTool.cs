using pixel_renderer;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Windows.Input;
using System.Numerics;

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
            CMouse.OnMouseMove += TryMoveCamera;
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
            
            IEnumerable<Camera> cams = Runtime.Current.GetStage().GetAllComponents<Camera>();
            
            if (!cams.Any())
                return;

            cams.First().node.Position = node.Position;
            
            if (!Input.Get(StopFollowingNodeKey))
                return;

            followNode = true;
        }

        private void TryFocusNode()
        {
            if (selected == null)
                return;

            IEnumerable<Camera> cams = Runtime.Current.GetStage().GetAllComponents<Camera>().AsParallel();

            lock(cams)
            if (!cams.Any()) return;

            bool selectNode = Input.Get(FocusNodeKey);

            if (!selectNode)
                return;

            cams.First().node.Position = selected.Position;

            if (!Input.Get(FollowModifier))
                return;

            followNode = false; 
        }
        public static void TryFocusNode(Node node)
        {
            if (node == null)
                return;
            
            IEnumerable<Camera> cams = Runtime.Current.GetStage().GetAllComponents<Camera>();

            if (!cams.Any()) 
                return;

            bool selectNode = Input.Get(Key.LeftShift) && Input.Get(Key.Space);

            if (!selectNode)
                return;

            cams.First().node.Position = node.Position;

            if (!Input.Get(FollowModifier))
                return;

            followNode = false; 
        }

        private void TryZoomCamera()
        {
            if (CMouse.MouseWheelDelta == 0)
                return;

            IEnumerable<Camera> enumerable = Runtime.Current.GetStage().GetAllComponents<Camera>().AsParallel();
            if (!enumerable.Any()) return;
            enumerable.First().Size *= MathF.Pow(Constants.MouseZoomSensitivityFactor, -CMouse.MouseWheelDelta);
        }

        private void TryMoveCamera()
        {
            IEnumerable<Camera> cams = Runtime.Current.GetStage().GetAllComponents<Camera>().AsParallel();

            if (!cams.Any())
                return;

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
            cams.First().node.Position -= offset;
        }

      
    }
}
