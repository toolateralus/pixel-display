using pixel_editor;
using pixel_renderer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
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
using System.Windows.Shapes;

namespace pixel_editor
{
    /// <summary>
    /// Interaction logic for StageWnd.xaml
    /// </summary>
    public partial class StageWnd : Window
    {
     
        public StageWnd()
        {
            InitializeComponent();
        }

        private void SetBackgroundClicked(object sender, RoutedEventArgs e)
        {
           
            AssetPipeline.ImportFileDialog(out Asset result);
            
            if (result is null) return;

            if (result as BitmapAsset is null) return;
            
            var bmpAsset = result as BitmapAsset;
            
            var image = BitmapAsset.BitmapFromColorArray(bmpAsset.Colors);
            
            if (image is not null)
            {
                background = image;
            }
        }
        bool usingStarterAssets = false;
        Bitmap background; 
        private void CreateNewStageButtonPressed(object sender, RoutedEventArgs e)
        {
            var count = int.Parse(nodeCtTxt.Text);
            var name = stageNameTxt.Text;
            List<Node> nodes = new();
            for (int i = 0; i < count; i++)
                Staging.CreateGenericNode(nodes, i);

            var stage = new Stage(name, background, nodes.ToArray());
            var x = MessageBox.Show("Stage Creation complete : Would you like to set this as the current stage?", "Set Stage?",MessageBoxButton.YesNo);
            if (x == MessageBoxResult.Yes)
            {
                Staging.SetCurrentStage(stage);
            }
        }

        private void OnStarterAssetsButtonClicked(object sender, RoutedEventArgs e) => usingStarterAssets = !usingStarterAssets; 
    }
}

public class InspectorProperty
{
    public string Type { get; set; }
    public string Name { get; set; }
    public string Value { get; set; }
    //public static List<InspectorProperty> GetForNode(Node node)
    //{
    //    List<InspectorProperty> properties = new();
    //    foreach (var component in node.ComponentsList)
    //    {
            // deserialize component info
    //    }
    //}
}
