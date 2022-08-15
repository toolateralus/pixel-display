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
using JoshDisplay.Classes; 
using Bitmap = System.Drawing.Bitmap;
//using System.Threading;

namespace JoshDisplay
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
            var nodes = new List<Node>(16);
            for (int i = 0; i < 16; i++)
            {
                nodes.Add(new Node($"NODE_INSTANCE_{i}", GUID.GetGUID(), new Vec2(i,i) , new Vec2(1,1), false));
            }
            SetCurrentStage(new Stage("DefaultStage", Backgrounds[0], nodes.ToArray()));
            stage.Awake();
            foreach (Node node in stage.nodes)
            {
                node.parentStage = stage;
                dispatchTimer.Tick += node.Update;  
            }
        }

        #region Properties & Variables
        int backgroundIndex = 0;
        float gravity = 1f;

        private string? workingDirectory;
        
        Rectangle[,] Display = new Rectangle[1,1];
        DispatcherTimer dispatchTimer = new DispatcherTimer();
        Timer? physicsTimer; 
        DotCharacter player = new DotCharacter();

        public List<Color[,]> Backgrounds = new List<Color[,]>();

        private long lastFrameTime;
        private int framesUntilCheck;

        private const int screenWidth = 64;
        private const int screenHeight = 64;

        int frameCount;
        private bool running;
        Stage stage;
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
        
        private void Render(Color[,] colorData)
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
        
        private void CompositionTarget_Rendering(object? sender, EventArgs e)
        {
            //FPS counter
            const int frameThreshold = 50;
            if (framesUntilCheck >= frameThreshold)
            {
                // FORMAT FOR OUTPUT TEXT BOX DEBUGGING -- COMMENT OUT FOR A LOT OF PERFORMANCE INCREASE
                SendConsoleDebug();
                // END STRING FORMATTING
                lastFrameTime = DateTime.Now.Ticks;

                framesUntilCheck = 0;
                frameCount = 0;
            }
            framesUntilCheck++;

            // Display every node in the debug console
            
            // Get input
            Input.UpdateKeyboardState();
            var frame = (Color[,])stage.Background.Clone();
            frame[(int)player.pos.x, (int)player.pos.y] = Color.FromRgb(0, 0, 0);
            Render(frame);
        }

        private void SendConsoleDebug()
        {
            outputTextBox.Text =
                            $" ===STATS===: \n\t {Math.Floor((1 / TimeSpan.FromTicks(DateTime.Now.Ticks - lastFrameTime).TotalSeconds) * frameCount)} Frames Per Second " +
                            $"\n\t Current Room : {backgroundIndex}";
            outputTextBox.Text += "\n ===HIERARCHY===";
            outputTextBox.Text += $"\n\t Stage : {stage.Name} (Loaded Nodes : {stage.nodes.Count()}) \n";
            foreach (var node in stage.nodes) outputTextBox.Text +=
            $"\n Node : \n\t Name : {node.Name} \n\t Position : {node.position.x} , {node.position.y} \n";
        }

        public void Update(object? sender, EventArgs e)
        {
            // script update
            player.Update();
            //physics update
            UpdatePlayerPhysics(player);
            if (stage != null) PhysicsUpdate(stage); // update all physics objects in current stage 
            
            frameCount++;
        } // Update -- Mostly drawing pixels. 
        private void PhysicsUpdate(Stage stage)
        {
            //var physicsNodes = stage.GetPhysicsNodes();
            //foreach (var physicsObject in physicsNodes)
            //{
            //    physicsObject.Update();
            //}
        }
        void SetCurrentStage(Stage stage)
        {
            this.stage = stage; 
        }
        private void UpdatePlayerPhysics(DotCharacter player)
        {
            player.vel.y += gravity;
            player.vel.x *= 0.6f;
            player.vel.y *= 0.6f;
            player.pos.y += player.vel.y;
            player.pos.x += player.vel.x;
            // move player left and right
            if (player.pos.x > screenWidth)
            {
                if (backgroundIndex < Backgrounds.Count - 1)
                {
                    backgroundIndex++;
                    player.pos.x = 0;
                }
                else
                {
                    player.vel.x = 0; 
                    player.pos.x = screenWidth -1;
                }
            }
            if (player.pos.x < 0)
            {
                if (backgroundIndex > 0)
                {
                    backgroundIndex--;
                    player.pos.x = screenWidth - 1;
                }
                else
                {
                    player.pos.x = 0;
                    player.vel.x = 0;
                }

            }

            // floor & ceiling prevents ascenscion/descent
            if (player.pos.y > screenHeight - 4)
            {
                player.pos.y = screenHeight - 4;
                player.vel.y = 0;
                player.isGrounded = true;
            }
            else player.isGrounded = false;
            if (player.pos.y < 0)
            {
                player.pos.y = 0;
                player.vel.y = 0;
            }
            
        } // old method to update physics for the player
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
        }
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
            CompositionTarget.Rendering += CompositionTarget_Rendering;

            //if (dispatchTimer != null) dispatchTimer.Stop();
            //dispatchTimer = new DispatcherTimer(DispatcherPriority.Normal);
            //dispatchTimer.Tick += Update;
            //dispatchTimer.Interval = interval;


            if (physicsTimer != null)
            {
                physicsTimer.Stop();
                return;
            }
            //Thread t = new Thread(physicsTimer = new System.Threading.Timer();
            //t.SetApartmentState(ApartmentState.STA);

            physicsTimer = new Timer(interval.TotalSeconds);
            physicsTimer.Elapsed += Update;
            //t.Start();
            
            if (physicsTimer.Enabled)
            {
                physicsTimer.Start();

                return;
            }
            physicsTimer.Start();

            //if (dispatchTimer.IsEnabled)
            //{
            //    dispatchTimer.Stop();
            //    return;
            //}
            //dispatchTimer.Start();
        }     // master clock for update method
        #region UI Button Event Methods
        private void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
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
        private void Reset_buttonClick(object sender, RoutedEventArgs e)
        {
        }
        #endregion

    }
}