using pixel_renderer.Assets;
using pixel_renderer.FileIO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public Project LoadedProject;

        public static Runtime Current
        {
            get
            {
                if (current is null)
                    throw new EngineInstanceException("The runtime domain is not yet initialized");
                return current;
            }
        }
        private protected volatile static Runtime? current;
        private protected volatile Stage? stage;

        public static event Action<EditorEvent>? InspectorEventRaised;
        public static event Action<Project> OnProjectSet = new(delegate { });
        public static event Action<Stage> OnStageSet = new(delegate { });
        public static List<Image> OutputImages = new();
        
        private protected Thread renderThread;
        private BackgroundWorker physicsWorker;
     
        public static object? Editor = null;

        public static bool IsRunning { get; private set; }
        public static bool PhysicsStopping { get; private set; }
        public static bool PhysicsInitialized { get; private set; }
        public static bool Initialized { get; private set; }
        public static bool IsTerminating { get; private set; }

        private Runtime(EngineInstance mainWnd, Project project)
        {
            current = this;
            this.mainWnd = mainWnd;
            Importer.Import(true);

            LoadedProject = project;

            renderHost = new();
            renderThread = new(OnRenderTick);
            renderThread.Start();

            mainWnd.Closing += (e, o) => Dispose();
            
            Initialized = true;
            Project.LoadStage(0);
            Current.stage?.Awake();
        }

        public static void TogglePhysics()
        {
            if (!PhysicsStopping)
            {
                PhysicsStopping = true;
                return;
            }
            PhysicsStopping = false;
            Current.InitializePhysics();
        }
        public static void ToggleRendering()
        {
            if (IsRunning)
            {
                IsRunning = false;
                return;
            }
            IsRunning = true;
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
        private void OnRenderTick()
        {
            while (renderThread != null && renderThread.IsAlive)
            {
               
                if (IsTerminating)
                    return;
                CMouse.Update();
                if (IsRunning)
                {
                        StagingHost.Update(stage);
                        renderHost?.Render();
                    if (Application.Current is null)
                        return;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CMouse.MouseWheelDelta = 0;

                        if (OutputImages.Count == 0 || OutputImages.First() is null) return;
                        var renderer = renderHost.GetRenderer();
                        CBit.RenderFromFrame(renderer.Frame, renderer.Stride, renderer.Resolution, OutputImages.First());
                    });
                    Thread.Sleep(1);
                }
            }
        }
        private void OnPhysicsTick(object sender, DoWorkEventArgs e)
        {
            while (!PhysicsStopping)
            {
                if (IsTerminating)
                    return;
                CMouse.Update();
                if (stage is null)
                    continue;
                if (!IsRunning) 
                    continue;

                Collision.Run();
                StagingHost.FixedUpdate(stage);
                if (Application.Current == null)
                    return;
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Input.Refresh();
                });
                Thread.Sleep(16);  // Wait for 16ms to maintain 60fps
            }
        }
        /// <summary>
        /// this is a method since it has to be initialized externally but the fields are hidden.
        /// </summary>
        /// <param name="mainWnd"></param>
        /// <param name="project"></param>
        public static void Initialize(EngineInstance mainWnd, Project project)
        {
            current ??= new(mainWnd, project);
        }
        private void InitializePhysics()
        {
            PhysicsInitialized = true;
            physicsWorker ??= new BackgroundWorker();
            physicsWorker.DoWork += OnPhysicsTick;
            physicsWorker.RunWorkerAsync();
        }
        public void SetProject(Project project)
        {
            LoadedProject = project;
            OnProjectSet?.Invoke(project);
        }
        internal protected static Stage InstantiateDefaultStageIntoProject()
        {
            Log("No stage found, either the requested index was out of range or no stages were found in the project." +
                " A Default will be instantiated and added to the project at the requested index.");

            Stage stage = Stage.Standard();

            Current.LoadedProject.AddStage(stage);

            StageIO.WriteStage(stage);

            return stage; 
        }
        public Stage? GetStage()
        {
            return stage;
        }
        public void SetStage(Stage stage) 
        {
            this.stage = stage;
            OnStageSet?.Invoke(stage);
        }
        private void Dispose()
        {
            IsTerminating = true;
            PhysicsStopping = true;
            Task.Run(()=> renderThread?.Join());
            renderThread = null;
        }
    }
}