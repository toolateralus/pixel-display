
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

                if (node.GetComponent<Sprite>() is not Sprite sprite)
                    continue;

                BoundingBox2D box = new(sprite.GetCorners());
                if (!clickPosition.IsWithin(box.min, box.max))
                    continue;

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
            Runtime.Current.renderHost.info.frameCount++;
            var delta = Runtime.Current.renderHost.info.FrameTime;
            stage.FixedUpdate((float)delta);
        }
        public static void Update(Stage m_stage)
        {
            m_stage.Update();
        }
    }
}