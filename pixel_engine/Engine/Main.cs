namespace pixel_renderer
{
    using pixel_renderer.Assets;
    using System;
    using System.Threading.Tasks;
    using System.Windows;

    /// <summary>
    /// Main Entry-Point for App.
    /// </summary>
    public class EngineInstance 
    {
        /// <summary>
        /// use this flag to prevent the engine from running on its own.
        /// </summary>
        public static bool FromEditor; 
        public EngineInstance()
        {
            Importer.Import(false);
            Project project = Project.Load();
            Runtime.Initialize(this, project);
            if (!FromEditor)
            {
                Runtime.ToggleRendering();
                Runtime.TogglePhysics();
            } 
        }
    }
}