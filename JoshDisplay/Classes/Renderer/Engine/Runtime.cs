namespace pixel_renderer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Timers;
    using System.Windows.Media;
    using Bitmap = System.Drawing.Bitmap;

    public class Runtime
    {
        private static Runtime instance = new();
        public static Runtime Instance { get { return instance; } }
        public EngineInstance mainWnd;
        public Timer? physicsTimer;
        public Stage? stage;
        public List<Bitmap> Backgrounds = new List<Bitmap>();

        public long lastFrameTime = 0;
        public int BackroundIndex = 0;
        public int framesUntilCheck = 50;
        public int frameCount;

        public bool running;
        public string? ImageDirectory;

        private void Execute()
        {
            if (running)
            {
                Input.UpdateKeyboardState();
                if (Rendering.State == RenderState.Game) Rendering.Render(mainWnd.renderImage);
                if (Debug.debugging) Debug.Log(mainWnd.outputTextBox);
            }
        }
        private void GetFramerate()
        {
            if (framesUntilCheck >= Constants.frameRateCheckThresh)
            {
                lastFrameTime = DateTime.Now.Ticks;
                framesUntilCheck = 0;
                frameCount = 0;
            }
            framesUntilCheck++;
        }

        public void InitializeClocks(TimeSpan interval)
        {
            if (physicsTimer == null)
            {
                CompositionTarget.Rendering += Update;
                physicsTimer = new Timer(interval.TotalSeconds);
                physicsTimer.Elapsed += FixedUpdate;
                physicsTimer.Start();
                return;
            }
            if (!physicsTimer.Enabled)
            {
                physicsTimer.Start();
                return;
            }
            physicsTimer.Stop();
            return;

        }
        public void InitializeBitmapCollection()
        {
            if (ImageDirectory == null) return;
            foreach (string path in
                Directory.GetFiles(path: ImageDirectory)) Backgrounds.Add(new Bitmap(path));
        }
        public static void Awake(EngineInstance mainWnd)
        {
            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            instance.ImageDirectory = appdata + "\\Pixel\\Images";
            if (!Directory.Exists(instance.ImageDirectory))
            {
                Directory.CreateDirectory(instance.ImageDirectory);
            }

            instance.mainWnd = mainWnd;
            
            instance.InitializeBitmapCollection();
            Staging.InitializeDefaultStage();
            FontAssetFactory.InitializeDefaultFont(); 
            
        }
        List<List<Node>> collisionMap = new();
        public void FixedUpdate(object? sender, EventArgs e)
        {
            if (stage == null) return; 
            _ = Collision.RegisterColliders(stage);
            Collision.BroadPhase(stage, collisionMap);
            Collision.NarrowPhase(collisionMap);
            Collision.Execute();
            Staging.UpdateCurrentStage(stage);
        }
        public void Update(object? sender, EventArgs e)
        {
            GetFramerate();
            Execute();
        }
    }

}