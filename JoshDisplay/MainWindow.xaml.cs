using System;
using System.Collections.Generic;
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

namespace JoshDisplay
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            outputGrid.ShowGridLines = false;
        }
        private void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        private void Accept_Clicked(object sender, RoutedEventArgs e)
        {
            RefreshRateClock(TimeSpan.FromSeconds(float.Parse(UpdateIntervalInSecs.Text)));
        }
        bool scrollingColor = false;
        bool moving = false;
        bool movingX = false;
        bool movingY = false;
        private bool cleaning;

        // updated on clock tick -- see RefreshRateClock()
        public void RefreshDisplay(object? sender, EventArgs e)
        {
            WipeDisplay();
            if (cleaning) return;
            // length of one side of the screen; a square.
            Int32.TryParse(displayAreaXText.Text, out int displayAreaX);
            Int32.TryParse(displayAreaYText.Text, out int displayAreaY);
            //!! starts in the center if displayArea == 3; && offset == 6;!! 

            Int32.TryParse(displayOffsetXText.Text, out int offsetX);
            Int32.TryParse(displayOffsetYText.Text, out int offsetY);

            if (moving)
            {
                if (movingX)
                {
                    if (offsetX < 16) offsetX++;
                    else if (offsetY < 16)
                    {
                        offsetY++;
                        offsetX = 0;
                    }
                    else offsetY = 0;
                }
                if (movingY)
                {
                    if (offsetY < 16) offsetY++;
                    else if (offsetX < 16)
                    {
                        offsetX++;
                        offsetY = 0;
                    }
                    else offsetX = 0;
                }
                displayOffsetXText.Text = offsetX.ToString();
                displayOffsetYText.Text = offsetY.ToString();
            }
            for (int i = 0; i < displayAreaX; i++)
            {
                //PixelProcessing.Pixel Vec = PixelProcessing.FormatPixelColor(Interface.GetColorValues(inputBox.Text));
                //// Scroll through colors 
                //if (scrollingColor)
                //{
                //    if (Vec.r < 255) Vec.r++; else Vec.r = 0;
                //    if (Vec.g < 255) Vec.g++; else Vec.g = 0;
                //    if (Vec.b < 255) Vec.b++; else Vec.b = 0;
                //}
                for (int j = 0; j < displayAreaY; j++)
                {
                    // get color from text box text string (0,0,0) alpha nyi 
                    PixelProcessing.Pixel Vec = PixelProcessing.FormatPixelColor(Interface.GetColorValues(inputBox.Text));
                    if (scrollingColor)
                    {
                        if (Vec.r < 255) Vec.r++; else Vec.r = 0;
                        if (Vec.g < 255) Vec.g++; else Vec.g = 0;
                        if (Vec.b < 255) Vec.b++; else Vec.b = 0;
                        inputBox.Text = $"{Vec.a},{Vec.r},{Vec.g},{Vec.b}";
                    }
                    Brush color = new SolidColorBrush(Color.FromArgb(255, (byte)Vec.r, (byte)Vec.g, (byte)Vec.b));

                    // offset text, used to determine X,Y origin of pixel draw, area expanding down right

                    outputTextBox.Content = $" Current Color: \n in (R,G,B) {Vec.r} {Vec.g} {Vec.b} \n " +
                        $"xOffset: {offsetX} yOffset: {offsetY} \n xArea: {displayAreaX} yArea: {displayAreaY}";
                    var rect = new Rectangle();
                    rect.Fill = color;
                    Grid.SetColumn(rect, offsetX + i);
                    Grid.SetRow(rect, offsetY + j);
                    Grid.SetRow(rect, offsetY + i);
                    Grid.SetColumn(rect, offsetX + j);
                    outputGrid.Children.Add(rect);
                }
            }
            outputGrid.UpdateLayout();
        }

        public void RefreshRateClock(TimeSpan? interval)
        {
            try
            {
                if (interval == null)
                    return;
            }
            catch (Exception e)
            {
                outputTextBox.Content = e.Message;
            }
            var timer = new DispatcherTimer();
            timer.Tick += RefreshDisplay;
#pragma warning disable CS8629 // Nullable value type may be null.
            timer.Interval = (TimeSpan)interval;
#pragma warning restore CS8629 // Nullable value type may be null.

            if (timer.IsEnabled)
            {
                timer.Stop();
                return;
            }
            timer.Start();
        }
        private void Reset_buttonClick(object sender, RoutedEventArgs e)
        {
            WipeDisplay();
        }
        // Cleans all rectangles off screen
        void WipeDisplay()
        {
            if (cleaning) return;
            cleaning = true;
            List<int> list = new List<int>();
            for (int i = 0; i < outputGrid.Children.Count; i++)
            {
                // ugly IF to check type
                if (outputGrid.Children[i].GetType() != typeof(Button)
                && outputGrid.Children[i].GetType() != typeof(RichTextBox)
                && outputGrid.Children[i].GetType() != typeof(Label)
                && outputGrid.Children[i].GetType() != typeof(TextBox)
                && outputGrid.Children[i].GetType() != typeof(CheckBox))

                { list.Add(i); }
            }
            foreach (int r in list)
            {
                try
                {
                    if (outputGrid.Children.Contains(outputGrid.Children[r]))
                    {
                        outputGrid.Children.RemoveAt(r);
                    }
                }
                catch (Exception e)
                {
                    outputTextBox.Content = e.Message;
                }
            }
            outputGrid.UpdateLayout();
            cleaning = false;
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


    }
    public static class PixelProcessing
    {
        public struct Pixel { public int r; public int g; public int b; public int a; }
        public static Pixel FormatPixelColor(List<int> color)
        {
            Pixel vector = new Pixel();
            vector.r = color[0];
            vector.g = color[1];
            vector.b = color[2];
            vector.a = color[3];
            return vector;
        }
    }

    public static class Interface 
    {
        public static List<int> GetColorValues(string input)
        {
            var list = new List<int>(3);
            var inputs = input.Split(',');
            Int32.TryParse(inputs[0], out var red);
            Int32.TryParse(inputs[1], out var green);
            Int32.TryParse(inputs[2], out var blue);
            Int32.TryParse(inputs[3], out var alpha);
            list.Add(red);
            list.Add(green);
            list.Add(blue);
            list.Add(alpha);

            return list; 
        }
    }
}