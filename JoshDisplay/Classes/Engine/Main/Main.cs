namespace pixel_renderer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Timers;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using Bitmap = System.Drawing.Bitmap;

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
        List<List<Node>> collisionMap = new();
        Dictionary<Node, Node> narrowMap = new(); 
        public void FixedUpdate(object? sender, EventArgs e)
        {
            if (stage == null) return; 
            Collision.BroadPhase(stage, collisionMap);
            Collision.NarrowPhase(collisionMap, narrowMap);
            Collision.GetCollision(narrowMap);
            Staging.UpdateCurrentStage(stage);
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
                Math.Floor(1 /
                TimeSpan.FromTicks(DateTime.Now.Ticks - lastFrameTime).TotalSeconds
                * frameCount);
            return frameRate;
        }
        static Runtime runtime => Runtime.Instance;
        public static void Render(Image output)
        {
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
        public static bool CheckOverlap(this Node nodeA, Node nodeB)
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
                    return true; 
            }
            return false; 
           
        }
        public static void BroadPhase(Stage stage, List<List<Node>> broadMap)
        {
            broadMap.Clear(); 
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
               broadMap.Add(result);
            }
            hash.ClearBuckets();
        }
        public static Dictionary<Node, Node> NarrowPhase(List<List<Node>> collisionMap, Dictionary<Node, Node> narrowMap)
        {
            narrowMap.Clear();

            if (collisionMap.Count <= 0 || collisionMap[0] == null) 
                return narrowMap; 
            
            for(int i = 0; i < collisionMap.Count(); i++)
            {
                var cell = collisionMap[i];
                if(cell.Count <= 0) continue;

                for (int j = 0; j < cell.Count; j++)
                {
                    var nodeA = cell[j];
                    if (nodeA is null) continue; 

                    for (int k = 0; k < cell.Count; k++)
                    {
                        var nodeB = cell[k];
                        if (nodeB is null) continue;

                        // check UUID instead of absolute value of somthing else
                        if (nodeA.UUID.Equals(nodeB.UUID)) continue; 

                        if (nodeA.CheckOverlap(nodeB))
                        {
                            /* with  a 2D loop, each node is compared twice from each perspective
                             and once against itself as well, because we use the entire node list
                            in the stage for both loops*/

                            // continue or remove and proceed?
                            // continue might be cheaper but might also continue to have to 
                            // try and do the alreasdy done or false comparison 

                            if (narrowMap.ContainsKey(nodeA))   continue;
                            if (narrowMap.ContainsKey(nodeB))   continue;
                            if (narrowMap.ContainsValue(nodeA)) continue;
                            if (narrowMap.ContainsValue(nodeB)) continue;

                            narrowMap.Add(nodeA, nodeB);
                        }
                    }
                }
            }
            return narrowMap; 
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
        internal static void GetCollision(Dictionary<Node, Node> narrowMap)
        {
            foreach (var collisionPair in narrowMap)
            {
                GetCollisionComponents(collisionPair, out Rigidbody rbA, out Rigidbody rbB);
                GetDominantBody(rbA, rbB, out Rigidbody submissive, out Rigidbody dominant);
                Collide(submissive, dominant);
            }
        }
        private static void Collide(Rigidbody submissive, Rigidbody dominant)
        {
            submissive.parentNode.position += dominant.velocity;
        }
        private static void GetDominantBody(Rigidbody rbA, Rigidbody rbB, out Rigidbody submissive, out Rigidbody dominant)
        {
            
            if (rbA.velocity.Length >= rbB.velocity.Length)
            {
                dominant = rbA;
                submissive = rbB;
            }
            else
            {
                dominant = rbB;
                submissive = rbA;
            }
            if (rbA.usingGravity && !rbB.usingGravity)
            {
                dominant = rbA;
                submissive = rbB;
            }
            else
            {
                dominant = rbB;
                submissive = rbA;
            }
            if (submissive == null || dominant == null)
            {
                submissive = rbB;
                dominant = rbA;
            }
        }
        /// <summary>
        /// Retrieves all relevant Node components to solve an already verified collision between two Nodes. 
        /// </summary>
        /// <param name="colliders"></param>
        /// <param name="rbA"></param>
        /// <param name="rbB"></param>
        /// <param name="submissive"></param>
        /// <param name="dominant"></param>
        private static void GetCollisionComponents(KeyValuePair<Node, Node> colliders, out Rigidbody rbA, out Rigidbody rbB)
        {
            Node a = colliders.Key;
            Node b = colliders.Value;
            if (a.Name == "Floor" || b.Name == "Floor")
            {
                
            }
            Sprite spriteA = a.GetComponent<Sprite>();
            Sprite spriteB = b.GetComponent<Sprite>();

            rbA = a.GetComponent<Rigidbody>();
            rbB = b.GetComponent<Rigidbody>();
            
            Vec2 sizeA = spriteA.size;
            Vec2 sizeB = spriteB.size;

            Vec2 posA = a.position;
            Vec2 posB = b.position;
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
                if (index < 0 || index >= rows * columns) continue;
                Buckets[index].Add(obj);
            }
        }
        internal List<Node> GetNearby(Node node)
        {
            List<Node> nodes = new List<Node>();
            List<int> buckets = Hash(node);
            foreach (var index in buckets)
            {
                if (index < 0 || index >= rows * columns) continue;
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
                obj.position.x,
                obj.position.y);
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
            AddFloor(nodes);

            for (int i = 0; i < 1000; i++)
            {
                AddDefaultNodes(nodes, i);
            }
            SetCurrentStage(new Stage("Default Stage", runtime.Backgrounds[0], nodes.ToArray()));
            InitializeNodes();
            runtime.stage.RefreshStageDictionary();
        }

        private static void AddFloor(List<Node> nodes)
        {
            Vec2 startPos = new(2, Constants.screenHeight - 4);
            Node floor = new("Floor", startPos, Vec2.one);
            Sprite floorSprite = 
                new(new Vec2(Constants.screenWidth - 4, 10),
                System.Drawing.Color.FromArgb(255, 145, 210, 75),
                true);
           
            Rigidbody floorRb = new()
            {
                usingGravity = false,
                drag = 0f,
                Name = "Floor - Rigidbody"
            };
            floor.AddComponent(floorRb);
            floor.AddComponent(floorSprite);
            nodes.Add(floor);
        }

        private static void InitializeNodes()
        {
            foreach (Node node in runtime.stage.Nodes)
            {
                node.parentStage = runtime.stage;
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
            node.AddComponent(new Sprite(position, JRandom.Color(), true));
            node.AddComponent(new Rigidbody()
            {
                usingGravity = JRandom.Bool(25),
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

}