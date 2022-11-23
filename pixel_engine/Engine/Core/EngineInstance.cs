namespace pixel_renderer
{
    using pixel_renderer.Assets;
    using System.Windows;

    /// <summary>
    /// Main Entry-Point for App.
    /// </summary>
    public partial class EngineInstance : Window
    {
        public EngineInstance()
        {
            InitializeComponent();
            Project project = LoadOrCreateProject();
            _ = Runtime.Awake(this, project);
        }

        private static Project LoadOrCreateProject()
        {
            return new("NewProject") { };
        }
    }
}