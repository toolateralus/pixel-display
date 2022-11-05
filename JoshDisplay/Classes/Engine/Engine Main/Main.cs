
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml.Serialization;
using Bitmap = System.Drawing.Bitmap;

namespace pixel_renderer
{
    /// <summary>
    /// Main Entry-Point for App.
    /// </summary>
    public partial class EngineInstance : Window
    {
        // main entry point for application
        public EngineInstance()
        {
            InitializeComponent();
            Runtime.Awake(this);
        }
        // start / stop button on UI.
        public void Accept_Clicked(object sender, RoutedEventArgs e)
        {
            Runtime env = Runtime.Instance;
            if (env.running)
            {
                acceptButton.Background = Brushes.Black;
                acceptButton.Foreground = Brushes.Green;
                env.running = false;
                return;
            }
            acceptButton.Background = Brushes.White;
            acceptButton.Foreground = Brushes.OrangeRed;
            env.InitializeClocks(TimeSpan.FromSeconds(0.05f));
            env.running = true;
        }
        public void DebugUnchecked(object sender, RoutedEventArgs e)
        {
            Debug.debugging = false;
        }
        public void DebugChecked(object sender, RoutedEventArgs e)
        {
            Debug.debugging = true;
        }
        public string x = ""; 
       
    }
    public class Runtime
    {
        private static Runtime instance = new();
        public static Runtime Instance { get { return instance; } }
        public EngineInstance mainWnd;
        public Timer? physicsTimer;
        public Stage? stage;
        public List<Bitmap> Backgrounds = new List<Bitmap>();

        public long lastFrameTime = 0;
        public int BackroundIndex = 0;
        public int framesUntilCheck = 50;
        public int frameCount;

        public bool running;
        public string? ImageDirectory;

        private void Execute()
        {
            if (running)
            {
                Input.UpdateKeyboardState();
                if (Rendering.State == RenderState.Game) Rendering.Render(mainWnd.renderImage);
                if (Debug.debugging) Debug.Log(mainWnd.outputTextBox);
            }
        }
        private void GetFramerate()
        {
            if (framesUntilCheck >= Constants.frameRateCheckThresh)
            {
                lastFrameTime = DateTime.Now.Ticks;
                framesUntilCheck = 0;
                frameCount = 0;
            }
            framesUntilCheck++;
        }

