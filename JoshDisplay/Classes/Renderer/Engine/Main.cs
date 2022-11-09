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
        private const float PhysicsRefreshInterval = 0.1f;


        public EngineInstance()
        {
            InitializeComponent();
            Runtime.Awake(this);
            
        }

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
            env.InitializeClocks(TimeSpan.FromSeconds(PhysicsRefreshInterval));
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
       
    }

}