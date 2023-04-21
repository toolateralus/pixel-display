using pixel_renderer.Assets;
using pixel_renderer.FileIO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace pixel_renderer
{
    public class Runtime
    {

        private protected volatile static Runtime? current;
        private protected volatile Stage? stage;
        private protected Thread renderThread; 
        private protected BackgroundWorker physicsWorker; 
        public static event Action<EditorEvent>? InspectorEventRaised;
        public static event Action<Project> OnProjectSet = new(delegate { });
        public static event Action<Stage> OnStageSet = new(delegate { });
        public static List<System.Windows.Controls.Image> OutputImages = new();
        public static Runtime Current
        {
            get
            {
                if (current is null)
                    throw new EngineInstanceException("The runtime domain is not yet initialized");
                return current;
            }
        }
       
        public object? Inspector = null;
        public static bool Initialized { get; private set; }
        public static bool IsRunning { get; private set; }
        public static bool IsDiposing { get; private set; }
        
        private Runtime(Project project)
        {
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            current = this;
            this.project = project;


            renderHost = new();
            renderThread = new(RenderLoop);
            Task.Run(() => renderThread.Start());

            physicsWorker = new();
            physicsWorker.DoWork += PhysicsLoop;
            physicsWorker.RunWorkerAsync();

            Initialized = true;
            Project.TryLoadStage(0);
            GetProjectSettings();

        }
        public RenderHost renderHost;
        public StagingHost stagingHost = new();
        public Project project;
        public LuaInterop Lua = new();
        public ProjectSettings projectSettings;
        
        private void GetProjectSettings()
        {
            Metadata meta = new("projectSettings", Constants.WorkingRoot + "\\projectSettings.asset", ".asset");

            var fetchedAsset = IO.ReadJson<ProjectSettings>(meta);

            if (fetchedAsset is null)
            {
                fetchedAsset = new();
                IO.WriteJson(fetchedAsset, meta);
            }

            projectSettings = fetchedAsset;
            SetResolution();
        }

        public void SetResolution() => renderHost.newResolution = projectSettings.CurrentResolution;
        public static void GetRenderingData(out RenderHost host, out StageRenderInfo info, out RendererBase renderer, out JImage baseImage)
        {
            host = Runtime.Current.renderHost;
            info = Current.stage.StageRenderInfo; 
            renderer = Runtime.Current.renderHost.GetRenderer();
            baseImage = renderer.baseImage;
        }
        private void PhysicsLoop(object? sender, DoWorkEventArgs e)
        {
            while (physicsWorker != null)
            {
                if (IsDiposing)
                    return;

                CMouse.MouseWheelDelta = 0;

                if (IsRunning)
                {
                    if (Current.stage is null)
                        continue;

                    StagingHost.FixedUpdate(Current.stage);

                    Physics.Step();

                    if (Application.Current is null)
                        return;

                    Thread.Sleep(projectSettings.PhysicsTimeStep);
                }
            }
            Runtime.Log("Physics simulation has ended.");

        }
        private static void RenderLoop()
        {
            while (Current.renderThread != null && Current.renderThread.IsAlive)
            {
                if (IsDiposing)
                    return;

                if (Application.Current is null)
                    return;
                CMouse.Update();
                Current.renderHost?.Render();
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    if (Application.Current is null)
                        return;
                    Input.Refresh();
                    var renderer = Current.renderHost?.GetRenderer();
                    if (OutputImages.Count == 0 || OutputImages.First() is null || renderer is null)
                        return;
                    Application.Current.Dispatcher.Invoke(() =>
                        CBit.RenderFromFrame(renderer.Frame, renderer.Stride, renderer.Resolution, OutputImages.First()));
                });

                if (IsRunning)
                {
                    if (Current.stage is null) 
                        return; 
                    StagingHost.Update(Current.stage);
                }
            }
            if(IsRunning) Log("renderer has exited unexpectedly.");

        }

        public static void Toggle()
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
           InspectorEventRaised?.Invoke(e);
        }
        public static void RaiseInspectorEvent(EditorEvent e)
        {
            InspectorEventRaised?.Invoke(e);
        }
        public static void Initialize(Project project)
        {
            current ??= new(project);
        }
        public void SetProject(Project project)
        {
            this.project = project;
            OnProjectSet?.Invoke(project);
            Project.TryLoadStage(0);
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
        public void Dispose()
        {
            IsDiposing = true;
            IsRunning = false;
            Task.Run(()=> renderThread?.Join());
        }
        internal protected static Stage InstantiateDefaultStageIntoProject()
        {
            Log("No stage found, either the requested index was out of range or no stages were found in the project." +
                " A Default will be instantiated and added to the project at the requested index.");

            Stage stage = Stage.Standard();
            Current.project.AddStage(stage);
            return stage; 
        }
    }
}