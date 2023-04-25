using pixel_renderer;
using System;
using System.Windows.Input;

namespace pixel_editor
{
    public class ClickTool : Tool
    {
        public override void Awake()
        {
            Editor.Current.image.MouseLeftButtonDown += Click;
        }

        private void Click(object sender, MouseEventArgs args)
        {
            TryClickNodeOnScreen(out var x);
            Editor.Current.LastSelected = x;
        }

        internal static bool TryClickNodeOnScreen(out Node? result)
        {
            Editor.Current.Inspector?.DeselectNode();
            result = null;
            Stage stage = Runtime.Current.GetStage();

            if (stage is null)
                return false;

            StagingHost stagingHost = Runtime.Current.stagingHost;

            if (stagingHost is null)
                return false;

            bool foundNode = stagingHost.GetNodeAtPoint(stage, CMouse.GlobalPosition, out var node);

            if (foundNode)
                Editor.Current.Inspector?.SelectNode(node);

            result = node;
            return foundNode;
        }
        public override void Update(float delta)
        {
        }
    }
}
