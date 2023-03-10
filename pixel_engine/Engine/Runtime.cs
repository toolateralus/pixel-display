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

namespace pixel_renderer
{
    public class Runtime
    {
        public RenderHost renderHost;
        public StagingHost stagingHost = new();
        public Project project;
        public static Runtime Current
        {
            get
            {
                if (current is null)
                    throw new EngineInstanceException("The runtime domain is not yet initialized");
                return current;
            }
        }
        public static void GetRenderingData(out RenderHost host, out StageRenderInfo info, out RendererBase renderer, out JImage baseImage)
        {
            host = Runtime.Current.renderHost;
            info = Current.stage.StageRenderInfo; 
            renderer = Runtime.Current.renderHost.GetRenderer();
            baseImage = renderer.baseImage;
        }

        private protected volatile static Runtime? current;
        private protected volatile Stage? stage;
        private protected volatile Thread renderThread; 
        public static event Action<EditorEvent>? InspectorEventRaised;
        public static event Action<Project> OnProjectSet = new(delegate { });
        public static event Action<Stage> OnStageSet = new(delegate { });
        public static List<System.Windows.Controls.Image> OutputImages = new();
        public BackgroundWorker physicsWorker; 

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
            renderThread = new(OnRenderBegin);
            Task.Run(() => renderThread.Start());

            physicsWorker = new();
            physicsWorker.DoWork += OnPhysicsBegin;
            physicsWorker.RunWorkerAsync();

            Initialized = true;
            Project.LoadStage(0);
            GetProjectSettings();

        }

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

            renderHost.newResolution = projectSettings.CurrentResolution; 
        }

        public ProjectSettings projectSettings;

        private void OnPhysicsBegin(object? sender, DoWorkEventArgs e)
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
        private static void OnRenderBegin()
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
            Runtime.Log("renderer has exited unexpectedly.");

        }
        public static void Initialize(Project project)
        {
            current ??= new(project);
        }
        public void SetProject(Project project)
        {
            this.project = project;
            OnProjectSet?.Invoke(project);
        }
        internal protected static Stage InstantiateDefaultStageIntoProject()
        {
            Log("No stage found, either the requested index was out of range or no stages were found in the project." +
                " A Default will be instantiated and added to the project at the requested index.");
            Stage stage = Stage.Standard();
            Current.project.AddStage(stage);
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
        public void Dispose()
        {
            IsDiposing = true;
            IsRunning = false;
            Task.Run(()=> renderThread?.Join());
            renderThread = null;
            current = null;
            stage = null;
            project = null;
        }
    }
}