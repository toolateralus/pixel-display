
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PixelRenderer.Components;
using Bitmap = System.Drawing.Bitmap;
using Color = System.Drawing.Color; 
namespace PixelRenderer
{
    public partial class MainWindow : Window
    {

        #region Properties & Variables
        int backgroundIndex = 1;
        const float Gravity = 1f;

        private string? workingDirectory;
        //Rectangle[,] Display = new Rectangle[1, 1];
        Bitmap Display = new Bitmap(screenWidth, screenHeight);
        Timer? physicsTimer;
        public List<Bitmap> Backgrounds = new List<Bitmap>();

        private long lastFrameTime = 0;
        private int framesUntilCheck = 50;

        private const int screenWidth = 64;
        private const int screenHeight = 64;


        int frameCount;
        private bool running;
        Stage stage;
        public string debug = "";
        private bool debugMode;
        #endregion


        public MainWindow()
        {
            workingDirectory = Directory.GetCurrentDirectory() + "\\Images";
            InitializeComponent();
            InitializeBitmapCollection();
            InitializeDefaultStage();
        } // Main()
        
        private void PrintDebugs()
        {
            outputTextBox.Text =
            $" ===STATS===: \n\t {Math.Floor((1 / TimeSpan.FromTicks(DateTime.Now.Ticks - lastFrameTime).TotalSeconds) * frameCount)} Frames Per Second \n PLAYER STATS : {stage.FindNode("Player").rb.GetDebugs()}\t" +
            $"\n\t Current Room : {backgroundIndex}";
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
        }// formats and sends the framerate , hierarchy status and other info to the "Debug Console" outputTextBox;


        void SetCurrentStage(Stage stage)
        {
            this.stage = stage;
        } // sets the current stage to the specified stage.
        private void UpdateCurrentStage(Stage stage)
        {
            stage.RefreshStageDictionary();
            if(debugMode) debug = "";
            foreach (Node node in stage)
            {
                // if has collision, calculate accordingly
                // apply physics to rigidbodies
                if (node.rb != null) ApplyPhysics(node);

                // call Update on node thus their components
                node.Update();
            }
            CheckCollision();
            frameCount++;

        } // updates all physics objects within the specified stage
        
        // Updated methods - called on ticks of timer.
        private void Update(object? sender, EventArgs e)
        {
            const int frameThreshold = 50;
            if (framesUntilCheck >= frameThreshold)
            {
                lastFrameTime = DateTime.Now.Ticks;
                framesUntilCheck = 0;
                frameCount = 0;
            }
            framesUntilCheck++;

           
            if (running)
            {
                Input.UpdateKeyboardState();
                var frame = (Bitmap)stage.Background.Clone();
                frame = DrawSprites(frame);
                if (debugMode) PrintDebugs();
                DisplayBitmap(frame); 
            }
        } // Main Rendering thread for front end, does not handle bitmap pixels
        public void FixedUpdate(object? sender, EventArgs e)
        {
            if (stage != null) UpdateCurrentStage(stage); // update all physics objects in current stage 
            else return;

        } // runs PhysicsUpdate() with current scene 
        

        void ApplyPhysics(Node node)
        {
            if (node.rb != null)
            {
                node.rb.velocity.y += Gravity;
                node.rb.velocity.y *= 0.4f;
                node.rb.velocity.x *= 0.4f;

                node.rb.position.y += node.rb.velocity.y;
                node.rb.position.x += node.rb.velocity.x;
                if (node.sprite != null)
                {
                    if (node.rb.position.y > screenHeight - 4 - node.sprite.size.y)
                    {
                        node.rb.isGrounded = true;
                        node.rb.position.y = screenHeight - 4 - node.sprite.size.y;
                    }
                    else node.rb.isGrounded = false;

                    if (node.rb.position.x > screenWidth - node.sprite.size.x)
                    {
                        node.rb.position.x = screenWidth - node.sprite.size.x;
                        node.rb.velocity.x = 0;
                    }

                    if (node.rb.position.x < 0)
                    {
                       node.rb.position.x = 0;
                       node.rb.velocity.x = 0;

                    }
                }
            }
        }

