namespace pixel_renderer
{
    using pixel_renderer.Assets;
    using pixel_renderer.Projects;
    using System.Windows;

    /// <summary>
    /// Main Entry-Point for App.
    /// </summary>
    public partial class EngineInstance : Window
    {
        public EngineInstance()
        {
            InitializeComponent();
            Project project = Project.LoadProject();
            ProjectIO.SaveProject(project);
            _ = Runtime.Awake(this, project);
        }

        
    }
}