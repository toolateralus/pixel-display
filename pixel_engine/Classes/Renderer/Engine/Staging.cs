using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Point = System.Windows.Point;

namespace pixel_renderer
{
    public abstract class InspectorEvent 
    {
        public string message;
        public object sender;
        public Action<object?, object?, object?, object?> expression = (object? arg1, object? arg2 , object? arg3, object? arg4) => { };
        public object[] expressionArgs = new object[3];
    }
  
    public static class Staging
    {
        private const int maxClickDistance_InPixels = 0;
        static Runtime runtime => Runtime.Instance;
        // this variable is used by the inspector to
        // ensure user'a click grabs a new node each time 
        public static Node lastSelected;
        public static void SetCurrentStage(Stage stage) => runtime.stage = stage;
        public static void UpdateCurrentStage(Stage stage)
        {
            stage.FixedUpdate(delta: runtime.lastFrameTime);
            runtime.frameCount++;
        }
        public static void InitializeDefaultStage()
        {
            var nodes = new List<Node>(); 
            InitializeGenericNodes(nodes);
            Bitmap background = new(256, 256);
            SetCurrentStage(new Stage("Default Stage", background, nodes.ToArray()));
        }
        private static void InitializeGenericNodes(List<Node> nodes)
        {
            AddPlayer(nodes);
            AddFloor(nodes);
            for (int i = 0; i < 1000; i++) CreateGenericNode(nodes, i);
        }
        private static void AddFloor(List<Node> nodes)
        {
            var staticNodes = new List<Node>(); 
           for (int i = 0; i < 240; i++)
               CreateGenericNode(staticNodes, i);
            foreach (var node in staticNodes)
            {
                var randomDrag = JRandom.Bool() ? 0 : 1;
                var x = node.GetComponent<Rigidbody>();
                x.usingGravity = JRandom.Bool();
                x.drag = randomDrag;

            }


            nodes.AddRange(staticNodes);

        }
        public static void CreateGenericNode(List<Node> nodes, int i)
        {
            var pos = JRandom.ScreenPosition();
            var five = Vec2.one * 5;
            var node = new Node($"NODE {i}", pos, Vec2.one);
            node.AddComponent(new Sprite(five, JRandom.Color(), true));
            node.AddComponent(new Rigidbody()
            {
                IsTrigger = false,
                usingGravity = true,
                drag = .1f
            });
            var randomDirection = JRandom.Direction(); 
            node.AddComponent(new Wind(randomDirection));
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
                    if (lastSelected != null)
                    {
                        if (lastSelected.TryGetComponent(out Sprite sprite))
                        {
                            sprite.RestoreCachedColor(false);
                        }
                    }
                    lastSelected = node;
                    if (node.TryGetComponent(out Sprite sprite_))
                    {
                        sprite_.Highlight(Color.White); 
                    }
                    return true;
                }
            }
            result = null;
            return false;
        }
        public static void AddPlayer(List<Node> nodes)
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

         
          
        }
    }
  
}