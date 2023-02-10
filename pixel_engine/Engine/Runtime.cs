using pixel_renderer.Assets;
using pixel_renderer.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using Timer = System.Timers.Timer;

namespace pixel_renderer
{
    public class Runtime
    {
        public EngineInstance mainWnd;
        public RenderHost renderHost = new();
        public StagingHost stagingHost = new();
        
        public Timer physicsClock;
        public Project LoadedProject;

        private protected volatile Stage? m_stage;
        
        public static event Action<EditorEvent>? InspectorEventRaised;

        public bool IsRunning { get; private set; }
        private protected bool PhysicsInitialized = false;
        private protected bool Initialized = false;
        
        public static object? inspector = null;

        private Runtime(EngineInstance mainWnd, Project project)
        {
            instance = this;
            this.mainWnd = mainWnd;
            this.LoadedProject = project;
            Initialized = true;

            var interval = TimeSpan.FromSeconds(Constants.PhysicsRefreshInterval);
            physicsClock = new Timer(interval.TotalSeconds);
            physicsClock.Elapsed += GlobalFixedUpdateRoot;


            CompositionTarget.Rendering += GlobalUpdateRoot;
            CompositionTarget.Rendering += Input.Refresh;
        }

        private protected volatile static Runtime? instance;
        public static Runtime Instance
        {
            get
            {
                if (instance is null)
                    throw new Exception("Runtime not initialized.");
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

        public Stage? GetStage()
        {
            return m_stage;
        }

        public static void Initialize(EngineInstance mainWnd, Project project)
        {
            instance ??= new(mainWnd, project);
            TryLoadStageFromProject();
        }

        public static void TryLoadStageFromProject()
        {
            List<Metadata> stagesMeta = Instance.LoadedProject.stagesMeta;
            List<Stage> stages = Instance.LoadedProject.stages;
            
            Stage stage;
            
            if (stagesMeta.Count <= 0)
            {
                stage = InstantiateDefaultStageIntoProject(stagesMeta, stages);
            }
            else
            {
                Metadata stageMeta = stagesMeta[0];
                stage = StageIO.ReadStage(stageMeta);
            }
            Instance.SetStage(stage);
        }

        private static Stage? InstantiateDefaultStageIntoProject(List<Metadata> stagesMeta, List<Stage> stages)
        {
            Log("No stages were found in the project. A Default will be instantiated and added to the project.");

            Stage stage = Stage.Default();
            if (stage != null)
            {
                stages.Add(stage);
                stage.Sync();
                stagesMeta.Add(stage.Metadata);
                StageIO.WriteStage(stage);
            }
            return stage; 
        }
        internal int selectedStage = 0;
        public void SetStage(Stage stage) {
            m_stage = stage;
        }

        private void InitializePhysics()
        {
            PhysicsInitialized = true;
        }
        public void SetProject(Project project) => LoadedProject = project;
        
       
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

        public void AddStageToProject(Stage stage)
        {
            if (LoadedProject is null) 
                throw new NullReferenceException("Loaded Project reference to a null instance of an object");
            
            LoadedProject.stages ??= new();
            LoadedProject.stages.Add(stage);
        }
      
        public void GlobalFixedUpdateRoot(object? sender, EventArgs e)
        {
            if(m_stage is null || !PhysicsInitialized) return;
            Task.Run(() => Collision.Run());
            StagingHost.Update(m_stage);
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