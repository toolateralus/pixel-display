using System;
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
using Bitmap = System.Drawing.Bitmap; 

namespace JoshDisplay
{

    public partial class MainWindow : Window
    {
        bool scrollingColor = false, moving = false, movingX = false, movingY = false, cleaning= false; 
        int backgroundIndex = 0;
        float gravity = 1f;  
        string fileName;
        private string workingDirectory;
        
        Rectangle[,] Display = new Rectangle[1,1];
        DispatcherTimer timer = new DispatcherTimer();
        DotCharacter player = new DotCharacter();

        public List<Color[,]> Backgrounds = new List<Color[,]>();

        public MainWindow()
        {
            InitializeComponent();
            outputGrid.ShowGridLines = false;
            InitializeRenderGrid();
        }

        private void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        private void Accept_Clicked(object sender, RoutedEventArgs e)
        {
            RefreshRateClock(TimeSpan.FromSeconds(float.Parse(UpdateIntervalInSecs.Text)));
        }
        public void Update(object? sender, EventArgs e)
        {
            // length of one side of the screen; a square.
            Int32.TryParse(displayAreaXText.Text, out int displayAreaX);
            Int32.TryParse(displayAreaYText.Text, out int displayAreaY);

            Int32.TryParse(displayOffsetXText.Text, out int offsetX);
            Int32.TryParse(displayOffsetYText.Text, out int offsetY);

            
            player.Update();
            UpdatePhysics(player); 

            var frame = (Color[,])Backgrounds[backgroundIndex].Clone();
            if (scrollingColor && moving)
            {
                frame = ScrollingGradient(); 
            }
            frame[(int)player.pos.x, (int)player.pos.y] = Color.FromRgb(255,255,255);
            DrawImage(frame);

        } // Update -- Mostly drawing pixels. 
        private void UpdatePhysics(DotCharacter player)
        {
            player.vel.y += gravity;
            player.vel.x *= 0.6f;
            player.vel.y *= 0.6f;
            player.pos.y += player.vel.y;
            player.pos.x += player.vel.x;
            // move player left and right
            if (player.pos.x > 15)
            {
                if (backgroundIndex < Backgrounds.Count - 1)
                {
                    backgroundIndex++;
                    player.pos.x = 0;
                }
                else
                {
                    player.vel.x = 0; 
                    player.pos.x = 15;
                }
            }
            if (player.pos.x < 0)
            {
                if (backgroundIndex > 0)
                {
                    backgroundIndex--;
                    player.pos.x = 15;
                }
                else
                {
                    player.pos.x = 0;
                    player.vel.x = 0;
                }

            }

            // floor & ceiling prevents ascenscion/descent
            if (player.pos.y > 13)
            {
                player.pos.y = 13;
                player.vel.y = 13;
                player.isGrounded = true;
            }
            else player.isGrounded = false;
            if (player.pos.y < 0)
            {
                player.pos.y = 0;
                player.vel.y = 0;
            }
            
        }
        
        public Color[,] ScrollingGradient()
        {
            var inputs = inputBox.Text.Split(',');
            byte.TryParse(inputs[0], out var red);
            byte.TryParse(inputs[1], out var green);
            byte.TryParse(inputs[2], out var blue);
            byte.TryParse(inputs[3], out var alpha);

            if (scrollingColor && moving)
            {
                if (red < 255) red++; else red = 0;
                if (green < 255) green++; else green = 0;
                if (blue < 255) blue++; else blue = 0;
                inputBox.Text = $"{alpha},{red},{green},{blue}";
            }
            var brush = new SolidColorBrush(Color.FromArgb(alpha, red, green, blue));
            var c = new Color[16, 16];
            const byte temp = 16;
            for (int i = 0; i < 16 * 16; i++)
            {
                c[i / 16, i % 16] = Color.FromArgb((byte)i, Convert.ToByte(i % temp), (byte)i, 0) + brush.Color;
            }
            return c; 
        }
        public static Color[,] GetImage(string path)
        {
            
            var colorArray = new Color[16,16];
            Bitmap bitmap = new Bitmap(path);
            for (int x = 0; x < bitmap.Width;x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    var a = bitmap.GetPixel(x, y).A;
                    var r = bitmap.GetPixel(x, y).R;
                    var g = bitmap.GetPixel(x, y).G;
                    var b = bitmap.GetPixel(x, y).B;
                    colorArray[x, y] = Color.FromArgb(a,r,g,b); 
                }
            }
            return colorArray;
        }
        #region Checkbox Events ((Massive Crud Below..))
        private void Reset_buttonClick(object sender, RoutedEventArgs e)
        {
            WipeDisplay();
            if (timer != null) timer.Stop();
        }

