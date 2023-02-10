
using Point = System.Windows.Point;

namespace pixel_renderer
{
    public class StagingHost
    {
        public Node? lastSelected;
        static Runtime runtime => Runtime.Instance;
        /// <summary>
        /// this is used for editor clicking.
        /// </summary>
        /// <param name="stage"></param>
        /// <param name="clickPosition"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool GetNodeAtPoint(Stage stage, Point clickPosition, out Node? result)
        {
            foreach (var node in stage.Nodes)
            {
                if (node == lastSelected) continue;

                bool hasSprite = !node.TryGetComponent(out Sprite sprite);
                if (hasSprite) continue;

                bool isWithin = ((Vec2)clickPosition).IsWithin(node.position, node.position + sprite.size);
                if (!isWithin) continue;

                result = node;
                
                sprite.Highlight(Constants.EditorHighlightColor);
                
                lastSelected?.GetComponent<Sprite>().RestoreCachedColor(false);
                lastSelected = node;

                return true;
            }
            result = null;
            return false;
        }
      
        public static void Update(Stage stage)
        {
            var delta = runtime.renderHost.info.FrameTime;
            stage.FixedUpdate(delta);
            runtime.renderHost.info.frameCount++;
        }
    }
}