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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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
            // get color from text box (0,0,0)
            PixelProcessing.Pixel Vec = PixelProcessing.FormatPixelColor(Interface.GetColorVector(inputBox.Text));

            Brush color = new SolidColorBrush(Color.FromRgb((byte)Vec.r, (byte)Vec.g, (byte)Vec.b));

            outputTextBox.Content = $" Current Color: \n (R,G,B) \n {Vec.r} {Vec.g} {Vec.b}";

            // length of one side of the screen; a square.
             //!! starts in the center if displayArea == 3; && offset == 6;!! 
            byte displayAreaX = (byte)Char.GetNumericValue(displayAreaXText.Text[0]);
            byte displayAreaY = (byte)Char.GetNumericValue(displayAreaYText.Text[0]);
            byte offsetX = (byte)Char.GetNumericValue(displayOffsetXText.Text[0]);
            byte offsetY = (byte)Char.GetNumericValue(displayOffsetYText.Text[0]);

            for (int i = 0; i < displayAreaX; i++)
            {
                for (int j = 0; j < displayAreaY; j++)
                {
                    var rect = new Rectangle();
                    rect.Fill = color;
                    Grid.SetRow(rect, offsetY + i);
                    Grid.SetRow(rect, offsetY + j);
                    Grid.SetColumn(rect, offsetX + j);
                    Grid.SetColumn(rect, offsetX + i);
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
        public static List<int> GetColorVector(string rawInput)
        {
            var list = new List<int>(2);
            try
            {
                var chars = rawInput.Split(',');
                for (int i = 0; i < chars.Length; i++)
                {
                    var j = chars[i][0];
                    if (Char.GetNumericValue(j) >= 0)
                    {
                        list.Add(j - 48);
                    }
                }
            }
            catch (Exception e) { Console.WriteLine(e.Message); }
            return list;
        }
    }
}
