
using System;
using Point = System.Windows.Point;

namespace pixel_renderer
{
    public class StagingHost
    {
        public Node? lastSelected;

        static Runtime runtime => Runtime.Current;
        /// <summary>
        /// this is used for editor clicking.
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="clickPosition"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool GetNodeAtPoint(Stage stage, Point clickPosition, out Node? result)
        {
            foreach (var node in stage.nodes)
            {
                if (node == lastSelected) continue;

                bool hasSprite = !node.TryGetComponent(out Sprite sprite);
                if (hasSprite) continue;

                bool isWithin = ((Vec2)clickPosition).IsWithin(node.Position, node.Position + sprite.size);
                if (!isWithin) continue;

                result = node;

                SelectNode(sprite);

                DeselectNode();

                lastSelected = node;

                return true;
            }
            result = null;
            return false;
        }

        private static void SelectNode(Sprite sprite) => sprite.selected_by_editor = true;

        public void DeselectNode()
        {
            if (lastSelected is null) return;
            var x = lastSelected.GetComponent<Sprite>();
            x.selected_by_editor = false;
            lastSelected = null;
        }

        public static void FixedUpdate(Stage stage)
        {
            var delta = runtime.renderHost.info.FrameTime;
            runtime.renderHost.info.frameCount++;
            stage.FixedUpdate(delta);
        }

        public static void Update(Stage m_stage)
        {
            m_stage.Update();
        }
    }
}