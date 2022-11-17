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
    using Bitmap = System.Drawing.Bitmap;

    public class Runtime
    {
        private static Runtime instance = new();
        public static Runtime Instance { get { return instance; } }
        /// <summary>
        /// Set to true when the Physics session is initialized.
        /// </summary>
        public bool Initialized { get; internal set; } = false;
        public event Action<InspectorEvent> InspectorEventRaised;

        public EngineInstance mainWnd;
        /// <summary>
        /// used to signify whether the engine is being witnessed by an inspector or not,
        /// useful for throwing errors directly to inspector
        /// </summary>
        public static object? inspector = null; 

        public Timer? physicsTimer;
        public Stage stage;
        public List<Bitmap> Backgrounds = new List<Bitmap>();

        public long lastFrameTime = 0;
        public int BackroundIndex = 0;
        public int framesUntilCheck = 50;
        public int frameCount;

        public bool IsRunning = false; 
        public string ImageDirectory;

        ConcurrentBag<ConcurrentBag<Node>> collisionMap = new();
       

        public static void Awake(EngineInstance mainWnd)
        {
       
            /// Do not change any code below this comment ///
            ImageDirectorySetup();

            //Instance.InitializeBitmapCollection();

            Instance.LoadBackgroundCollection();
            
            Instance.mainWnd = mainWnd;
            
            CompositionTarget.Rendering += Instance.GlobalUpdateRoot;

            Instance.InitializePhysics();

            Instance.Initialized = true;
            /// Do not change any code above this comment ///

            Staging.InitializeDefaultStage();
            
            //FontAssetFactory.InitializeDefaultFont();

        }

       

        private void InitializePhysics()
        {
            var interval = TimeSpan.FromSeconds(Settings.PhysicsRefreshInterval);
            physicsTimer = new Timer(interval.TotalSeconds);
            physicsTimer.Elapsed += GlobalFixedUpdateRoot;
            physicsTimer.Start();
            IsRunning = true; 
        }

        private static void ImageDirectorySetup()
        {
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            Instance.ImageDirectory = appdata + "\\Pixel\\Images";

            if (!Directory.Exists(Instance.ImageDirectory))
            {
                Directory.CreateDirectory(Instance.ImageDirectory);
            }
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
            if (physicsTimer == null)
            {
                throw new NullReferenceException("Physics timer is not set to an Instance of an object. " +
                    "NOTE: Source code may be corrupted or missing," +
                    " Clean your repository and rebuild the engine.");
            }
            if (!physicsTimer.Enabled)
            {
                physicsTimer.Start();
                IsRunning = true;
                return;
            }
            physicsTimer.Stop();
            IsRunning = false;
            return;
        }
        public void InitializeBitmapCollection()
        {
            int i = 0;

            foreach (var file in Directory.GetFiles(ImageDirectory)) Backgrounds.Add(new Bitmap(file));
               
            foreach (var bitmap in Backgrounds)
            {
                i++;
                AssetLibrary.Register(typeof(BitmapAsset), new BitmapAsset("", typeof(Bitmap)) { currentValue = bitmap, Name = $"Background{i}" });
            }
        }

        private void LoadBackgroundCollection()
        {

            List<Bitmap> bitmaps = new();

            // parses pre loaded json objects from the asset library (runtime dictionary containing all assets used and unused.)
            if (AssetLibrary.Fetch<BitmapAsset>(out List<object> bmpAssets))
            {
                foreach (var bmpAsset in bmpAssets)
                {
                    if (bmpAsset as BitmapAsset == null) continue;
                    var asset = bmpAsset as BitmapAsset;
                    var colors = asset.colors;
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
            InspectorEventRaised?.Invoke(e); 
        }
    }

}