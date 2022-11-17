namespace pixel_renderer
{
    using System.Windows;

    /// <summary>
    /// Main Entry-Point for App.
    /// </summary>
    public partial class EngineInstance : Window
    {
        public EngineInstance()
        {
            InitializeComponent();

            AssetPipeline.ImportAsync(false);

            Runtime.Awake(this);
        }
    }
}