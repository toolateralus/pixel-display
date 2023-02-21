using pixel_renderer.Assets;
using pixel_renderer.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using Timer = System.Timers.Timer;

namespace pixel_renderer
{
    public class Runtime
    {
        public EngineInstance mainWnd;
        public RenderHost renderHost;
        public StagingHost stagingHost = new();
        public static List<Image> OutputImages = new();
        public Timer physicsClock;
        public Project LoadedProject;

        public bool IsRunning { get; private set; }
        public static bool PhysicsInitialized = false;
        public static bool Initialized = false;
        public static object? inspector = null;
        internal int selectedStage = 0;
        
        private protected volatile Stage? m_stage;
        private protected volatile static Runtime? instance;
        public static Runtime Instance
        {
            get
            {
                if (instance is null)
                    throw new EngineInstanceException("The runtime domain is not yet initialized");
                return instance;
            }
        }

        public static event Action<EditorEvent>? InspectorEventRaised;
        public static event Action<Project> OnProjectSet = new(delegate { });
        public static event Action<Stage> OnStageSet = new(delegate { });
        private protected Thread renderThread;

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

            renderHost = new();

            renderThread = new(RenderTick);
            renderThread.Start();

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
        public static void Initialize(EngineInstance mainWnd, Project project)
        {
            instance ??= new(mainWnd, project);
            TryLoadStageFromProject(0);
            Instance.m_stage?.Awake();
            Log($"{Instance.GetStage().Name} instantiated & engine started.");

        }
        private void InitializePhysics()
        {
            PhysicsInitialized = true;
        }
        public static void TryLoadStageFromProject(int index)
        {
            List<Metadata> stagesMeta = Instance.LoadedProject.stagesMeta;
            
            Stage stage;

            if (stagesMeta.Count - 1 > index)
            {
                Metadata stageMeta = stagesMeta[index];
                stage = StageIO.ReadStage(stageMeta);
            }
            else stage = InstantiateDefaultStageIntoProject();

            Instance.SetStage(stage);
        }
        private static Stage InstantiateDefaultStageIntoProject()
        {
            Log("No stage found, either the requested index was out of range or no stages were found in the project." +
                " A Default will be instantiated and added to the project at the requested index.");

            Stage stage = Stage.Default();

            Instance.LoadedProject.AddStage(stage);

            StageIO.WriteStage(stage);

            return stage; 
        }

        public void SetStage(Stage stage) 
        {
            m_stage = stage;
            OnStageSet?.Invoke(stage);
        }
        public Stage? GetStage()
        {
            return m_stage;
        }
        public void SetProject(Project project)
        {
            LoadedProject = project;
            OnProjectSet?.Invoke(project);
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

        private void RenderTick()
        {
            while (renderThread.IsAlive)
                if (IsRunning)
                {
                    CompositionTarget.Rendering += OnRendering;
                    renderHost?.Render();
                    Thread.Sleep(1);
                }
        }
        private void OnRendering(object? sender, EventArgs e)
        {
            // Detach the rendering event so we don't get called again until the next frame
            CompositionTarget.Rendering -= OnRendering;

            // Update your UI as needed, using the Dispatcher to ensure it's done on the UI thread
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                if (OutputImages.Count == 0 || OutputImages.First() is null) return; 
                var renderer = renderHost.GetRenderer(); 
                CBit.RenderFromFrame(renderer.Frame, renderer.Stride, renderer.Resolution, OutputImages.First());
            });
        }

        public void GlobalFixedUpdateRoot(object? sender, EventArgs e)
        {
            if(m_stage is null || !PhysicsInitialized) 
                return;
            
            
            Task collisionTask = new(delegate { Collision.Run(); });
            Task stageUpdateTask = new(delegate { StagingHost.FixedUpdate(m_stage); });

            collisionTask.Start();
            collisionTask.Wait();

            stageUpdateTask.Start();
            stageUpdateTask.Wait();

        }
        public void GlobalUpdateRoot(object? sender, EventArgs e)
        {
            if (!IsRunning || m_stage is null) 
                return;

            Task stageUpdateTask = new( delegate
            {  StagingHost.Update(m_stage); });
               
            stageUpdateTask.Start();
            stageUpdateTask.Wait();
        }
    }
}