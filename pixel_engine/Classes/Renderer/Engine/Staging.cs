namespace pixel_renderer
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Security;
    using System.Windows;
    using System.Xaml;
    using Point = System.Windows.Point;

    public static class Staging
    {
        private const int maxClickDistance_InPixels = 0;
        static Runtime runtime => Runtime.Instance;
        public static Node lastSelected;

        public static void SetCurrentStage(Stage stage) => runtime.stage = stage;
        public static void UpdateCurrentStage(Stage stage)
        {
            stage.RefreshStageDictionary();
            stage.FixedUpdate(delta: runtime.lastFrameTime);
            if (Debug.debugging) Debug.debug = "";
            runtime.frameCount++;
        }
        public static void InitializeDefaultStage()
        {
            List<Node> nodes = new List<Node>();

            AddPlayer(nodes);
            //AddFloor(nodes);

            for (int i = 0; i < 100; i++)
            {
                AddNode(nodes, i);
            }

            Bitmap background = GetFallbackBackground();
            if (runtime.Backgrounds.Count >= 0)
            {
                background = runtime.Backgrounds[0];
            }

            SetCurrentStage(new Stage("Default Stage", background, nodes.ToArray()));
            InitializeNodes();
            runtime.stage.RefreshStageDictionary();
        }

        private static Bitmap GetFallbackBackground()
        {
            Bitmap FallbackBitmap = new(256, 256);
            for (int i = 0; i < 256; i++)
                for (int j = 0; j < 256; j++)
                    FallbackBitmap.SetPixel(i, j, JRandom.Color());
            return FallbackBitmap;
        }

        private static void AddFloor(List<Node> nodes)
        {
            Vec2 startPos = new(2, Constants.ScreenWidth - 16);
            Node floor = new("Floor", startPos, Vec2.one);
            Floor floorScript = new(); 
            Sprite floorSprite = 
                new(new Vec2(Constants.ScreenHeight - 4, 10),
                System.Drawing.Color.FromArgb(255, 145, 210, 75),
                true);

            Rigidbody floorRb = new()
            {
                IsTrigger = false, 
                usingGravity = true,
                drag = 0f,
                Name = "Floor - Rigidbody"
            };
            floor.AddComponent(floorRb);
            floor.AddComponent(floorSprite);
            floor.AddComponent(floorScript);
            nodes.Add(floor);
        }

        private static void InitializeNodes()
        {
            foreach (Node node in runtime.stage.Nodes)
            {
                node.parentStage = runtime.stage;
                node.Awake(); 
            }
        }
        private static void AddNode(List<Node> nodes, int i)
        {
            var pos = JRandom.ScreenPosition();
            var node = new Node($"NODE {i}", new Vec2(pos.x, pos.y), new Vec2(0, 1));
            var position = Vec2.one * JRandom.Int(1, 3);
            node.AddComponent(new Sprite(position, JRandom.Color(), true));
            node.AddComponent(new Rigidbody()
            {
                IsTrigger = false,
                usingGravity = true,
                drag = .1f
            });
            nodes.Add(node);
        }
        public static bool TryCheckOccupant(Point pos, out Node? result)
        {
            // round up number to improve click accuracy
            // todo = consider size of sprite to reliably get 
            // clicks that arent exactly on the corner of the object
            // does not really work

            Stage stage = Runtime.Instance.stage ?? Stage.Empty;
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
                // (200 == 250) == true;
                var xDelta = pt.X - pos.X;
                var yDelta = pt.Y - pos.Y;

                if (xDelta + yDelta < maxClickDistance_InPixels)
                {
                    if (node == lastSelected) continue;
                    result = node;
                    lastSelected = node;
                    return true;
                }
            }
            result = null;
            return false;
        }
        private static void AddPlayer(List<Node> nodes)
        {
            Vec2 playerStartPosition = new Vec2(12, 24);
            Node playerNode = new("Player", playerStartPosition, Vec2.one);
            
            Rigidbody rb = new()
            {
                IsTrigger = false,

            };
            Rigidbody trigger = new()
            {
                IsTrigger = true
            };
            Sprite sprite = new(Vec2.one, JRandom.Color(), true);

            Camera cam = new();

            Player player_obj = new()
            {
                takingInput = true
            };
            Text text = new() 
            {
                
            };
            playerNode.AddComponent(text);
            playerNode.AddComponent(rb);
            playerNode.AddComponent(trigger);
            playerNode.AddComponent(player_obj);
            playerNode.AddComponent(sprite);
            playerNode.AddComponent(cam);
            playerNode.AddComponent(cam);
            nodes.Add(playerNode);

            // create asset of type script because the player is an example of a
            // user-created script; 
          
        }
    }

   
}