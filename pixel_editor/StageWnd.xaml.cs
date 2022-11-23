using pixel_renderer;
using pixel_renderer.Assets;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;


namespace pixel_editor
{
    /// <summary>
    /// Interaction logic for StageWnd.xaml
    /// </summary>
    public partial class StageWnd : Window
    {
        bool usingStarterAssets = false;

        Bitmap? background;

        public StageWnd()
        {
            InitializeComponent();
        }

        private void SetBackgroundClicked(object sender, RoutedEventArgs e)
        {

            Importer.ImportFileDialog(out Asset result);

            if (result is null) return;

            if (result as BitmapAsset is null) return;

            var bmpAsset = result as BitmapAsset;

            var image = bmpAsset.RuntimeValue ?? new(256,256); 

            if (image is not null)
            {
                background = image;
            }
        }
        private async void CreateNewStageButtonPressed(object sender, RoutedEventArgs e)
        {
            int count = nodeCtTxt.Text.ToInt();
            var name = stageNameTxt.Text.ToFileNameFormat();

            List<Node> nodes = new();
            for (int i = 0; i < count; i++)
                Node.CreateGenericNode(nodes, i);

            if (usingStarterAssets) Staging.AddPlayer(nodes);
            if (background is null)
            {
                var msg = MessageBox.Show("No background selected! please navigate to a Bitmap file to continue.");
                await Task.Run(Importer.ImportAssetDialog);
                if (background is null) return;
            }
            var stage = new Stage(name, background, nodes.ToArray());

            var msgResult = MessageBox.Show("Stage Creation complete : Would you like to set this as the current stage?", "Set Stage?", MessageBoxButton.YesNo);

            if (msgResult == MessageBoxResult.Yes)
            {
                Staging.SetCurrentStage(stage);
                var asset = new StageAsset(stage.Name, stage);
                Library.Register(typeof(Stage), asset);
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
