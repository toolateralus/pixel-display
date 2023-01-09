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
        public EngineInstance()
        {
            InitializeComponent();
            Runtime.Awake(this, new(""));
        }
    }
}