        //if (backgroundIndex > 1) backgroundIndex--;
        //else backgroundIndex = 0;
        //SetCurrentStage(new Stage($"Stage {node.Name + backgroundIndex}", Backgrounds[backgroundIndex], stage.nodes));
        static bool IsInside(Vec2 point, Vec2 topLeft, Vec2 size)
        {
            if (point.x < topLeft.x) return false;
            if (point.x > topLeft.x + size.x) return false;
            if (point.y < topLeft.y) return false;
            if (point.y > topLeft.y + size.y) return false;
            return true;
        }
        void CheckCollision()
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
                        if (collider.sprite == null) continue;
                        if (!collider.sprite.isCollider) continue;

                        // check for intersecting sprites
                        if (node.position.x + node.sprite.size.x < collider.position.x || node.position.x > collider.position.x + collider.sprite.size.x) continue;
                        if (node.position.y + node.sprite.size.y < collider.position.y || node.position.y > collider.position.y + collider.sprite.size.y) continue;

                        Vec2 position = new();
                        // get 4 corners of collider
                        for (int k = i + 1; k < 4; k++)
                        {
                            Vec2 point = new(node.position.x + node.sprite.size.x * k / 2, node.position.y + node.sprite.size.y * k % 2);
                            if (IsInside(point , collider.position, collider.sprite.size))
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
                            var center = (collider.position.x + collider.sprite.size.x - node.position.x) /2 + collider.position.x + collider.sprite.size.x;
                            node.rb.position.x = center;
                            collider.position.x = center - collider.sprite.size.x;
                        }
                        if (position.x < 0 && position.y == 0)
                        {
                            var median = (collider.position.x + collider.sprite.size.x - node.position.x) / 2 + collider.position.x + collider.sprite.size.x;
                            node.rb.position.x = median;
                            collider.position.x = median - collider.sprite.size.x;
                        }
                        if (position.y > 0 && position.x == 0)
                        {
                            var median = (collider.position.y + collider.sprite.size.y - node.position.y) / 2 + collider.position.y + collider.sprite.size.y;
                            node.rb.position.y = median;
                            collider.position.y = median - collider.sprite.size.y;
                        }
                        if (position.y < 0 && position.x == 0)
                        {
                            var median = (collider.position.y + collider.sprite.size.y - node.position.x) / 2 + collider.position.y + collider.sprite.size.y;
                            node.rb.position.y = median;
                            collider.position.y = median - collider.sprite.size.y;
                        }

                    }
                }
            }
        } // Handles Collision
        

        public static class ConvertBitmapToBitmapImage
        {
            /// <summary>
            /// Takes a bitmap and converts it to an image that can be handled by WPF ImageBrush
            /// </summary>
            /// <param name="src">A bitmap image</param>
            /// <returns>The image as a BitmapImage for WPF</returns>
            public static BitmapImage Convert(Bitmap src)
            {
                MemoryStream ms = new();
                src.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                BitmapImage image = new();
                image.BeginInit();
                ms.Seek(0, SeekOrigin.Begin);
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }

        public void DisplayBitmap(Bitmap input)
        {
            var bitmap = ConvertBitmapToBitmapImage.Convert(input);
            renderImage.Source = bitmap;
          
        }
        Bitmap DrawSprites(Bitmap frame)
        {
            foreach (var node in stage.nodes)
            {
                // draw sprites in scene
                if (node.sprite != null)
                {
                    // Draw pixels according to sprite data
                    for (int x = 0; x < node.sprite.size.x; x++)
                        for (int y = 0; y < node.sprite.size.y; y++)
                        {
                            if (node.position.x + x >= screenWidth) continue;
                            if (node.position.x + x < 0) continue;
                            if (node.position.y + y < 0) continue;
                            if (node.position.y + y >= screenHeight) continue;
                            var index = new Vec2((int)node.position.x + x, (int)node.position.y + y);
                            var sprite = node.sprite.colorData[x, y];
                            frame.SetPixel((int)index.x, (int)index.y, sprite); 
                        }
                }
            }
            return frame;
        } // Physics Update methods.

        private void InitializeDefaultStage() // Initialize Default home Stage, Setup one "Player" with Rigidbody and Sprite components
        {
            var nodes = new List<Node>(); // Create 16 new 'Nodes' for new stage;
            var sprite = new Sprite();
            for (int i = 0; i < 24; i++)
            {
                // SETUP PLAYER @ NODE #0
                if (i == 0)
                {
                    nodes.Add(new Node("Player", UUID.NewUUID(), new Vec2(48, 0), new Vec2(1, 1))); // add new node
                    //Initialize Rigidbody on Player
                    var playerRigidbody = new Rigidbody();
                    playerRigidbody.takingInput = true;
                    nodes[i].rb = playerRigidbody;
                    playerRigidbody.parentNode = nodes[i];
                    nodes[i].AddComponent(playerRigidbody);

                    // Draw Sprite on Player
                    sprite = new Sprite(new Vec2(3, 8), Color.FromArgb(255, 180, 185, 90), true);
                    nodes[i].sprite = sprite;
                    nodes[i].AddComponent(sprite);
                    continue;
                }
                if (i == 1)
                {
                    var sunNode = new Node("Sun", UUID.NewUUID(), new Vec2(59, 0), new Vec2(1, 1));
                    sunNode.AddComponent(new Sprite(new Vec2(5, 5), Color.FromArgb(255, 225, 200, 150), false));
                    nodes.Add(sunNode);
                    continue; 
                }
                sprite = new Sprite(new Vec2(i + 2, i / 2 + 1), Color.FromArgb((byte)(150 + (i * 2)), 150, 255, 255), true);
                nodes.Add(new Node($"New Node_{i}", UUID.NewUUID(), new Vec2(15, 0), new Vec2(1, 1)));

                // add rigidbody component
                var rb = new Rigidbody();
                nodes[i].rb = rb;

                // set up rigidbody
                rb.takingInput = false;
                rb.parentNode = nodes[i];

                // add sprite component
                nodes[i].sprite = sprite;
                sprite.parentNode = nodes[i];

                // finalize component addition
                nodes[i].AddComponent(rb);
                nodes[i].AddComponent(sprite);
                nodes[i].parentNode = nodes[0];

            }
            SetCurrentStage(new Stage("DefaultStage", Backgrounds[0] , nodes.ToArray()));
            foreach (Node node in stage.nodes)
            {
                node.parentStage = stage;
            }
            stage.RefreshStageDictionary();
        }
       
        private void InitializeBitmapCollection()
        {
            if (workingDirectory == null) return;
            foreach (string path in Directory.GetFiles(path: workingDirectory))
            {
                Backgrounds.Add(new Bitmap(path));
            }
        } // find all .bmp bitmap images in working directory of program.
        public void InitializeClocks(TimeSpan interval)
        {
            // if no timer - instantiate; 
            if (physicsTimer == null)
            {
                physicsTimer = new Timer(interval.TotalSeconds);
                CompositionTarget.Rendering += Update;
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

        }     // master clock for update method

        // UI Buttton Events
        private void Accept_Clicked(object sender, RoutedEventArgs e)
        {
            if (running)
            {
                acceptButton.Background = Brushes.Black;
                acceptButton.Foreground = Brushes.Green;
                running = false;
                return;
            }
            acceptButton.Background = Brushes.White;
            acceptButton.Foreground = Brushes.OrangeRed;
            InitializeClocks(TimeSpan.FromSeconds(0.05f));
            running = true;
        }
        private void DebugUnchecked(object sender, RoutedEventArgs e)
        {
            debugMode = false;
        }
        private void DebugChecked(object sender, RoutedEventArgs e)
        {
            debugMode = true;
        }
    }

}