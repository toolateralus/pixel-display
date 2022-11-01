
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PixelRenderer.Components;
using Bitmap = System.Drawing.Bitmap;

namespace PixelRenderer
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
    }
    public class Runtime
    {
        private static Runtime instance = new(); 
        public static Runtime Instance{ get { return instance; } }
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
            instance.ImageDirectory =  appdata + "\\Pixel\\Images";
            if (!Directory.Exists(instance.ImageDirectory))
            {
                Directory.CreateDirectory(instance.ImageDirectory);
            }
            instance.InitializeBitmapCollection();
            Staging.InitializeDefaultStage(); 
        }
        public void FixedUpdate(object? sender, EventArgs e)
        {
            if (stage != null)
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
        public static List<Bitmap> FrameBuffer = new List<Bitmap>(); 
        // RenderState.Game == Build
        public static RenderState State = RenderState.Game;
        private static Bitmap Draw(Bitmap frame)
        {
            Stage? stage = Runtime.Instance.stage;
            if (stage == null) return new Bitmap(Runtime.Instance.Backgrounds[0]);
            foreach (var node in stage.nodes)
            {
                var sprite = node.GetComponent<Sprite>();
                // draw sprites in scene
                if (sprite != null)
                {
                    // Draw pixels according to sprite data
                    for (int x = 0; x < sprite.size.x; x++)
                        for (int y = 0; y < sprite.size.y; y++)
                        {
                            if (node.position.x + x < 0) continue;
                            if (node.position.y + y < 0) continue;

                            if (node.position.x + x >= Constants.screenWidth) continue;
                            if (node.position.y + y >= Constants.screenHeight) continue;

                            var position = new Vec2((int)node.position.x + x, (int)node.position.y + y);
                            var color = sprite.colorData[x, y];

                            var pixelOffsetX = (int)position.x;
                            var pixelOffsetY = (int)position.y;

                            if (frame.GetPixel(pixelOffsetX, pixelOffsetY) == null) continue;
                            frame.SetPixel(pixelOffsetX, pixelOffsetY, color);
                        }
                }
            }
            return frame;
        }
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
        private static void Insert(Bitmap inputFrame)
        {
            if (FrameBuffer.Count > 1) FrameBuffer.RemoveAt(0);
            FrameBuffer.Add(inputFrame);
        }
        private static void DrawToImage(Bitmap inputFrame, Image renderImage)
        {
            var bitmap = ConvertBitmapToBitmapImage.Convert(inputFrame);
            renderImage.Source = bitmap;
        }
        public static void Render(Image output)
        {
            var runtime = Runtime.Instance; 
            var frame = Draw((Bitmap)runtime.stage.Background.Clone());
            Insert(frame);
            DrawToImage(FrameBuffer[0], output);
        }
    }
    public static class Debug
    {
        // really sloppy quick implementation using the most wasteful string possible
        public static string debug = "";
        public static bool debugging;
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
            $"\n\t Stage : {stage.Name} (Loaded Nodes : {stage.nodes.Count()}) \n";
            // NODE HEIRARCHY
            outputTextBox.Text += "\n\n";
            foreach (var node in stage.nodes)
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
        public static bool IsInside(Vec2 point, Vec2 topLeft, Vec2 size)
        {
            if (point.x < topLeft.x) return false;
            if (point.x > topLeft.x + size.x) return false;
            if (point.y < topLeft.y) return false;
            if (point.y > topLeft.y + size.y) return false;
            return true;
        }
        public static bool IsInside(Node thisNode, out Node[]? collision)
        {
            collision = null;  
            
            if (!thisNode.TryGetComponent(out Sprite sprite)) return false; 
            
            var size = sprite.size;
            var topLeft = thisNode.position - sprite.size; 

            if (thisNode.position.x < topLeft.x)
            {
                return false;
            }
            if (thisNode.position.x > topLeft.x + size.x)
            {
                return false;
            }
            if (thisNode.position.y < topLeft.y)
            {
                return false;
            }
            if (thisNode.position.y > topLeft.y + size.y)
            {
                return false;
            }
            
            return true;
        }
        public static void CheckCollision(Stage stage)
        {
            for (int i = 0; i < stage.nodes.Length; i++)
            {
                var node = stage.nodes[i];
                if (!node.TryGetComponent(out Sprite sprite)) return;
                if (sprite.isCollider)
                {
                    for (int j = i + 1; j < stage.nodes.Length; j++)
                    {
                        var collider = stage.nodes[j];
                        var colliderSprite = collider.GetComponent<Sprite>();
                        // make sure colliders are sprite and have collision enabled
                        
                        if (sprite is null) continue;
                        if (!sprite.isCollider) continue;
                        
                        // check for intersecting sprites
                        if (node.position.x + sprite.size.x < collider.position.x ||
                            node.position.x > collider.position.x + colliderSprite.size.x) continue;
                        
                        if (node.position.y + sprite.size.y < collider.position.y ||
                            node.position.y > collider.position.y + colliderSprite.size.y) continue;
                        
                        Vec2 position = new();
                        
                        // get 4 corners of collider
                        
                        for (int k = i + 1; k < 4; k++)
                        {
                            Vec2 point = new(node.position.x + sprite.size.x * k / 2, node.position.y + sprite.size.y * k % 2);
                            if (IsInside(point, collider.position, colliderSprite.size))
                            {
                                if (point.x < collider.position.x) position.x = -1;
                                if (point.x > collider.position.x + colliderSprite.size.x) position.y = 1;
                                if (point.y < collider.position.y) position.y = -1;
                                if (point.y > collider.position.y + colliderSprite.size.y) position.y = 1;
                                break;
                            }
                        }
                        
                        var nodeRB = node.GetComponent<Rigidbody>(); 

                        if (position.x > 0 && position.y == 0)
                        {
                            var center = (collider.position.x + colliderSprite.size.x - node.position.x) / 2 + collider.position.x + colliderSprite.size.x;
                            nodeRB.parentNode.position.x = center;
                            collider.position.x = center - colliderSprite.size.x;
                        }
                        if (position.x < 0 && position.y == 0)
                        {
                            var median = (collider.position.x + colliderSprite.size.x - node.position.x) / 2 + collider.position.x + colliderSprite.size.x;
                            nodeRB.parentNode.position.x = median;
                            collider.position.x = median - colliderSprite.size.x;
                        }
                        if (position.y > 0 && position.x == 0)
                        {
                            var median = (collider.position.y + colliderSprite.size.y - node.position.y) / 2 + collider.position.y + colliderSprite.size.y;
                            nodeRB.parentNode.position.y = median;
                            collider.position.y = median - colliderSprite.size.y;
                        }
                        if (position.y < 0 && position.x == 0)
                        {
                            var median = (collider.position.y + colliderSprite.size.y - node.position.x) / 2 + collider.position.y + colliderSprite.size.y;
                            nodeRB.parentNode.position.y = median;
                            collider.position.y = median - colliderSprite.size.y;
                        }

                    }
                }
            }
        }
        public static void ViewportCollision(Node parentNode, Sprite sprite, Rigidbody rb)
        {
            if (sprite != null && sprite.isCollider)
            {
                if (parentNode.position.y > Constants.screenHeight - 4 - sprite.size.y)
                {
                    parentNode.position.y = Constants.screenHeight - 4 - sprite.size.y;
                }
                if (parentNode.position.x > Constants.screenWidth - sprite.size.x)
                {
                    parentNode.position.x = Constants.screenWidth - sprite.size.x;
                    rb.velocity.x = 0;
                }
                if (parentNode.position.x < 0)
                {
                    parentNode.position.x = 0;
                    rb.velocity.x = 0;
                }
            }
        }
    }
    public static class Staging
    {
        static Runtime Env = Runtime.Instance; 
        public static void SetCurrentStage(Stage stage) => Env.stage = stage;
        public static void UpdateCurrentStage(Stage stage)
        {
            stage.RefreshStageDictionary();
            stage.Update(delta: Env.lastFrameTime);
            if (Debug.debugging) Debug.debug = "";
            Env.frameCount++;
        }
        public static void InitializeDefaultStage()
        {
            var nodes = new List<Node>();
            AddPlayer(nodes);
            for (int i = 0; i < 3; i++)
            {
                AddDefaultNodes(nodes, i);
            }
            SetCurrentStage(new Stage("Stage One!", Env.Backgrounds[1], nodes.ToArray()));
            InitializeNodes();
            Env.stage.RefreshStageDictionary();
        }
        private static void InitializeNodes()
        {
            foreach (Node node in Env.stage.nodes)
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
            var node = new Node($"NODE {i}", UUID.NewUUID(), new Vec2(0, 0), new Vec2(0, 1));
            var position = Vec2.one * JRandom.RandomInt(1, 3);
            var jumpingBeanScript = new Wind();
            node.AddComponent(jumpingBeanScript); 
            node.AddComponent(new Sprite(position, JRandom.GetRandomColor(), true));
            node.AddComponent(new Rigidbody());
            nodes.Add(node);
        }
        private static void AddPlayer(List<Node> nodes)
        {
            Vec2 playerStartPosition = new Vec2(3, 8);
            Node playerNode = new("Player", UUID.NewUUID(), playerStartPosition , Vec2.one);
            Rigidbody rb = new();
            Wind bean = new(); 
            Sprite sprite = new(Vec2.one* 6, JRandom.GetRandomColor(), true);
            Player player_obj = new()
            {
                TakingInput = true
            };
            playerNode.AddComponent(sprite);
            playerNode.AddComponent(player_obj);
            playerNode.AddComponent(rb);
            playerNode.AddComponent(bean);
            nodes.Add(playerNode);
        }
    }
}