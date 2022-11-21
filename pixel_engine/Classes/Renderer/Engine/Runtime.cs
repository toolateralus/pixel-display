namespace pixel_renderer
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using System.Timers;
    using System.Windows;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Bitmap = System.Drawing.Bitmap;

    public class Runtime
    {
        private static Runtime instance = new();
        public static Runtime Instance { get { return instance; } }
        /// <summary>
        /// Set to true when the Physics session is initialized.
        /// </summary>
        public bool Initialized { get; internal set; } = false;
        public event Action<InspectorEvent, Action> InspectorEventRaised;

        public EngineInstance mainWnd;
        /// <summary>
        /// used to signify whether the engine is being witnessed by an inspector or not,
        /// useful for throwing errors directly to inspector
        /// </summary>
        public static object? inspector = null; 

        public Timer? physicsClock;
        public Stage stage;
        public List<Bitmap> Backgrounds = new List<Bitmap>();

        public long lastFrameTime = 0;
        public int BackroundIndex = 0;
        public int framesUntilCheck = 50;
        public int frameCount;

        public bool IsRunning = false; 
        public string ImageDirectory;

        ConcurrentBag<ConcurrentBag<Node>> collisionMap = new();
       
        public static async Task Awake(EngineInstance mainWnd)
        {
            // changes made to the code below can and will likely cause massive errors or failure

            Instance.mainWnd = mainWnd;

            await AssetPipeline.ImportAsync(false);

            CompositionTarget.Rendering += Instance.GlobalUpdateRoot;

            Instance.LoadBackgroundCollection();

            Instance.InitializePhysics();

            Instance.Initialized = true;

            await Staging.InitializeDefaultStage();

            FontAssetFactory.InitializeDefaultFont();

            // changes made to the code above can and will likely cause massive errors or failure
        }

        private void InitializePhysics()
        {
            var interval = TimeSpan.FromSeconds(Settings.PhysicsRefreshInterval);
            physicsClock = new Timer(interval.TotalSeconds);
            physicsClock.Elapsed += GlobalFixedUpdateRoot;
            physicsClock.Start();
            IsRunning = true; 
        }
       
        private void Execute()
        {
            if (IsRunning)
            {
                if (Rendering.State == RenderState.Game) 
                    Rendering.Render(mainWnd.renderImage);
                Input.Refresh();
               
            }
        }
        private void GetFramerate()
        {
            if (framesUntilCheck >= Settings.FramesBetweenFramerateChecks)
            {
                lastFrameTime = DateTime.Now.Ticks;
                framesUntilCheck = 0;
                frameCount = 0;
            }
            framesUntilCheck++;
        }
        /// <summary>
        /// Toggle Updating of Physics on and off (also affects FixedUpdate, since they are called in tandem.)
        /// </summary>
        /// <exception  cref="NullReferenceException"> </exception>  
        
        public void Toggle()
        {
            if (physicsClock == null)
            {
                throw new NullReferenceException("Physics Clock is not set to an instance of an object. " +
                    "NOTE: Source code may be corrupted or partially missing," +
                    " Clean your repository and rebuild the engine, or fix the Clock XD");
            }
            if (!physicsClock.Enabled)
            {
                physicsClock.Start();
                IsRunning = true;
                return;
            }
            physicsClock.Stop();
            IsRunning = false;
            return;
        }

        private void LoadBackgroundCollection()
        {

            List<Bitmap> bitmaps = new();

            // parses pre loaded json objects from the asset library (runtime dictionary containing all assets used and unused.)

            if (AssetLibrary.Fetch<BitmapAsset>(out List<object> bitmapAssetCollection))
            {
                foreach (var asset in bitmapAssetCollection)
                {
                    if (asset as BitmapAsset == null) continue;
                    var bitmapAsset = asset as BitmapAsset;
                    
                    var colors = bitmapAsset.Colors;
                    
                    var bitmap = BitmapAsset.BitmapFromColorArray(colors);

                    if (bitmap.Height == Settings.ScreenHeight
                        && bitmap.Width == Settings.ScreenWidth)
                            bitmaps.Add(bitmap);
                }

                if (bitmaps.Count == 0) return; 
                
                Backgrounds = bitmaps;
            }
        }

        public void GlobalFixedUpdateRoot(object? sender, EventArgs e)
        {
            if (stage == null)
            {
                return; 
            }

            _ = Collision.RegisterColliders(stage);
            Collision.BroadPhase(stage, collisionMap);
            Collision.NarrowPhase(collisionMap);
            Collision.Execute();

            Staging.UpdateCurrentStage(stage);
        }
        public void GlobalUpdateRoot(object? sender, EventArgs e)
        {
            GetFramerate();
            Execute();
        }

        internal void RaiseInspectorEvent(InspectorEvent e)
        {
            InspectorEventRaised?.Invoke(e , e.expression); 
        }
    }

}