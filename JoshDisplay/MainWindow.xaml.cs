using System;
using System.Timers; 
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using PixelRenderer.Components; 
using Bitmap = System.Drawing.Bitmap;
//using System.Threading;

namespace PixelRenderer
{
    public partial class MainWindow : Window
    {
        // Main()
        public MainWindow()
        {
            workingDirectory = Directory.GetCurrentDirectory() + "\\Images";
            InitializeComponent();
            InitializeRenderGrid();
            InitializeBitmapCollection();
            SetupDefaultStage(); 
        }

        private void SetupDefaultStage() // Initialize Default home Stage, Setup one "Player" with Rigidbody and Sprite components
        {
            var nodes = new List<Node>(); // Create 16 new 'Nodes' fir new stage; will be read from a file presumably
            var sprite = new Sprite(new Vec2(2, 2), Color.FromArgb(255, 255,255,255), false);
            for (int i = 0; i < 17; i++)
            {
                byte z = (byte)(i * 25);
                if (i == 0) // Setup Player
                {
                    nodes.Add(new Node("Player", UUID.NewUUID(), new Vec2(i, i), new Vec2(1, 1), false)); // add new node
                    
                    // Initialize Rigidbody on Player
                    var playerRigidbody = new Rigidbody();
                    playerRigidbody.takingInput = true; 
                    nodes[i].rb = playerRigidbody;
                    playerRigidbody.parentNode = nodes[i];
                    _playerRigidbody = playerRigidbody;
                    nodes[i].Components.Add(playerRigidbody);
                
                    // Draw Sprite on Player
                    nodes[i].sprite = sprite;
                    nodes[i].Components.Add(sprite);
                }
                var rb = new Rigidbody();
                nodes[i].rb = rb;
                rb.takingInput = true; 
                rb.parentNode = nodes[i];
                nodes[i].sprite = sprite;
                sprite.parentNode = nodes[i];
                nodes[i].Components.Add(rb);
                nodes[i].Components.Add(sprite);
                nodes.Add(new Node($"New Node_{i}", UUID.NewUUID(), new Vec2(i, i), new Vec2(1, 1), false));
                nodes[i].parentNode = nodes[0];
                
            }
            SetCurrentStage(new Stage("DefaultStage", Backgrounds[0], nodes.ToArray()));
            stage.Awake();
            foreach (Node node in stage.nodes)
            {
                node.parentStage = stage;
            }
        }

        #region Properties & Variables
        int backgroundIndex = 0;
        float gravity = 1f;

        private string? workingDirectory;
        Rigidbody _playerRigidbody;
        Rectangle[,] Display = new Rectangle[1,1];
        DispatcherTimer dispatchTimer = new DispatcherTimer();
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
        private bool canProceed;
        #endregion

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
        private void FinalRender(Color[,] colorData)
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
        private void PreRendering(object? sender, EventArgs e)
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
                SendConsoleDebug();
                foreach (var node in stage.nodes)
                {
                    // draw sprites in scene
                    if (node.sprite != null)
                    {
                        for (int i = 0; i < node.sprite.size.x; i++)
                        {
                            for (int j = 0; j < node.sprite.size.y; j++)
                            {
                              frame[(int)node.position.x + i , (int)node.position.y + j] = node.sprite.color;
                            }
                        }
                    }
                    
                }
                frame[(int)_playerRigidbody.pos.x, (int)_playerRigidbody.pos.y] = Color.FromRgb(0, 0, 0);
                FinalRender(frame);
            }
        } // Main Rendering thread for front end, does not handle bitmap pixels
        private void SendConsoleDebug()
        {
            outputTextBox.Text =
                            $" ===STATS===: \n\t {Math.Floor((1 / TimeSpan.FromTicks(DateTime.Now.Ticks - lastFrameTime).TotalSeconds) * frameCount)} Frames Per Second \n PLAYER STATS : {_playerRigidbody.GetDebugs()}\t" +
                            $"\n\t Current Room : {backgroundIndex}";
            outputTextBox.Text += "\n ===HIERARCHY===";
            outputTextBox.Text += $"\n\t Stage : {stage.Name} (Loaded Nodes : {stage.nodes.Count()}) \n";
            
            // NODE HEIRARCHY
            foreach (var node in stage.nodes) outputTextBox.Text += 
            $" \n\t\b Node : \n\t \b Name  : {node.Name} \n\t \b Position : {node.position.x} , {node.position.y} \n\t\t BOOL__isSprite : {node.sprite} \n\t\t BOOL__isRigidbody : {node.rb} \t";
            
            outputTextBox.Text += $" \n {debug} \n";
        }// formats and sends the framerate , hierarchy status and other info to the "Debug Console" outputTextBox;
        public void Update(object? sender, EventArgs e)
        {
            if (stage != null) PhysicsUpdate(stage); // update all physics objects in current stage 
            else return;
          

        } // runs PhysicsUpdate() with current scene 
        private void PhysicsUpdate(Stage stage)
        {
            foreach (Node node in stage)
            {
                // apply physics to rigidbodies
                if (node.rb != null)
                {
                    node.rb.velocity.y += gravity;
                    node.rb.velocity.y *= 0.6f;
                    node.rb.velocity.x *= 0.6f;
                    
                    node.rb.pos.y += node.rb.velocity.y;
                    node.rb.pos.x += node.rb.velocity.x;
                    
                    if (node.rb.pos.y > screenHeight - 4 - node.sprite.size.y)
                    {
                        node.rb.pos.y = screenHeight - 4 - node.sprite.size.x;
                        node.rb.velocity.y *= -20f;
                    }

                    if (node.rb.pos.x >= screenWidth - node.sprite.size.x)
                    {
                        if (canProceed)
                        {
                            canProceed = false;
                            backgroundIndex++;
                            SetCurrentStage(new Stage("Stage 000", Backgrounds[backgroundIndex], stage.nodes));
                            node.rb.pos.x = screenWidth - node.sprite.size.x;
                            node.rb.velocity.x = 0;
                            canProceed = true; 
                        }
                    }

                    if (node.rb.pos.x <= 0)
                    {
                        node.rb.pos.x = 0;
                        node.rb.velocity.x = 0;
                    }
                    node.Update();
                }
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
            CompositionTarget.Rendering += PreRendering;
            if (physicsTimer != null)
            {
                physicsTimer.Stop();
                return;
            }
            physicsTimer = new Timer(interval.TotalSeconds);
            physicsTimer.Elapsed += Update;
            if (physicsTimer.Enabled)
            {
                physicsTimer.Start();

                return;
            }
            physicsTimer.Start();
        }     // master clock for update method
        private void Accept_Clicked(object sender, RoutedEventArgs e)
        {
            if (running)
            {
                WipeDisplay();
                acceptButton.Background = Brushes.Azure;
                acceptButton.Foreground = Brushes.Red;
                if (dispatchTimer != null) dispatchTimer.Stop();
                running = false;
                return;
            }
            acceptButton.Background = Brushes.Red;
            acceptButton.Foreground = Brushes.White;
            InitializeClocks(TimeSpan.FromSeconds(float.Parse(updateTickBox.Text)));
            running = true;
        }
    }
}