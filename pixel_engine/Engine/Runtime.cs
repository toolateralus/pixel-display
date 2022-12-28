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
using System.Runtime.CompilerServices;

namespace pixel_renderer
{
    public class Runtime
    {
        public EngineInstance mainWnd;
        public Timer? physicsClock;
        public RenderHost? renderHost = new();
        public Project? LoadedProject = null;
        public StagingHost? stagingHost = new();
        private StageAsset? m_stageAsset;
        private protected volatile Stage? m_stage;
        
        public event Action<InspectorEvent> InspectorEventRaised;

        public bool IsRunning { get; private set; }
        private protected bool PhysicsInitialized = false;
        private protected bool Initialized = false;
        
        public static object? inspector = null;

        private protected volatile static Runtime instance = new();
        public static Runtime Instance
        {
            get
            {
                if (instance is not null)
                    return instance;
                else instance = new();
                return instance;
            }
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
        public Stage? GetStage()
        {
            if (m_stageAsset is null) m_stageAsset = StageAsset.Default;
            if (m_stage is null || m_stage.UUID != m_stageAsset.settings.UUID)
                m_stage = m_stageAsset?.Copy();
            return m_stage;
        }

        public static async Task AwakeAsync(EngineInstance mainWnd, Project project)
        {
            Instance.LoadedProject = project;
            Instance.mainWnd = mainWnd;
            await Importer.ImportAsync(false);
            CompositionTarget.Rendering += Instance.GlobalUpdateRoot;
            Instance.Initialized = true;
        }
        private void InitializePhysics()
        {
            var interval = TimeSpan.FromSeconds(Settings.PhysicsRefreshInterval);
            physicsClock = new Timer(interval.TotalSeconds);
            physicsClock.Elapsed += GlobalFixedUpdateRoot;
            PhysicsInitialized = true;
        }
        
        public StageAsset? GetStageAsset() => m_stageAsset;
        public void SetProject(Project project) =>  LoadedProject = project;

        public void ResetCurrentStage()
        {
            bool wasPaused = false;
            if (IsRunning)
            {
                wasPaused = true;
                Toggle();
            }
            SetStage(m_stageAsset?.Copy());
            if (!IsRunning && wasPaused) Toggle();
        }
        public void RaiseInspectorEvent(InspectorEvent e) => InspectorEventRaised?.Invoke(e);
        private protected void SetStage(Stage? value) 
        {
            if (IsRunning) Toggle();
            m_stage = null; 
            m_stage = value;
            if (!IsRunning) Toggle();

        }
        public void AddStageToProject(StageAsset stageAsset)
        {
            if (LoadedProject is null) 
                throw new NullReferenceException("Loaded Project reference to a null instance of an object");
            
            LoadedProject.stages ??= new();
            LoadedProject.stages.Add(stageAsset);

            SetStageAsset(stageAsset);
        }
        public void TrySetStageAsset(int stageAssetIndex)
        {
            if (LoadedProject is null) return;
            if (LoadedProject.stages is null) return;
            if (LoadedProject.stages.Count <= stageAssetIndex) return;
            if (LoadedProject.stages[stageAssetIndex] is null) return;
                SetStageAsset(LoadedProject.stages[stageAssetIndex]);
        }
        public void SetStageAsset(StageAsset stageAsset)
        {
            if (IsRunning) Toggle();
            m_stageAsset = stageAsset;
            if (!IsRunning) Toggle();
        }
        public void GlobalFixedUpdateRoot(object? sender, EventArgs e)
        {
            Task.Run(() => Collision.Run());
            StagingHost.Update(GetStage());
        }
        public void GlobalUpdateRoot(object? sender, EventArgs e)
        {
            if (!IsRunning ||  renderHost.State is RenderState.Off)  return; 
            if (renderHost.State is RenderState.Error)  throw new Exception("Rendering error");
            if (renderHost.State is RenderState.Game)   renderHost.Render(mainWnd.renderImage, this);
            Input.Refresh();
        }
    }
}