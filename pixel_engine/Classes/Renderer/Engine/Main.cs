namespace pixel_renderer
{
    using System;
    using System.Windows;

    /// <summary>
    /// Main Entry-Point for App.
    /// </summary>
    public partial class EngineInstance : Window
    {
        public EngineInstance()
        {
            InitializeComponent();

            _ = Runtime.Awake(this);
        }

      
    }
}