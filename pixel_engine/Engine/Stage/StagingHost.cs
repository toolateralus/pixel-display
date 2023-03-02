
using System.Numerics;
using Point = System.Windows.Point;

namespace pixel_renderer
{
    public class StagingHost
    {
        public Node? lastSelected;
        /// <summary>
        /// this is used for editor clicking.
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="clickPosition"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool GetNodeAtPoint(Stage stage, Vector2 clickPosition, out Node? result)
        {
            foreach (var node in stage.nodes)
            {
                if (node == lastSelected) continue;

                bool hasSprite = !node.TryGetComponent(out Sprite sprite);
                if (hasSprite) continue;

                bool isWithin = clickPosition.IsWithin(node.Position, node.Position + sprite.size);
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
            lastSelected = null;
            if (x is not null)
            x.selected_by_editor = false;
        }
        public static void FixedUpdate(Stage stage)
        {
            var delta = Runtime.Current.renderHost.info.FrameTime;
            Runtime.Current.renderHost.info.frameCount++;
            stage.FixedUpdate(delta);
        }
        public static void Update(Stage m_stage)
        {
            m_stage.Update();
        }
    }
}