using pixel_renderer.Assets;
using pixel_renderer.IO;
using System;
using System.Collections.Generic;
using Color = System.Drawing.Color; 
using Point = System.Windows.Point;

namespace pixel_renderer
{
    public class StagingHost
    {
        public Node? lastSelected;
        static Runtime runtime => Runtime.Instance;
        
        public bool GetNodeAtPoint(Stage stage, Point pos, out Node? result)
        {
            pos = new Point()
            {
                X = Math.Round(pos.X),
                Y = Math.Round(pos.Y)
            };
            foreach (var node in stage.Nodes)
            {
                // round up number to improve click accuracy
                Point pt = node.position;
                pt = new()
                {
                    X = Math.Round(pt.X),
                    Y = Math.Round(pt.Y)
                };

                var xDelta = pt.X - pos.Y;
                var yDelta = pt.Y - pos.X;

                if (xDelta < 0) xDelta = CMath.Negate(xDelta);
                if (yDelta < 0) yDelta = CMath.Negate(yDelta);

                if (xDelta > Constants.maxClickDistanceInPixels) continue;
                if (yDelta > Constants.maxClickDistanceInPixels) continue;

                if (node == lastSelected) continue;
                result = node;
                
                if (lastSelected != null)
                {
                    if (lastSelected.TryGetComponent(out Sprite sprite))
                        sprite.RestoreCachedColor(false);
                }
                lastSelected = node;
                if (node.TryGetComponent(out Sprite sprite_))
                {
                    sprite_.Highlight(Color.Black);
                }
                return true;
            }
            result = null;
            return false;
        }

        public static Stage Default()
        {
            var nodes = new List<Node>();
            
            AddPlayer(nodes);

            var bitmap = Constants.AppDataDir + "\\Pixel\\Images" + "\\home";
            var backgroundMeta = new Metadata("Bitmap Metadata", bitmap, ".bmp");
            
            var stage = new Stage("Default Stage", backgroundMeta, nodes.ToNodeAssets());

            for (int i = 0; i < 10; i++) 
                stage.create_generic_node();

            return stage;
        }
        public static void Update(Stage stage)
        {
            var delta = runtime.renderHost.info.lastFrameTime;
            stage.FixedUpdate(delta);
            runtime.renderHost.info.frameCount++;
        }
        public static void ReloadCurrentStage()
        {
            var reset = runtime.GetStageAsset();
            if (reset is null)
                throw new NullStageException("Resetting stage failed");

            if (runtime.renderHost.State is not RenderState.Off)
                runtime.Toggle();

            runtime.SetStageAsset(reset);
        }
        public static void AddPlayer(List<Node> nodes)
        {
            Vec2 playerStartPosition = new Vec2(12, 24);
            Node playerNode = new("Player", playerStartPosition, Vec2.one);
            Rigidbody rb = new()
            {
                IsTrigger = false,
            };
            Sprite sprite = new(Vec2.one * 4, JRandom.Color(), true);
            Player player_obj = new()
            {
                takingInput = true
            };

            playerNode.AddComponent(rb);
            playerNode.AddComponent(player_obj);
            playerNode.AddComponent(sprite);
            
            nodes.Add(playerNode);
        }
    }
}