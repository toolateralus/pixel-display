﻿using Newtonsoft.Json;
using pixel_renderer;
using pixel_renderer.Assets; 
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using Timer = System.Timers.Timer;
using Bitmap = System.Drawing.Bitmap;

namespace pixel_renderer
{
    public class Runtime
    {
        private protected static Runtime instance = new();
        public static Runtime Instance { get { return instance; } }
        /// <summary>
        /// Set to true when the Physics session is initialized.
        /// </summary>
        public bool Initialized { get; internal set; } = false;
        public event Action<InspectorEvent> InspectorEventRaised;
        
        [JsonIgnore]
        public EngineInstance mainWnd;

        [JsonIgnore]
        public Project? LoadedProject = null; 
        
        /// <summary>
        /// used to signify whether the engine is being witnessed by an inspector or not,
        /// useful for throwing errors directly to inspector
        /// </summary>
        public static object? inspector = null;

        public Timer? physicsClock;
        public Stage? stage;
        public StageAsset _stage; 
        public List<Bitmap> Backgrounds = new List<Bitmap>();

        public long lastFrameTime = 0;
        public int BackroundIndex = 0;
        public int framesUntilCheck = 50;
        public int frameCount;

        public bool IsRunning = false;
        public string ImageDirectory;

        ConcurrentBag<ConcurrentBag<Node>> collisionMap = new();

        public static async Task Awake(EngineInstance mainWnd, Project project)
        {
            // changes made to the code below  will likely cause failure or seriously erroneous behaviour
            Instance.LoadedProject = project; 
            Instance.mainWnd = mainWnd;
            await Importer.ImportAsync(false);
            CompositionTarget.Rendering += Instance.GlobalUpdateRoot;
            Instance.InitializePhysics();
            await Task.Delay(TimeSpan.FromSeconds(0.1f));
            Instance.LoadBackgroundCollection();
            FontAssetFactory.InitializeDefaultFont();
            Staging.SetCurrentStage(Instance.stage ?? Stage.New);
            Instance.Initialized = true;
            // changes made to the code below  will likely cause failure or seriously erroneous behaviour

        }
        public void Toggle()
        {
            if (physicsClock == null)
            {
                throw new NullReferenceException("Physics Clock is not set to an instance of an object. " +
                    "NOTE: Source code may be corrupted or partially missing," +
                    " Clean your repository and rebuild the engine,");
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
        private void InitializePhysics()
        {
            var interval = TimeSpan.FromSeconds(Settings.PhysicsRefreshInterval);
            physicsClock = new Timer(interval.TotalSeconds);
            physicsClock.Elapsed += GlobalFixedUpdateRoot;
            physicsClock.Start();
            IsRunning = true;
        }
        private void ExecuteFrame()
        {
            if (IsRunning)
            {
                if (Rendering.State == RenderState.Game)
                    Rendering.Render(mainWnd.renderImage);
                Input.Refresh();
            }
        }
        private void LoadBackgroundCollection()
        {
            List<Bitmap> bitmaps = new();
            // parses pre loaded json objects from the asset library (runtime dictionary containing all assets used and unused.)
            if (Library.Fetch<BitmapAsset>(out List<object> bitmapAssetCollection))
            {
                foreach (var asset in bitmapAssetCollection)
                {
                    if (asset as BitmapAsset == null) continue;

                    var bitmapAsset = asset as BitmapAsset;
                    var bitmap = bitmapAsset.RuntimeValue; 
                    if (bitmap.Height == Settings.ScreenH
                        && bitmap.Width == Settings.ScreenW)
                        bitmaps.Add(bitmap);
                }

                if (bitmaps.Count == 0) return;
                Backgrounds = bitmaps;
            }
        }
        public void GlobalFixedUpdateRoot(object? sender, EventArgs e)
        {
            stage ??= Stage.New;  
            _ = Collision.RegisterColliders(stage);
            Collision.BroadPhase(stage, collisionMap);
            Collision.NarrowPhase(collisionMap);
            Collision.Execute();
            Staging.UpdateCurrentStage(stage);
        }
        public void GlobalUpdateRoot(object? sender, EventArgs e)  => ExecuteFrame();
        public void RaiseInspectorEvent(InspectorEvent e) => InspectorEventRaised?.Invoke(e);
    }
}