        private void ScrollX_checked(object sender, RoutedEventArgs e)
        {
            movingX = true;
        }
        private void ScrollX_unchecked(object sender, RoutedEventArgs e)
        {
            movingX = false;
        }

        private void ScrollY_checked(object sender, RoutedEventArgs e)
        {
            movingY = true;
        }
        private void ScrollY_unchecked(object sender, RoutedEventArgs e)
        {
            movingY = false;
        }

        private void ScrollingColor_checked(object sender, RoutedEventArgs e)
        {
            scrollingColor=true;
        }
        private void ScrollingColor_unchecked(object sender, RoutedEventArgs e)
        {
            scrollingColor=false;
        }

        private void AnimationCheckbox_checked(object sender, RoutedEventArgs e)
        {
            moving = true;
        }
        private void AnimationCheckbox_unchecked(object sender, RoutedEventArgs e)
        {
            moving = false; 
        }
        #endregion
        
        private void InitializeBitmapCollection()
        {
            foreach (string path in Directory.GetFiles(workingDirectory))
            {
                var bitmap = GetImage(path);
                Backgrounds.Add(bitmap);
            }
        }
        private void InitializeRenderGrid()
        {
            workingDirectory = Directory.GetCurrentDirectory() + "\\Images";
            InitializeBitmapCollection(); 
            Int32.TryParse(displayOffsetXText.Text, out int offsetX);
            Int32.TryParse(displayOffsetYText.Text, out int offsetY);
            Display = new Rectangle[16,16];
            // scroll texture back and forth +y or +x 
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    // offset text, used to determine X,Y origin of pixel draw, area expanding down right
                    var rect = new Rectangle();

                    Grid.SetColumn(rect, offsetX + x);
                    Grid.SetRow(rect, offsetY + y);
                    
                    Grid.SetRow(rect, offsetY + y);
                    Grid.SetColumn(rect, offsetX + x);

                   
                    
                    Display[x,y] = rect;
                    outputGrid.Children.Add(rect);
                }

            }
            outputGrid.UpdateLayout();
        }
        public void RefreshRateClock(TimeSpan interval)
        {
            if (timer != null) timer.Stop(); 
            timer = new DispatcherTimer();
            timer.Tick += Update;
            timer.Interval = interval;

            if (timer.IsEnabled)
            {
                timer.Stop();
                return;
            }
            timer.Start();
        }     // master clock for update method
        private void WipeDisplay()
        {
            if (cleaning) return;
            cleaning = true;
            List<int> toRemove = new List<int>();
            for (int i = 0; i < outputGrid.Children.Count; i++)
            {
                // if typeof Rectangle send in list to get removed; 
                if (outputGrid.Children[i].GetType() == typeof(Rectangle))
                { toRemove.Add(i); }
                
            }
            // iterate on previously created list, actually destroying the pixels.
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
                    outputTextBox.Content = $"{ e.Message}";
                }
            }
            outputGrid.UpdateLayout();
            cleaning = false;
        } // Returns All rectangles to black 
        private void DrawImage(Color[,] colorData)
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
        }
    }
    
}