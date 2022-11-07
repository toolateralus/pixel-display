namespace pixel_renderer
{
    using System;
    using System.Windows;
    using System.Windows.Media;

    /// <summary>
    /// Main Entry-Point for App.
    /// </summary>
    public partial class EngineInstance : Window
    {
        // main entry point for application
        public EngineInstance()
        {
            InitializeComponent();
            Runtime.Awake(this);
        }
        // start / stop button on UI.
        public void Accept_Clicked(object sender, RoutedEventArgs e)
        {
            Runtime env = Runtime.Instance;
            if (env.running)
            {
                acceptButton.Background = Brushes.Black;
                acceptButton.Foreground = Brushes.Green;
                env.running = false;
                return;
            }
            acceptButton.Background = Brushes.White;
            acceptButton.Foreground = Brushes.OrangeRed;
            env.InitializeClocks(TimeSpan.FromSeconds(0.05f));
            env.running = true;
        }
        public void DebugUnchecked(object sender, RoutedEventArgs e)
        {
            Debug.debugging = false;
        }
        public void DebugChecked(object sender, RoutedEventArgs e)
        {
            Debug.debugging = true;
        }
        public string x = ""; 
       
    }

}