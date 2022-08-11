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

            // get color from text box text string (0,0,0) alpha nyi 
            PixelProcessing.Pixel Vec = PixelProcessing.FormatPixelColor(Interface.GetColorValues(inputBox.Text));

            Brush color = new SolidColorBrush(Color.FromArgb(255,  (byte)Vec.r, (byte)Vec.g, (byte)Vec.b));


            // length of one side of the screen; a square.
            //!! starts in the center if displayArea == 3; && offset == 6;!! 
            Int32.TryParse(displayAreaXText.Text, out int displayAreaX);
            Int32.TryParse(displayAreaYText.Text, out int displayAreaY);

            Int32.TryParse(displayOffsetXText.Text, out int offsetX);
            Int32.TryParse(displayOffsetYText.Text, out int offsetY);

            outputTextBox.Content = $" Current Color: \n in (R,G,B) {Vec.r} {Vec.g} {Vec.b} \n " +
                $"xOffset: {offsetX} yOffset: {offsetY} \n xArea: {displayAreaX} yArea: {displayAreaY}";


            for (int i = 0; i < displayAreaX; i++)
            {
                for (int j = 0; j < displayAreaY; j++)
                {
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
    

        private void Reset_buttonClick(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < outputGrid.Children.Count; i++)
            {
                // ugly IF to check type
                if (outputGrid.Children[i].GetType() != typeof(Button)
                &&  outputGrid.Children[i].GetType() != typeof(RichTextBox)
                &&  outputGrid.Children[i].GetType() != typeof(Label)
                &&  outputGrid.Children[i].GetType() != typeof(TextBox)){ 
                    outputGrid.Children.RemoveAt(i);
                }
            }
            outputGrid.UpdateLayout(); 
        }
    }
    public static class PixelProcessing  
    {
        public struct Pixel { public int r; public int g; public int b; public int a;}

        public static Pixel FormatPixelColor(List<int> color)
        {
            Pixel vector = new Pixel();
            vector.r = color[0];
            vector.g = color[1];
            vector.b = color[2];
            return vector; 
        }
        public static void RefreshDisplay()
        {
            
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
            //Int32.TryParse(inputs[3], out var alpha);
            list.Add(red);
            list.Add(green);
            list.Add(blue);

            return list; 
        }
    }
}