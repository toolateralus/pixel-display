using pixel_renderer.Assets;
using pixel_renderer.FileIO;
using pixel_renderer.Scripts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Color = System.Drawing.Color; 
using Point = System.Windows.Point;

namespace pixel_renderer
{
    public class StagingHost
    {
        public Node? lastSelected;
        static Runtime runtime => Runtime.Instance;
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

        public static Stage Default()
        {
            var nodes = new List<Node>();
            Player.AddPlayer(nodes);

            var bitmap = Constants.WorkingRoot + Constants.ImagesDir + "\\home.bmp";
            var backgroundMeta = new Metadata("Bitmap Metadata", bitmap, ".bmp");
            var stage = new Stage("Default Stage", backgroundMeta, nodes.ToNodeAssets());

            for (int i = 0; i < 10; i++) 
                stage.create_generic_node();

            return stage;
        }
        public static void Update(Stage stage)
        {
            var delta = runtime.renderHost.info.FrameTime;
            stage.FixedUpdate(delta);
            runtime.renderHost.info.frameCount++;
        }
        public static void ReloadCurrentStage()
        {
            var reset = runtime.GetStage();

            if (reset is null)
                throw new NullStageException("Resetting stage failed");

            if (runtime.renderHost.State is not RenderState.Off)
                runtime.Toggle();

            runtime.SetStage(reset);
        }
    }
}