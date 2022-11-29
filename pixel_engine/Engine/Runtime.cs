using Newtonsoft.Json;
using pixel_renderer;
using pixel_renderer.Assets; 
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
using Timer = System.Timers.Timer;
using Bitmap = System.Drawing.Bitmap;
using System.Runtime.InteropServices;

namespace pixel_renderer
{
    public class Runtime
    {
        public EngineInstance mainWnd;
        public Project? LoadedProject = null; 
        private protected static Runtime instance = new();
        public static Runtime Instance { get { return instance; } }
        /// <summary>
        /// Set to true when the Physics session is initialized.
        /// </summary>
        public bool Initialized { get; internal set; } = false;
        public event Action<InspectorEvent> InspectorEventRaised;
        /// <summary>
        /// used to signify whether the engine is being witnessed by an inspector or not,
        /// useful for throwing errors directly to inspector
        /// </summary>
        public static object? inspector = null;
        public Timer? physicsClock;
        public Stage? stage
        {
            get
            { 
                if (m_stageAsset is null) m_stageAsset = StageAsset.Default;
                if (m_stage is null) m_stage = m_stageAsset.Copy();
                return m_stage;
            }
            set => m_stage = value;
        }
        private Stage m_stage; 
        private StageAsset m_stageAsset;
        public void SetStageAsset(StageAsset stageAsset)
        {
            if (IsRunning) Toggle(); 
            stage.Dispose(); 
            m_stageAsset = stageAsset;
            _ = stage; 
        }

        public StageAsset? GetStageAsset() => m_stageAsset;

        public long lastFrameTime = 0;
        public int BackroundIndex = 0;
        public int framesUntilCheck = 50;
        public int frameCount;

        public bool PhysicsInitialized { get; private set; }
        public bool IsRunning = false;
        public string ImageDirectory;

        public static async Task Awake(EngineInstance mainWnd, Project project)
        {
            // changes made to the code below  will likely cause failure or seriously erroneous behaviour
            Instance.LoadedProject = project; 
            Instance.mainWnd = mainWnd;
            await Importer.ImportAsync(false);
            CompositionTarget.Rendering += Instance.GlobalUpdateRoot;
            Instance.Initialized = true;
            // changes made to the code below  will likely cause failure or seriously erroneous behaviour

        }
        public void Toggle()
        {
            if (!PhysicsInitialized) InitializePhysics();
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
            PhysicsInitialized = true; 
        }
        private void ExecuteFrame()
        {
            if (!IsRunning || Rendering.State is RenderState.Off) return; 
            if(Rendering.State is RenderState.Error) throw new Exception("Rendering error");
            if (Rendering.State is RenderState.Game) Rendering.Render(mainWnd.renderImage);
             Input.Refresh();
        }
      
        public void GlobalFixedUpdateRoot(object? sender, EventArgs e)
        {
            Collision.Run(); 
            Staging.Update(stage);
        }
        public void GlobalUpdateRoot(object? sender, EventArgs e)  => ExecuteFrame();
        public void RaiseInspectorEvent(InspectorEvent e) => InspectorEventRaised?.Invoke(e);

        public void SetProject(Project project)
        {
            LoadedProject = project;
            SetStageAsset(project.stages[0]);
        }
    }
}