        public void InitializeClocks(TimeSpan interval)
        {
            if (physicsTimer == null)
            {
                CompositionTarget.Rendering += Update;
                physicsTimer = new Timer(interval.TotalSeconds);
                physicsTimer.Elapsed += FixedUpdate;
                physicsTimer.Start();
                return;
            }
            if (!physicsTimer.Enabled)
            {
                physicsTimer.Start();
                return;
            }
            physicsTimer.Stop();
            return;

        }
        public void InitializeBitmapCollection()
        {
            if (ImageDirectory == null) return;
            foreach (string path in
                Directory.GetFiles(path: ImageDirectory)) Backgrounds.Add(new Bitmap(path));
        }
        public static void Awake(EngineInstance mainWnd)
        {
            instance.mainWnd = mainWnd;
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            instance.ImageDirectory = appdata + "\\Pixel\\Images";
            if (!Directory.Exists(instance.ImageDirectory))
            {
                Directory.CreateDirectory(instance.ImageDirectory);
            }
            instance.InitializeBitmapCollection();
            Staging.InitializeDefaultStage();
        }
        public void FixedUpdate(object? sender, EventArgs e)
        {
            if (stage == null) return; 
            Staging.UpdateCurrentStage(stage);
            var collisionMap = Collision.BroadPhase(stage);
            var narrowMap = Collision.NarrowPhase(collisionMap);
            Collision.Solve(narrowMap);
        }
        public void Update(object? sender, EventArgs e)
        {
            GetFramerate();
            Execute();
        }
    }
    public static class Rendering
    {
        /// <summary>
        /// Game = Build;
        /// Scene = Inspector;
        /// Off = Off; 
        /// Controlled and read externally, serves as a reference to what is currently being rendered; 
        /// </summary>
        public static RenderState State = RenderState.Game;
        public static Queue<Bitmap> FrameBuffer = new Queue<Bitmap>();
        public static double FrameRate()
        {
            Runtime env = Runtime.Instance;
            var lastFrameTime = env.lastFrameTime;
            var frameCount = env.frameCount;
            var frameRate =
                System.Math.Floor(1 /
                TimeSpan.FromTicks(DateTime.Now.Ticks - lastFrameTime).TotalSeconds
                * frameCount);
            return frameRate;
        }
        public static void Render(Image output)
        {
            var runtime = Runtime.Instance;
            var player = runtime.stage.FindNode("Player");
            var cam = player.GetComponent<Camera>();
            var frame = Draw(cam, (Bitmap)runtime.stage.Background.Clone());
            Insert(frame);
            var renderFrame = FrameBuffer.First();
            DrawToImage(ref renderFrame, output);
        }
        private static Bitmap Draw(Camera camera, Bitmap frame)
        {
            Stage stage = Runtime.Instance.stage;
            Vec2 camPos = camera.parentNode.position;
            foreach (var node in stage.Nodes)
            {
                var sprite = node.GetComponent<Sprite>();
                if (sprite is null) continue;

                for (int x = 0; x < sprite.size.x; x++)
                    for (int y = 0; y < sprite.size.y; y++)
                    {
                        var offsetX = node.position.x + x;
                        var offsetY = node.position.y + y;
                        if (offsetX < 0) continue;
                        if (offsetY < 0) continue;

                        if (offsetX >= Constants.screenWidth) continue;
                        if (offsetY >= Constants.screenHeight) continue;

                        var color = sprite.colorData[x, y];
                        var position = new Vec2((int)offsetX, (int)offsetY);

                        var pixelOffsetX = (int)position.x;
                        var pixelOffsetY = (int)position.y;

                        frame.SetPixel(pixelOffsetX, pixelOffsetY, color);
                    }
            }
            return frame;
        }
        private static void Insert(Bitmap inputFrame)
        {
            if (FrameBuffer.Count > 0) FrameBuffer.Dequeue();
            FrameBuffer.Enqueue(inputFrame);
        }
        private static void DrawToImage(ref Bitmap inputFrame, Image renderImage)
        {
            var bitmap = ConvertBitmapToBitmapImage.Convert(inputFrame);
            renderImage.Source = bitmap;
        }
    }
    [Obsolete]
    public static class Debug
    {
        // really sloppy quick implementation using the most wasteful string possible -- must be totally revised.
        // crashes on any stage containing more than 10 nodes XD
        [Obsolete("DO NOT USE ON STAGES CONTAINING MORE THAN 10 NODES! IT WILL CRASH!")]
        public static string debug = ""; // move to Runtime class
        [Obsolete("DO NOT USE ON STAGES CONTAINING MORE THAN 10 NODES! IT WILL CRASH!")]
        public static bool debugging;  // move to Runtime class
        // take a string, create label, populate label, insert to feed. 
        // auto sizing XML grid could create a chat feed look with expandable fields
        [Obsolete("DO NOT USE ON STAGES CONTAINING MORE THAN 10 NODES! IT WILL CRASH!")]
        public static void Log(TextBox outputTextBox)
        {
            var runtime = Runtime.Instance;
            Stage stage = runtime.stage;
            outputTextBox.Text =
            $" ===STATS===: \n\t {Rendering.FrameRate()} Frames Per Second \n PLAYER STATS : {stage.FindNode("Player").GetComponent<Rigidbody>().GetDebugs()}\t " +
            $"\n RB_DRAG :{stage.FindNode("Player").GetComponent<Rigidbody>().GetDrag()}" +
            $"\n\t Current Room : {runtime.BackroundIndex}";
            outputTextBox.Text +=
            "\n ===HIERARCHY===";
            outputTextBox.Text +=
            $"\n\t Stage : {stage.Name} (Loaded Nodes : {stage.Nodes.Count()}) \n";
            // NODE HEIRARCHY
            outputTextBox.Text += "\n\n";
            foreach (var node in stage.Nodes)
            {
                outputTextBox.Text +=
                $" \n\t Node : \n\t  Name  : {node.Name} \n\t\t Position : {node.position.x} , {node.position.y} ";

                if (node.TryGetComponent(out Sprite sprite))
                {
                    outputTextBox.Text +=
                    $"\n\t isSprite : {sprite} \n\t\t";
                }
                if (node.TryGetComponent(out Rigidbody rb))
                {
                    outputTextBox.Text +=
                    $" isRigidbody : {rb} \n\t Velocity : {rb.velocity.x} , {rb.velocity.y}\n ";
                }
            }
            outputTextBox.Text += $" \n {debug} \n";
        }
    }
    public static class Collision
    {
        public static SpatialHash hash = new(Constants.screenWidth, Constants.screenHeight, Constants.collisionCellSize);
                                                       
                                                      
        public static bool IsInside(this Node nodeA, Node nodeB)
        {
            Vec2 a = nodeA.position;
            Vec2 b = nodeB.position; 
            Vec2 spriteSizeA = nodeA.GetComponent<Sprite>().size;
            Vec2 spriteSizeB = nodeB.GetComponent<Sprite>().size;

            if (spriteSizeA != null && spriteSizeB != null)
            {
                // messy if for box collision; 
                if (a.x < b.x + spriteSizeB.y && 
                    a.x + spriteSizeA.x > b.x && 
                    a.y < b.y + spriteSizeB.y && 
                    spriteSizeA.y + a.y > b.y)
                    {
                    // Collision detected!
                        
                        return true; 
                    }
                // No collision
            }
            return false; 
           
        }
        public static List<List<Node>> BroadPhase(Stage stage)
        {
            List<List<Node>> collisionMap = new(); 
            foreach (var node in stage)
            {
                if (!node.TryGetComponent(out Sprite sprite) || !sprite.isCollider)
                {
                    continue; 
                }
                hash.RegisterObject(node);
            }
            foreach (var node in stage)
            {
               List<Node> result = hash.GetNearby(node);
               collisionMap.Add(result);
            }
            hash.ClearBuckets();
            return collisionMap; 
        }
        public static Dictionary<Node, Node> NarrowPhase(List<List<Node>> collisionMap)
        {
            var nodes = new Dictionary<Node, Node>();
            foreach (var cells in collisionMap)
            {
                if(cells.Count <= 0) continue;
                foreach (var nodeA in cells)
                {
                    foreach (var nodeB in cells)
                    {
                        if (nodeA.UUID.Equals(nodeB.UUID)) continue; 
                        if (nodeA.IsInside(nodeB))
                        {
                            // continue or remove and proceed?
                            if   (nodes.ContainsKey(nodeA)) nodes.Remove(nodeA);
                            if   (nodes.ContainsKey(nodeB)) nodes.Remove(nodeB);
                            if (nodes.ContainsValue(nodeA)) nodes.Remove(nodeA);
                            if (nodes.ContainsValue(nodeB)) nodes.Remove(nodeB);
                            nodes.Add(nodeA, nodeB);
                        }
                    }
                }
            }
            return nodes; 
        }
        public static void ViewportCollision(Node node)
        {
            Sprite sprite = node.GetComponent<Sprite>();
            Rigidbody rb = node.GetComponent<Rigidbody>();
            if (sprite is null || rb is null) return;
            if (sprite.isCollider)
            {
                if (node.position.y > Constants.screenHeight - 4 - sprite.size.y)
                {
                    node.position.y = Constants.screenHeight - 4 - sprite.size.y;
                }
                if (node.position.x > Constants.screenWidth - sprite.size.x)
                {
                    node.position.x = Constants.screenWidth - sprite.size.x;
                    rb.velocity.x = 0;
                }
                if (node.position.x < 0)
                {
                    node.position.x = 0;
                    rb.velocity.x = 0;
                }
            }
        }
        internal static void Solve(Dictionary<Node, Node> narrowMap)
        {
            foreach (KeyValuePair<Node, Node> colliders in narrowMap)
            {
                Node a = colliders.Key;
                Node b = colliders.Value; 
                
                Sprite spriteA = a.GetComponent<Sprite>(); 
                Sprite spriteB = b.GetComponent<Sprite>();

                Rigidbody rbA = a.GetComponent<Rigidbody>();
                Rigidbody rbB = b.GetComponent<Rigidbody>();
                Rigidbody submissive = null;
                Rigidbody dominant = null;
                Vec2 sizeA = spriteA.size;
                Vec2 sizeB = spriteB.size;

                Vec2 posA = a.position;
                Vec2 posB = b.position;

                var distance = (posA + sizeA - posB + sizeB); 
                if (rbA.velocity.Length > rbB.velocity.Length)
                {
                    dominant = rbB; 
                    submissive = rbA;
                }
                else
                {
                    dominant = rbA; 
                    submissive = rbB;
                }
                var depenetrationForce = 0f;
                if (dominant.velocity.Length < 1)
                {
                    depenetrationForce = Constants.depenetrationForce;
                }
                else depenetrationForce = Constants.depenetrationForce + dominant.velocity.Length; 
                submissive.velocity = CMath.Negate(dominant.velocity); 
            }
            hash = new(Constants.screenWidth, Constants.screenHeight, Constants.collisionCellSize);
        }
    }
    public static class Staging
    {
        private const int maxClickDistance_InPixels = 0;
        static Runtime Env = Runtime.Instance;
        public static void SetCurrentStage(Stage stage) => Env.stage = stage;
        public static void UpdateCurrentStage(Stage stage)
        {
            stage.RefreshStageDictionary();
            stage.FixedUpdate(delta: Env.lastFrameTime);
            if (Debug.debugging) Debug.debug = "";
            Env.frameCount++;
        }
        public static void InitializeDefaultStage()
        {
            var nodes = new List<Node>();
            AddPlayer(nodes);
            for (int i = 0; i < 25; i++)
            {
                AddDefaultNodes(nodes, i);
            }
            SetCurrentStage(new Stage("Default Stage", Env.Backgrounds[0], nodes.ToArray()));
            InitializeNodes();
            Env.stage.RefreshStageDictionary();
        }
        private static void InitializeNodes()
        {
            foreach (Node node in Env.stage.Nodes)
            {
                node.parentStage = Env.stage;
                foreach (var component in node.Components)
                {
                    component.Value.Awake();
                }
            }
        }
        private static void AddDefaultNodes(List<Node> nodes, int i)
        {
            var pos = JRandom.ScreenPosition();
            var node = new Node($"NODE {i}", new Vec2(pos.x, pos.y), new Vec2(0, 1));
            var position = Vec2.one * JRandom.Int(1, 3);
            //var wind = new Wind(Direction.Left);
            //node.AddComponent(wind);
            node.AddComponent(new Sprite(position, JRandom.Color(), true));
            node.AddComponent(new Rigidbody());
            nodes.Add(node);
        }
        public static Node lastSelected;
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
                Point pt = (Point)node.position;
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
            Vec2 playerStartPosition = new Vec2(3, 8);
            Node playerNode = new("Player", playerStartPosition, Vec2.one);

