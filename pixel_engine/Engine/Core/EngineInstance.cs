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
            ProjectAsset project = LoadOrCreateProject();
            _ = Runtime.Awake(this, project);
        }

        private static ProjectAsset LoadOrCreateProject()
        {
            ProjectAsset project = null;
            Importer.ImportFileDialog(out Asset? asset);
            if (asset is not null)
            {
                if (asset.GetType().Equals(typeof(ProjectAsset)))
                {
                    project = asset as ProjectAsset;
                }
            }
            if (project is null)
            {
                project = new("Default Projekt");
            }
            Library.Register(typeof(ProjectAsset),project);
            Library.Sync();
            return project;
        }
    }
}