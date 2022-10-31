
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using PixelRenderer.Components;
using Bitmap = System.Drawing.Bitmap;
using Color = System.Drawing.Color;
namespace PixelRenderer
{
    public partial class MainWindow : Window
    {
        // main entry point for application
        public MainWindow()
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
        private static Runtime _runtime = new(); 
        public static Runtime Instance{ get { return _runtime; } }
        
        public MainWindow mainWnd;
        public Timer? physicsTimer;
        public Stage? stage;
        public List<Bitmap> Backgrounds = new List<Bitmap>();
        
        public int BackroundIndex = 0;
        public string? ImageDirectory;
        public bool running;
        public long lastFrameTime = 0;
        public int framesUntilCheck = 50;
        public int frameCount;

        public static void Awake(MainWindow mainWnd)
        {
            _runtime.mainWnd = mainWnd; 
            _runtime.ImageDirectory = Directory.GetCurrentDirectory() + "\\Images";
            _runtime.InitializeBitmapCollection();
            Staging.InitializeDefaultStage(); 
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
        
        public void FixedUpdate(object? sender, EventArgs e)
        {
            if (stage != null)
                Staging.UpdateCurrentStage(stage);
        }
        public void Update(object? sender, EventArgs e)
        {
            HandleFrameRate();
            Execute();
        }
        
        private void Execute()
        {
            if (running)
            {
                Input.UpdateKeyboardState();
                var frame = (Bitmap)stage.Background.Clone();
                frame = Rendering.DrawSprites(frame);
                if (Debug.debugging) Debug.Log(mainWnd.outputTextBox);
                Rendering.DisplayBitmap(frame, mainWnd.renderImage);
            }
        }
        private void HandleFrameRate()
        {
            if (framesUntilCheck >= Constants.frameRateCheckThresh)
            {
                lastFrameTime = DateTime.Now.Ticks;
                framesUntilCheck = 0;
                frameCount = 0;
            }
            framesUntilCheck++;
        }
    }
}
namespace PixelRenderer
{
    public static class Rendering
    {
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
        /// <summary>
        /// Sets the Image UI Element to the last rendered render texture
        /// todo - add framebuffer
        /// </summary>
        /// <param name="input"></param>
        /// <param name="renderImage"></param>
        public static void DisplayBitmap(Bitmap input, Image renderImage)
        {
            var bitmap = ConvertBitmapToBitmapImage.Convert(input);
            renderImage.Source = bitmap;
        }
        public static Bitmap DrawSprites(Bitmap frame)
        {
            Stage stage = Runtime.Instance.stage; 
            foreach (var node in stage.nodes)
            {
                // draw sprites in scene
                if (node.sprite != null)
                {
                    // Draw pixels according to sprite data
                    for (int x = 0; x < node.sprite.size.x; x++)
                        for (int y = 0; y < node.sprite.size.y; y++)
                        {
                            if (node.position.x + x >= Constants.screenWidth) continue;
                            if (node.position.x + x < 0) continue;
                            if (node.position.y + y < 0) continue;
                            if (node.position.y + y >= Constants.screenHeight) continue;
                            var index = new Vec2((int)node.position.x + x, (int)node.position.y + y);
                            var sprite = node.sprite.colorData[x, y];
                            frame.SetPixel((int)index.x, (int)index.y, sprite);
                        }
                }
            }
            return frame;
        }
    }
    public static class Debug
    {
        public static string debug = "";
        public static bool debugging;
        public static void Log(TextBox outputTextBox)
        {
            var runtime = Runtime.Instance;
            Stage stage = runtime.stage; 
            outputTextBox.Text =
            $" ===STATS===: \n\t {Rendering.FrameRate()} Frames Per Second \n PLAYER STATS : {stage.FindNode("Player").rb.GetDebugs()}\t" +
            $"\n\t Current Room : {runtime.BackroundIndex}";
            outputTextBox.Text +=
            "\n ===HIERARCHY===";
            outputTextBox.Text +=
            $"\n\t Stage : {stage.Name} (Loaded Nodes : {stage.nodes.Count()}) \n";
            // NODE HEIRARCHY
            outputTextBox.Text += "\n\n";
            foreach (var node in stage.nodes)
            {
                if (node.Components.Count == 0) return;

                outputTextBox.Text +=
                $" \n\t Node : \n\t  Name  : {node.Name} \n\t\t Position : {node.position.x} , {node.position.y} ";

                if (node.sprite != null)
                {
                    outputTextBox.Text +=
                    $"\n\t isSprite : {node.sprite} \n\t\t";
                }

                if (node.rb != null)
                {
                    outputTextBox.Text +=
                    $" isRigidbody : {node.rb} \n\t Velocity : {node.rb.velocity.x} , {node.rb.velocity.y}\n ";
                }
            }
            outputTextBox.Text += $" \n {debug} \n";
        }
        public static void Log(TextBox outputTextBox, object additionalData)
        {
            string additional = additionalData.ToString(); 
            if (additional == null)
            {
                additional = "";
            }
            var runtime = Runtime.Instance;
            Stage stage = runtime.stage;
            outputTextBox.Text =
            $" ===STATS===: \n\t {Rendering.FrameRate()} Frames Per Second \n PLAYER STATS : {stage.FindNode("Player").rb.GetDebugs()}\t" +
            $"\n\t Current Room : {runtime.BackroundIndex}";
            outputTextBox.Text +=
            "\n ===HIERARCHY===";
            outputTextBox.Text +=
            $"\n\t Stage : {stage.Name} (Loaded Nodes : {stage.nodes.Count()}) \n";
            // NODE HEIRARCHY
            outputTextBox.Text += "\n\n";
            foreach (var node in stage.nodes)
            {
                if (node.Components.Count == 0) return;

                outputTextBox.Text +=
                $" \n\t Node : \n\t  Name  : {node.Name} \n\t\t Position : {node.position.x} , {node.position.y} ";

                if (node.sprite != null)
                {
                    outputTextBox.Text +=
                    $"\n\t isSprite : {node.sprite} \n\t\t";
                }

                if (node.rb != null)
                {
                    outputTextBox.Text +=
                    $" isRigidbody : {node.rb} \n\t Velocity : {node.rb.velocity.x} , {node.rb.velocity.y}\n ";
                }
            }
            outputTextBox.Text += $" \n {debug} \n";
        }
    }
    public static class Collision
    {
        static bool IsInside(Vec2 point, Vec2 topLeft, Vec2 size)
        {
            if (point.x < topLeft.x) return false;
            if (point.x > topLeft.x + size.x) return false;
            if (point.y < topLeft.y) return false;
            if (point.y > topLeft.y + size.y) return false;
            return true;
        }
        public static void CheckCollision(Stage stage)
        {
            for (int i = 0; i < stage.nodes.Length; i++)
            {
                var node = stage.nodes[i];

                if (node.sprite == null) return;

                // perform collision check against every other node in the scene to passed in node
                if (node.sprite.isCollider)
                {
                    for (int j = i + 1; j < stage.nodes.Length; j++)
                    {
                        var collider = stage.nodes[j];
                        // make sure colliders are sprite and have collision enabled
                        if (collider.sprite is null) continue;
                        if (!collider.sprite.isCollider) continue;

                        // check for intersecting sprites
                        if (node.position.x + node.sprite.size.x < collider.position.x ||
                            node.position.x > collider.position.x + collider.sprite.size.x) continue;
                        if (node.position.y + node.sprite.size.y < collider.position.y ||
                            node.position.y > collider.position.y + collider.sprite.size.y) continue;

                        Vec2 position = new();
                        // get 4 corners of collider
                        for (int k = i + 1; k < 4; k++)
                        {
                            Vec2 point = new(node.position.x + node.sprite.size.x * k / 2, node.position.y + node.sprite.size.y * k % 2);
                            if (IsInside(point, collider.position, collider.sprite.size))
                            {
                                if (point.x < collider.position.x) position.x = -1;
                                if (point.x > collider.position.x + collider.sprite.size.x) position.y = 1;
                                if (point.y < collider.position.y) position.y = -1;
                                if (point.y > collider.position.y + collider.sprite.size.y) position.y = 1;
                                break;
                            }
                        }

                        if (position.x > 0 && position.y == 0)
                        {
                            var center = (collider.position.x + collider.sprite.size.x - node.position.x) / 2 + collider.position.x + collider.sprite.size.x;
                            node.rb.parentNode.position.x = center;
                            collider.position.x = center - collider.sprite.size.x;
                        }
                        if (position.x < 0 && position.y == 0)
                        {
                            var median = (collider.position.x + collider.sprite.size.x - node.position.x) / 2 + collider.position.x + collider.sprite.size.x;
                            node.rb.parentNode.position.x = median;
                            collider.position.x = median - collider.sprite.size.x;
                        }
                        if (position.y > 0 && position.x == 0)
                        {
                            var median = (collider.position.y + collider.sprite.size.y - node.position.y) / 2 + collider.position.y + collider.sprite.size.y;
                            node.rb.parentNode.position.y = median;
                            collider.position.y = median - collider.sprite.size.y;
                        }
                        if (position.y < 0 && position.x == 0)
                        {
                            var median = (collider.position.y + collider.sprite.size.y - node.position.x) / 2 + collider.position.y + collider.sprite.size.y;
                            node.rb.parentNode.position.y = median;
                            collider.position.y = median - collider.sprite.size.y;
                        }

                    }
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
            // Create 16 new 'Nodes' for new stage;
            var sprite = new Sprite();
            for (int i = 0; i < 24; i++)
            {
                if (i == 0)
                {
                    nodes.Add(new Node("Player", UUID.NewUUID(), new Vec2(48, 0), new Vec2(1, 1))); // add new node
                    //Initialize Rigidbody on Player
                    var playerRigidbody = new Rigidbody();
                    nodes[i].rb = playerRigidbody;
                    nodes[i].AddComponent(playerRigidbody);

                    // Draw Sprite on Player
                    sprite = new Sprite(new Vec2(3, 8), Color.FromArgb(255, 180, 185, 90), true);
                    nodes[i].sprite = sprite;
                    nodes[i].AddComponent(sprite);
                    var player = new Player();
                    nodes[i].AddComponent(player);
                    continue;
                }
                if (i == 1)
                {
                    var sunNode = new Node("Sun", UUID.NewUUID(), new Vec2(59, 0), new Vec2(1, 1));
                    sunNode.AddComponent(new Sprite(new Vec2(5, 5), Color.FromArgb(255, 225, 200, 150), true));
                    sunNode.AddComponent(new Rigidbody());
                    nodes.Add(sunNode);
                    continue;
                }
                // add rigidbody component
                sprite = new Sprite(new Vec2(i + 2, i / 2 + 1), Color.FromArgb((byte)(150 + (i * 2)), 150, 255, 255), true);
                nodes.Add(new Node($"New Node_{i}", UUID.NewUUID(), new Vec2(15, 0), new Vec2(1, 1)));
                
                var rb = new Rigidbody();
                nodes[i].rb = rb;
                // set up rigidbody
                // add sprite component
                nodes[i].sprite = sprite;
                // finalize component addition
                nodes[i].AddComponent(rb);
                nodes[i].AddComponent(sprite);

            }
            SetCurrentStage(new Stage("DefaultStage", Env.Backgrounds[0], nodes.ToArray()));
            foreach (Node node in Env.stage.nodes)
            {
                node.parentStage = Env.stage;
                foreach (var component in node.Components)
                {
                    component.Value.Awake(); 
                }
            }
            
            Env.stage.RefreshStageDictionary();
        }
    }
}