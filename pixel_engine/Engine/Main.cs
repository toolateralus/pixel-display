namespace pixel_renderer
{
    using pixel_renderer.Assets;
    using System;
    using System.Threading.Tasks;
    using System.Windows;

    /// <summary>
    /// Main Entry-Point for App.
    /// </summary>
    public partial class EngineInstance : Window
    {
        /// <summary>
        /// use this flag to prevent the engine from running on its own.
        /// </summary>
        public static bool FromEditor; 
        public EngineInstance()
        {
            Project project = Project.Load();
            InitializeComponent();
            Runtime.Initialize(this, project);
            if (!FromEditor)
            {
                Runtime.ToggleRendering();
                Runtime.TogglePhysics();
                Runtime.OutputImages.Add(renderImage);
            } 
        }
    }
}