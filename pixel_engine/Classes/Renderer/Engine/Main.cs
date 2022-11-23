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
            ProjectAsset project = LoadOrCreateProject();
            _ = Runtime.Awake(this, project);
        }

        private static ProjectAsset LoadOrCreateProject()
        {
            ProjectAsset project = null;
            AssetPipeline.ImportFileDialog(out Asset? asset);
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
            AssetLibrary.Register(typeof(ProjectAsset),project);
            AssetLibrary.Sync();
            return project;
        }
    }
}