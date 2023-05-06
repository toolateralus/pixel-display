using Pixel;
using System.Windows;

namespace Pixel_Editor
{
    /// <summary>
    /// Interaction logic for GameView.xaml
    /// </summary>
    public partial class GameView : Window
    {
        public GameView(Window owner)
        {
            InitializeComponent();
            Runtime.SetOutputImageAsMain(RenderImage);


            Closing += (e, o) => 
            {
                if (owner is Editor editor)
                {
                    Runtime.SetOutputImageAsMain(editor.mainImage);
                    editor.gameView = null;
                }
            };
            owner.Closing += (e, o) =>
            {
                Close();
            };
            Show();
        }
    }
}
