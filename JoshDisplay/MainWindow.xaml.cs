
using System;
using System.Timers; 
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

using PixelRenderer.Components;
using PixelRenderer.MathP;

using Bitmap = System.Drawing.Bitmap;

namespace PixelRenderer
{
    public partial class MainWindow : Window
    {

        #region Properties & Variables
        int backgroundIndex = 1;
        float gravity = 1f;

        private string? workingDirectory;
        Rectangle[,] Display = new Rectangle[1,1];
        Timer? physicsTimer; 

        public List<Color[,]> Backgrounds = new List<Color[,]>();

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
            InitializeRenderGrid();
            InitializeBitmapCollection();
            SetupDefaultStage(); 
        } // Main()
        private void SetupDefaultStage() // Initialize Default home Stage, Setup one "Player" with Rigidbody and Sprite components
        {
            var nodes = new List<Node>(); // Create 16 new 'Nodes' for new stage;
            var sprite = new Sprite(new Vec2(2, 2), Color.FromArgb(255, 255,255,255), false);
            for (int i = 0; i < 2; i++)
            {
                if (i == 0) // Setup Player
                {
                    nodes.Add(new Node("Player", UUID.NewUUID(), new Vec2(0, 0), new Vec2(1, 1), false)); // add new node

                    // Initialize Rigidbody on Player
                    var playerRigidbody = new Rigidbody();
                    playerRigidbody.takingInput = true;
                    nodes[i].rb = playerRigidbody;
                    playerRigidbody.parentNode = nodes[i];
                    nodes[i].AddComponent(playerRigidbody);

                    // Draw Sprite on Player
                    sprite = new Sprite(new Vec2(5,5), Color.FromArgb(255, 180, 185, 90), true); 
                    nodes[i].sprite = sprite;
                    nodes[i].AddComponent(sprite);
                    continue; 
                }
                
                sprite = new Sprite(new Vec2(24,24), Color.FromArgb((byte)(150 + (i * 2)), 150, 255, 255), true); 
                nodes.Add(new Node($"New Node_{i}", UUID.NewUUID(), new Vec2(i, i), new Vec2(1, 1), false));

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
            SetCurrentStage(new Stage("DefaultStage", Backgrounds[0], nodes.ToArray()));
            foreach (Node node in stage.nodes)
            {
                node.parentStage = stage;
            }
            stage.RefreshStageDictionary();
        }
        public static Color[,] ConvertBitmap(string path)
        {
            var colorArray = new Color[screenWidth, screenHeight];
            Bitmap bitmap = new Bitmap(path);
            if (bitmap.Height > screenHeight || bitmap.Width > screenWidth) throw new NullReferenceException("Bitmap passed into converted with inappropritate size"); 
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    var a = bitmap.GetPixel(x, y).A;
                    var r = bitmap.GetPixel(x, y).R;
                    var g = bitmap.GetPixel(x, y).G;
                    var b = bitmap.GetPixel(x, y).B;
                    colorArray[x, y] = Color.FromArgb(a, r, g, b);
                }
            }
            return colorArray;
        } //Find bitmap .bmp file @ specified path and return array of colors corresponding to each pixel in the image.
        private void WipeDisplay()
        {
            List<int> toRemove = new List<int>();
            for (int i = 0; i < outputGrid.Children.Count; i++)
            {
                // if typeof Rectangle send in list to get removed; 
                if (outputGrid.Children[i].GetType() == typeof(Rectangle))
                { toRemove.Add(i); }

            }
            // iterate on previously created list, now setting them to default color; 
            foreach (int r in toRemove)
            {
                try
                {
                    if (outputGrid.Children.Contains(outputGrid.Children[r]))
                    {
                        if (outputGrid.Children[r] as Rectangle == null) continue;
                        var rect = (Rectangle)outputGrid.Children[r];
                        rect.Fill = Brushes.Black;
                    }
                }
                catch (Exception e)
                {
                    outputTextBox.Text = $"{e.Message}";
                }
            }
            outputGrid.UpdateLayout();
        } // Returns All rectangles to black 
        private void Draw(Color[,] colorData)
        {
            for (int x = 0; x < colorData.GetLength(0); x++)
            {
                for (int y = 0; y < colorData.GetLength(1); y++)
                {
                    if (x > Display.GetLength(0) || y > Display.GetLength(1)) continue;
                    var brush = new SolidColorBrush(colorData[x, y]);
                    if (Display[x, y].Fill == brush) return;
                    Display[x, y].Fill = brush;
                }
            }
        } // render final output image as colorData Array to display
        private void Rendering(object? sender, EventArgs e)
        {
            //FPS counter
            const int frameThreshold = 50;
            if (framesUntilCheck >= frameThreshold)
            {
                lastFrameTime = DateTime.Now.Ticks;
                framesUntilCheck = 0;
                frameCount = 0;
            }
            framesUntilCheck++;

            // Display every node in the debug console (See method for inclusions)
            if (running)
            {
                
                Input.UpdateKeyboardState();
                var frame = (Color[,])stage.Background.Clone();
                frame = DrawSprites(frame);
                if(debugMode) PrintDebugs();
                Draw(frame);
            }
        } // Main Rendering thread for front end, does not handle bitmap pixels
        private void PrintDebugs()
        {
            if (stage.FindNode("Player") == null) return; 
            outputTextBox.Text =
            $" ===STATS===: \n\t {Math.Floor((1 / TimeSpan.FromTicks(DateTime.Now.Ticks - lastFrameTime).TotalSeconds) * frameCount)} Frames Per Second \n PLAYER STATS : {stage.FindNode("Player").rb.GetDebugs()}\t" +
            $"\n\t Current Room : {backgroundIndex}";
            outputTextBox.Text +=
            "\n ===HIERARCHY===";
            outputTextBox.Text +=
            $"\n\t Stage : {stage.Name} (Loaded Nodes : {stage.nodes.Count()}) \n";
            // NODE HEIRARCHY
            foreach (var node in stage.nodes) outputTextBox.Text +=
            $" \n\t Node : \n\t  Name  : {node.Name} \n\t Position : {node.position.x} , {node.position.y} \n\t\t isSprite : {node.sprite} \n\t\t isRigidbody : {node.rb} \t";
            outputTextBox.Text += $" \n {debug} \n";
        }// formats and sends the framerate , hierarchy status and other info to the "Debug Console" outputTextBox;
        public void Update(object? sender, EventArgs e)
        {
            if (stage != null) UpdateCurrentStage(stage); // update all physics objects in current stage 
            else return;

        } // runs PhysicsUpdate() with current scene 
        Color[,] DrawSprites(Color[,] frame)
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
                        if (node.position.x + x > screenWidth) continue;
                        if (node.position.x + x < 0) continue;
                        if (node.position.y + y > screenHeight) continue;
                        frame[(int)node.position.x + x, (int)node.position.y + y] = node.sprite.colorData[x, y];
                    }
                }
            }
            return frame; 
        } // Physics Update methods.
        void CheckCollision()
        {
            for (int i = 0; i < stage.nodes.Length; i++)
            {
                var node = stage.nodes[i];

                if (node.sprite == null) return; 

                // perform collision check against every other node in the scene to passed in node
                if (node.sprite.isCollider)
                {
                    for(int j = i+1; j<stage.nodes.Length; j++)
                    {
                        var collider = stage.nodes[j];
                        // make sure colliders are sprite and have collision enabled
                        if (collider.sprite == null) continue;
                        if (!collider.sprite.isCollider) continue;


                        // check for intersecting sprites
                        if (node.position.x + node.sprite.size.x < collider.position.x || node.position.x > collider.position.x + collider.sprite.size.x) continue;
                        if (node.position.y + node.sprite.size.y < collider.position.y || node.position.y > collider.position.y + collider.sprite.size.y) continue;
                        
                        if (node.Name == "Player" || collider.Name == "Player") node.position.x += 3;
                        
                        return; 
                    
                    }
                }
            }
        } // Handles Collision
        void SyncTransforms(Node node)
        {
            if (node.rb != null)
            {
                node.rb.velocity.y += gravity;
                node.rb.velocity.y *= 0.4f;
                node.rb.velocity.x *= 0.4f;

                node.rb.pos.y += node.rb.velocity.y;
                node.rb.pos.x += node.rb.velocity.x;
                if (node.sprite != null)
                {
                    if (node.rb.pos.y > screenHeight - 4 - node.sprite.size.y)
                    {
                        node.rb.pos.y = screenHeight - 4 - node.sprite.size.y;
                    }

                    if (node.rb.pos.x > screenWidth - node.sprite.size.x)
                    {
                        node.rb.pos.x = screenWidth - node.sprite.size.x;
                        node.rb.velocity.x = 0;
                        //node.rb.takingInput = false;
                        //node.rb.velocity.x = 0;
                        //node.rb.pos.x = screenWidth - node.sprite.size.x;
                        //backgroundIndex++;
                        //SetCurrentStage(new Stage($"Stage {node.Name + backgroundIndex}", Backgrounds[backgroundIndex], stage.nodes));
                        //node.rb.takingInput = true;
                    }
                    if (node.rb.pos.x < 0)
                    {
                        node.rb.pos.x = 0;
                        node.rb.velocity.x = 0;
                        //node.rb.pos.x = 0;
                        //node.rb.velocity.x = 0;

                        //if (backgroundIndex > 1) backgroundIndex--;
                        //else backgroundIndex = 0;
                        //SetCurrentStage(new Stage($"Stage {node.Name + backgroundIndex}", Backgrounds[backgroundIndex], stage.nodes));
                    }
                }
            }
        } // moves all rigidbodies and pending transforms
        private void UpdateCurrentStage(Stage stage)
        {
            stage.RefreshStageDictionary();
            debug = "";
            CheckCollision();
            foreach (Node node in stage)
            {
                // if has collision, calculate accordingly
                // apply physics to rigidbodies
                if (node.rb != null) SyncTransforms(node); 
                    
                // call Update on node thus their components
                node.Update();
            }
            frameCount++;

        } // updates all physics objects within the specified stage
        void SetCurrentStage(Stage stage)
        {
            this.stage = stage; 
        } // sets the current stage to the specified stage.
        private void InitializeRenderGrid()
        {
            Display = new Rectangle[screenWidth,screenHeight];
            for (int i = 0; i < screenHeight; i++)
            {
                outputGrid.RowDefinitions.Add(new RowDefinition());
            }
            for (int i = 0; i < screenWidth; i++)
            {
                outputGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            for (int x = 0; x < outputGrid.ColumnDefinitions.Count ; x++)
            {
                for (int y = 0; y < outputGrid.RowDefinitions.Count  ; y++)
                {
                    // offset text, used to determine X,Y origin of pixel draw, area expanding down right
                    var rect = new Rectangle();
                    Grid.SetColumn(rect, x);
                    Grid.SetRow(rect, y);
                    
                    Grid.SetRow(rect, y);
                    Grid.SetColumn(rect, x);

                   
                    
                    Display[x,y] = rect;
                    outputGrid.Children.Add(rect);
                }

            }
            outputGrid.UpdateLayout();
        } // initializes the zone which all pixels are held, and sets them to a default color. 
        private void InitializeBitmapCollection()
        {
            if (workingDirectory == null) return; 
            foreach (string path in Directory.GetFiles(path: workingDirectory))
            {
                var bitmap = ConvertBitmap(path);
                Backgrounds.Add(bitmap);
            }
        } // find all .bmp bitmap images in working directory of program.
        public void InitializeClocks(TimeSpan interval)
        {
            // if no timer - instantiate; 
            if (physicsTimer == null)
            {
                physicsTimer = new Timer(interval.TotalSeconds);
                CompositionTarget.Rendering += Rendering;
                physicsTimer.Elapsed += Update;
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
        private void Accept_Clicked(object sender, RoutedEventArgs e)
        {
            if (running)
            {
                WipeDisplay();
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
    }
    
}