            Rigidbody rb = new();
            Sprite sprite = new(Vec2.one * 14, JRandom.Color(), true);

            Camera cam = new();

            Player player_obj = new()
            {
                takingInput = true
            };

            playerNode.AddComponent(player_obj);
            playerNode.AddComponent(rb);
            playerNode.AddComponent(sprite);
            playerNode.AddComponent(cam);
            nodes.Add(playerNode);
        }
    }

    public class SpatialHash
    {
        int rows;
        int columns;
        int cellSize;
        List<List<Node>> Buckets = new List<List<Node>>();

        public SpatialHash(int screenWidth, int screenHeight, int cellSize)
        {
            Buckets = new List<List<Node>>(); 
            rows = screenHeight / cellSize;
            columns = screenWidth / cellSize;
            this.cellSize = cellSize;
            for (int i = 0; i < columns * rows; i++)
            {
                Buckets.Add(new List<Node>());
            }
        }
        internal void ClearBuckets()
        {
            for (int i = 0; i < columns * rows; i++)
            {
                Buckets[i].Clear();
            }
        }
        internal void RegisterObject(Node obj)
        {
            List<int> cells = Hash(obj);
            foreach (var index in cells)
            {
                if (index < 0) continue;
                Buckets[index].Add(obj);
            }
        }
        internal List<Node> GetNearby(Node node)
        {
            List<Node> nodes = new List<Node>();
            List<int> buckets = Hash(node);
            foreach (var index in buckets)
            {
                if (index < 0) continue;
                nodes.AddRange(Buckets[index]);
            }
            return nodes;
        }
        private void AddBucket(Vec2 vector, float width, List<int> bucket)
        {
            int cellPosition = (int)(
                       (Math.Floor(vector.x / cellSize)) +
                       (Math.Floor(vector.y / cellSize)) *
                       width
            );

            if (!bucket.Contains(cellPosition))
                bucket.Add(cellPosition);

        }
        private List<int> Hash(Node obj)
        {
            Sprite sprite = obj.GetComponent<Sprite>(); 
            List<int> bucketsObjIsIn = new List<int>();
            Vec2 min = new Vec2(
                obj.position.x - sprite.size.x,
                obj.position.y - sprite.size.y);
            Vec2 max = new Vec2(
                obj.position.x + sprite.size.x,
                obj.position.y + sprite.size.y);
            float width = Constants.screenWidth / cellSize;
            
            //TopLeft
            AddBucket(min, width, bucketsObjIsIn);
            //TopRight
            AddBucket(new Vec2(max.x, min.y), width, bucketsObjIsIn);
            //BottomRight
            AddBucket(new Vec2(max.x, min.y), width, bucketsObjIsIn);
            //BottomLeft
            AddBucket(new Vec2(max.x, min.y), width, bucketsObjIsIn);

            return bucketsObjIsIn;

        }
    }
}