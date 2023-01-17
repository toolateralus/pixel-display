using pixel_renderer;
using pixel_renderer.Assets;
using System;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using Timer = System.Timers.Timer;

namespace pixel_renderer
{
    public class Runtime
    {
        public EngineInstance mainWnd;
        public RenderHost? renderHost = new();
        public StagingHost? stagingHost = new();
        
        public Timer? physicsClock;
        private StageAsset? m_stageAsset;
        public Project? LoadedProject = null;
        

        private protected volatile Stage? m_stage;
        
        public static event Action<EditorEvent> InspectorEventRaised;

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
            if (!PhysicsInitialized)
                InitializePhysics();

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
        public Stage GetStage()
        {
            if (m_stageAsset is null)
            {
                m_stageAsset = StageAsset.Default;
                AssetLibrary.Sync();
            }
            if (m_stage is null || m_stage.UUID != m_stageAsset.UUID)
                m_stage = m_stageAsset.Copy();
            return m_stage;
        }
        public static void Awake(EngineInstance mainWnd, Project? project = null)
        {
            if (project != null) 
                Instance.LoadedProject = project;

            Instance.mainWnd = mainWnd;
            Instance.Initialized = true;
           
            Importer.Import(false);

            CompositionTarget.Rendering += Instance.GlobalUpdateRoot;
            CompositionTarget.Rendering += Input.Refresh;
        }
        private void InitializePhysics()
        {
            var interval = TimeSpan.FromSeconds(Constants.PhysicsRefreshInterval);
            physicsClock = new Timer(interval.TotalSeconds);
            physicsClock.Elapsed += GlobalFixedUpdateRoot;
            PhysicsInitialized = true;
        }
        public StageAsset? GetStageAsset() => m_stageAsset;
        public void SetProject(Project project) => LoadedProject = project;
        public void ResetCurrentStage()
        {
            if(m_stageAsset != null)
                SetStage(m_stageAsset.Copy());
        }


        /// <summary>
        /// Prints a message in the editor console.
        /// </summary>
        /// <param name="message"></param>
        public static void Log(object obj, bool includeDateTime = false, bool clearConsole = false)
        {
           EditorEvent e = new(obj.ToString(), includeDateTime, clearConsole);
           RaiseInspectorEvent(e);
        }
        public static void RaiseInspectorEvent(EditorEvent e) => InspectorEventRaised?.Invoke(e);

        private protected void SetStage(Stage? value) 
        {
            m_stage = null;
            m_stage = value;
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
            m_stageAsset = stageAsset;
        }
        public void GlobalFixedUpdateRoot(object? sender, EventArgs e)
        {
            Task.Run(() => Collision.Run());
            StagingHost.Update(GetStage());
        }
        public void GlobalUpdateRoot(object? sender, EventArgs e)
        {

            bool HasNoRenderSurface = renderHost.State is RenderState.Off;

            if (!IsRunning || HasNoRenderSurface) 
                return;

            if (renderHost.State is RenderState.Game) 
                renderHost.Render(mainWnd.renderImage);

        }
    }
}