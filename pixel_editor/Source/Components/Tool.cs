using pixel_renderer;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Windows.Input;

namespace pixel_editor
{
    public abstract class Tool
    {
        public abstract void Awake();
        public abstract void Update(float delta);
    }
    public class DragTool : Tool
    {
        public Node? selected;
        private Vec2 mouseSelectedNodeOffset;

        public override void Awake()
        {
            CMouse.OnLeftPressedThisFrame += SetSelectedNodeOffest;
        }
        private void SetSelectedNodeOffest()
        {
            if (selected == null)
                return;
            mouseSelectedNodeOffset = selected.Position - CMouse.GlobalPosition;
        }
        public override void Update(float delta)
        {
            selected = Editor.Current.selected; 
            if (CMouse.Left && selected != null)
                selected.Position = CMouse.GlobalPosition + mouseSelectedNodeOffset;
        }
    }
    public class StageCamera : Tool
    {
        public Camera camera;
        public Node? selected; 
        private bool followNode;
        
        public override void Awake()
        {

        }

        public override void Update(float delta)
        {
            selected = Editor.Current.selectedNode;
            TryFollowNode();
            TryFocusNode();
            TryZoomCamera();
            TryMoveCamera();
        }

        private void TryFollowNode()
        {
            if (selected is null) return;
            IEnumerable<Camera> cams = Runtime.Current.GetStage().GetAllComponents<Camera>();
            if (!cams.Any()) return;
            cams.First().parent.Position = selected.Position;
            if (!Input.GetInputValue(Key.Escape))
                return;
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
