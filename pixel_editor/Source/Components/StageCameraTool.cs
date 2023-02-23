using pixel_renderer;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Windows.Input;

namespace pixel_editor
{
    public class StageCameraTool : Tool
    {
        public Camera camera;
        public Node? selected; 
        private bool followNode = false;
        
        public override void Awake()
        {
        }

        public override void Update(float delta)
        {
            selected = Editor.Current.Selected;
            TryFollowNode();
            TryFocusNode();
            TryZoomCamera();
            TryMoveCamera();
        }

        private void TryFollowNode()
        {
            if (selected is null || !followNode)
                return;
            IEnumerable<Camera> cams = Runtime.Current.GetStage().GetAllComponents<Camera>();
            if (!cams.Any())
                return;
            cams.First().parent.Position = selected.Position;
            if (!Input.GetInputValue(Key.Escape))
                return;
            followNode = false;
        }

        private void TryFocusNode()
        {
            if (selected == null)
                return;
            IEnumerable<Camera> cams = Runtime.Current.GetStage().GetAllComponents<Camera>();
            if (!cams.Any()) return;
            if (!Input.GetInputValue(Key.F))
                return;
            cams.First().parent.Position = selected.Position;
            if (!Input.GetInputValue(Key.LeftShift))
                return;
            followNode = true;
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
                followNode = false;
                cams.First().parent.Position += CMouse.Delta * Constants.MouseSensitivity;
            }

        }

      
    }
}
