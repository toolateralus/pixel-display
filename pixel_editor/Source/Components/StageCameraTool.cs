using pixel_renderer;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Windows.Input;

namespace pixel_editor
{
    public enum StageCameraState : byte { Following, Idle, Inactive};

    public class StageCameraTool : Tool
    {
        private const Key FollowNodeKey = Key.LeftCtrl;
        private const Key StopFollowingNodeKey = Key.Escape;
        public Camera camera;
        public Node? selected;
        private static bool followNode;

        public static StageCameraState State { get; private set;}
        
        public override void Awake()
        {

        }

        public override void Update(float delta)
        {
            selected = Editor.Current.LastSelected;
            TryFollowNode(selected);
            TryFocusNode();
            TryZoomCamera();
            TryMoveCamera();
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

            if (!cams.Any()) return;

            bool selectNode = Input.Get(Key.LeftShift) && Input.Get(Key.Space);
            if (!selectNode)
                return;

            cams.First().node.Position = selected.Position;

            if (!Input.Get(FollowNodeKey))
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

            if (!Input.Get(FollowNodeKey))
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

            if (CMouse.Right)
            {
                State = StageCameraState.Idle; 
                cams.First().node.Position += CMouse.Delta * Constants.MouseSensitivity;
            }

        }

      
    }
}
