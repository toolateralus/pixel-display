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

        public static object? inspector = null;
        public event Action<InspectorEvent> InspectorEventRaised;

        public RenderHost? renderHost = new();

        public Project? LoadedProject = null;
        public StagingHost? stagingHost = new();
        private StageAsset? m_stageAsset;
        public Stage? stage
        {
            get
            {
                if (m_stageAsset is null) m_stageAsset = StageAsset.Default;
                if (m_stage is null || m_stage.UUID != m_stageAsset.UUID) m_stage = m_stageAsset.Copy();
                return m_stage;
            }
            set => m_stage = value;
        }
        private Stage? m_stage;

        public bool PhysicsInitialized { get; private set; }
        public bool IsRunning = false;
        public bool Initialized { get; internal set; } = false;
        
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
        private protected static Runtime instance = new();

        public static async Task AwakeAsync(EngineInstance mainWnd, Project project)
        {
            Instance.LoadedProject = project;
            Instance.mainWnd = mainWnd;
            await Importer.ImportAsync(false);
            CompositionTarget.Rendering += Instance.GlobalUpdateRoot;
            Instance.Initialized = true;
        }
        public void SetProject(Project project)
        {
            LoadedProject = project;
            if (project.stages.Count <= 0)
                project.stages.Add(new("", StagingHost.Default()));

            SetStageAsset(project.stages[0]);
        }
        public void SetStageAsset(StageAsset stageAsset)
        {
            if (IsRunning) Toggle();
            m_stageAsset = stageAsset;
            _ = stage;
        }
        public StageAsset? GetStageAsset() => m_stageAsset;

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
        
        public void GlobalFixedUpdateRoot(object? sender, EventArgs e)
        {
            Collision.Run();
            StagingHost.Update(stage);
        }
        public void GlobalUpdateRoot(object? sender, EventArgs e)
        {
            if (!IsRunning ||  renderHost.State is RenderState.Off)  return; 
            if (renderHost.State is RenderState.Error)  throw new Exception("Rendering error");
            if (renderHost.State is RenderState.Game)   renderHost.Render(mainWnd.renderImage, this);
            Input.Refresh();
        }
        
        // NYI, probably wont be implemented as is anyway.
        public void RaiseInspectorEvent(InspectorEvent e) => InspectorEventRaised?.Invoke(e);
    }
}