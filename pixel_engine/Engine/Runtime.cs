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

        public static List<Image> OutputImages = new();

        public bool IsRunning { get; private set; }
        public static bool PhysicsInitialized { get; private set; }
        public static bool Initialized { get; private set; }
        public static object? Editor = null;

        internal int selectedStage = 0;
        
        private protected volatile Stage? stage;
        private protected volatile static Runtime? instance;
        public static Runtime Current
        {
            get
            {
                if (instance is null)
                    throw new EngineInstanceException("The runtime domain is not yet initialized");
                return instance;
            }
        }

        public bool IsTerminating { get; private set; }

        public static event Action<EditorEvent>? InspectorEventRaised;
        public static event Action<Project> OnProjectSet = new(delegate { });
        public static event Action<Stage> OnStageSet = new(delegate { });
        private protected Thread renderThread;
        private BackgroundWorker _worker;
        private bool _stopWorker = false;

        private Runtime(EngineInstance mainWnd, Project project)
        {
            instance = this;
            this.mainWnd = mainWnd;
            this.LoadedProject = project;
            Initialized = true;

            renderHost = new();

            renderThread = new(OnRenderTick);
            renderThread.Start();


            mainWnd.Closing += (e, o) =>
            {
                Dispose();
            };
        }


        public void Toggle()
        {
            if (IsRunning)
            {
                IsRunning = false;
                return;
            }
            IsRunning = true; 
            if (!PhysicsInitialized)
                InitializePhysics();


        }

        public static void Initialize(EngineInstance mainWnd, Project project)
        {
            instance ??= new(mainWnd, project);
            TryLoadStageFromProject(0);
            Current.stage?.Awake();
            Log($"{Current.GetStage().Name} instantiated & engine started.");

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
                if (IsRunning)
                {
                        StagingHost.Update(stage);
                        renderHost?.Render();
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (OutputImages.Count == 0 || OutputImages.First() is null) return;
                            var renderer = renderHost.GetRenderer();
                            CBit.RenderFromFrame(renderer.Frame, renderer.Stride, renderer.Resolution, OutputImages.First());
                        });
                    Thread.Sleep(1);
                }
            }
        }
        private void InitializePhysics()
        {
            PhysicsInitialized = true;
            _worker = new BackgroundWorker();
            _worker.DoWork += OnPhysicsTick;
            _worker.RunWorkerAsync();
        }
        private void OnPhysicsTick(object sender, DoWorkEventArgs e)
        {
            while (!_stopWorker)
            {
                if (IsTerminating)
                    return;
                if (stage is null || !PhysicsInitialized)
                    continue;

                    Collision.Run();
                    StagingHost.FixedUpdate(stage);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        //TODO: Fix this hacky fix;
                        CMouse.MouseWheelDelta = 0;
                        Input.Refresh();
                    });
                Thread.Sleep(16);  // Wait for 16ms to maintain 60fps
            }
        }
        private void Dispose()
        {
            IsTerminating = true;
            _stopWorker = true;
            Task.Run(()=> renderThread?.Join());
            renderThread = null;
        }

        public void SetProject(Project project)
        {
            LoadedProject = project;
            OnProjectSet?.Invoke(project);
        }

        public static void TryLoadStageFromProject(int index)
        {
            List<Metadata> stagesMeta = Current.LoadedProject.stagesMeta;
            
            Stage stage;

            if (stagesMeta.Count - 1 > index)
            {
                Metadata stageMeta = stagesMeta[index];
                stage = StageIO.ReadStage(stageMeta);
            }
            else stage = InstantiateDefaultStageIntoProject();

            Current.SetStage(stage);
        }
        private static Stage InstantiateDefaultStageIntoProject()
        {
            Log("No stage found, either the requested index was out of range or no stages were found in the project." +
                " A Default will be instantiated and added to the project at the requested index.");

            Stage stage = Stage.Default();

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
    }
}