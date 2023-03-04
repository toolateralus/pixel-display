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
        public static StageCameraState State { get; private set;}
        
        public override void Awake()
        {

        }

        public override void Update(float delta)
        {
            selected = Editor.Current.LastSelected;
            TryFollowNode();
            TryFocusNode();
            TryZoomCamera();
            TryMoveCamera();
        }

        private void TryFollowNode()
        {
            if (selected is null || State == StageCameraState.Idle)
                return;
            IEnumerable<Camera> cams = Runtime.Current.GetStage().GetAllComponents<Camera>();
            if (!cams.Any())
                return;
            cams.First().node.Position = selected.Position;
            if (!Input.Get(StopFollowingNodeKey))
                return;
            State = 0;
        }

        private void TryFocusNode()
        {
            if (selected == null)
                return;
            IEnumerable<Camera> cams = Runtime.Current.GetStage().GetAllComponents<Camera>();
            if (!cams.Any()) return;

            bool selectNode = Input.Get(Key.LeftShift) && Input.Get(Key.Space);
            if (!selectNode)
                return;

            cams.First().node.Position = selected.Position;

            if (!Input.Get(FollowNodeKey))
                return;
            State = 0; 
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

            State = 0; 
        }

        private void TryZoomCamera()
        {
            if (CMouse.MouseWheelDelta == 0)
                return;

            IEnumerable<Camera> enumerable = Runtime.Current.GetStage().GetAllComponents<Camera>();
            if (!enumerable.Any()) return;
            enumerable.First().Size *= MathF.Pow(Constants.MouseZoomSensitivityFactor, -CMouse.MouseWheelDelta);
        }

        private void TryMoveCamera()
        {
            IEnumerable<Camera> cams = Runtime.Current.GetStage().GetAllComponents<Camera>();

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
