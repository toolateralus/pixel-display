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
            // instead of refreshing the whole stage hierarchy each frame, we could just add events that queue an update, even of just that member, but
            // probably all of them since the hierarchy is a relationship
            stage.RefreshStageDictionary();
            stage.FixedUpdate(delta: runtime.lastFrameTime);
            if (Debug.debugging) Debug.debug = "";
            runtime.frameCount++;
        }
        public static void InitializeDefaultStage()
        {
            List<Node> nodes = new List<Node>();

            AddPlayer(nodes);
            AddFloor(nodes);

            for (int i = 0; i < 100; i++)
            {
                CreateGenericNode(nodes, i);
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
            Vec2 pos = new(2, Constants.ScreenWidth - 4);
            Node node = new("Floor", pos, Vec2.one);
            Floor floor = new(); 
            Sprite sprite = new( new(Constants.ScreenWidth - 20, 10), JRandom.Color(), true);
                   
                
            Rigidbody rb = new()
            {
                IsTrigger = false, 
                usingGravity = true,
                drag = 0f,
                Name = "Floor Rigidbody"
            };

            node.AddComponent(rb);

            node.AddComponent(sprite);

            node.AddComponent(floor);

            nodes.Add(node);
        }

        private static void InitializeNodes()
        {
            foreach (Node node in runtime.stage.Nodes)
            {
                node.parentStage = runtime.stage;
                node.Awake(); 
            }
        }
        private static void CreateGenericNode(List<Node> nodes, int i)
        {
            var pos = JRandom.ScreenPosition();
            var randomScale = Vec2.one * 5;
            var node = new Node($"NODE {i}", pos, Vec2.one);
            node.AddComponent(new Sprite(randomScale, JRandom.Color(), true));
            node.AddComponent(new Rigidbody()
            {
                IsTrigger = false,
                usingGravity = true,
                drag = .1f
            });
            //var randomDirection = JRandom.Direction(); 
            //node.AddComponent(new Wind(randomDirection));
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
            Sprite sprite = new(Vec2.one, JRandom.Color(), true);
            Player player_obj = new()
            {
                takingInput = true
            };
            Text text = new(); 
           

            playerNode.AddComponent(text);
            playerNode.AddComponent(rb);
            playerNode.AddComponent(player_obj);
            playerNode.AddComponent(sprite);
            nodes.Add(playerNode);

            // create asset of type script because the player is an example of a
            // user-created script; 
          
        }
    }

   
}