using pixel_core.Assets;
using pixel_core.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using pixel_core.Statics;
using pixel_core.types.physics;
using System.Reflection;
using System.ComponentModel;
using Component = pixel_core.types.Components.Component;
using Microsoft.Win32;
using System.Windows.Input;

namespace pixel_core
{
    public class Runtime
    {
        #region General
        public void GetAllTypes()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type[] types = assembly.GetTypes();

            Interop.GetAllTypes();

            foreach (Type type in types.Where(t => t.IsSubclassOf(typeof(Component))))
                AllComponents.Add(type);

           AllComponents = AllComponents.Concat(Interop.AllComponents).ToList();
        }
        public static void Initialize(Project project)
        {
            current ??= new(project);
        }
        public void Dispose()
        {
            IsDiposing = true;
            IsRunning = false;
            Task.Run(() => renderThread?.Join());
        }
        #endregion
        #region Fields and Properties
        public static Runtime Current
        {
            get
            {
                if (current is null)
                    throw new EngineInstanceException("The runtime domain is not yet initialized");
                return current;
            }
        }
        public RenderHost renderHost;
        public StagingHost stagingHost = new();
        public Project project;
        public LUA Lua = new();
        public ProjectSettings projectSettings;
        
        private protected volatile static Runtime? current;
        private protected volatile Stage? stage;
        private protected Thread renderThread;
        private protected BackgroundWorker physicsWorker;
        
        public static List<System.Windows.Controls.Image> OutputImages = new();
        public static List<Type> AllComponents = new();

        public static event Action<EditorEvent>? InspectorEventRaised;
        public static event Action<Project> OnProjectSet = new(delegate { });
        public static event Action<Stage> OnStageSet = new(delegate { });
        public object? Inspector { get; set; } = null;
        public static bool Initialized { get; private set; } = false;
        /// <summary>
        /// Setting this to true will toggle physics
        /// </summary>
        public static bool IsRunning { get; private set; } = false;
        /// <summary>
        /// Setting this true will shut down the engine.
        /// </summary>
        public static bool IsDiposing { get; private set; } = false;
        #endregion
        #region Project/Stage Functions
        public static void SetAndLoadBackground(Metadata meta)
        {
            if (meta is null)
                return;

            Stage? stage1 = Current.GetStage();

            if (stage1 is null) return;
            
            stage1.backgroundMetadata = meta;
            var bmp = new Bitmap(meta.Path);

            Current.GetStage()?.SetBackground(bmp);
            Current.renderHost.GetRenderer().baseImageDirty = true;
        }
        public void SetProject(Project project)
        {
            this.project = project;
            OnProjectSet?.Invoke(project);
            project.TryLoadStage(0);
        }
        public Stage? GetStage()
        {
            return stage;
        }
        public void SetStage(Stage stage)
        {
            if (stage is null)
                return;

            this.stage = stage;
            OnStageSet?.Invoke(stage);
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
            SetResolution();
        }
        internal protected static Stage InstantiateDefaultStageIntoProject()
        {
            Log("No stage found, either the requested index was out of range or no stages were found in the project." +
                " A Default will be instantiated and added to the project at the requested index.");

            Stage stage = StagingHost.Standard();
            Current.project.AddStage(stage);
            return stage;
        }
        public static async Task<Metadata> GetSelectedFileMetadataAsync()
        {
            EditorEvent e = new(EditorEventFlags.GET_FILE_VIEWER_SELECTED_METADATA);
            object? asset = null;
            e.action = (e) => { asset = e.First(); };
            RaiseInspectorEvent(e);

            float time = 0;
            const float timeOut = 250;
            const int duration = 15;

            while (!e.processed && time < timeOut)
            {
                if (asset != null && asset is Metadata meta)
                    return meta;
                time += duration;
                await Task.Delay(duration);
            }
            return null;

        }
        #endregion
        #region Rendering Functions
        public void SetResolution() => renderHost.newResolution = ProjectSettings.CurrentResolution;
        public static void GetRenderingData(out RenderHost host, out StageRenderInfo info, out RendererBase renderer, out JImage baseImage)
        {
            host = Current.renderHost;
            info = Current.stage.StageRenderInfo;
            renderer = Current.renderHost.GetRenderer();
            baseImage = renderer.baseImage;
        }
        private static void RenderLoop()
        {
            while (Current.renderThread != null && Current.renderThread.IsAlive)
            {
                if (IsDiposing)
                    return;

                

                CMouse.Update();

                if (Application.Current is null)
                    return;


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
                        continue;
                    StagingHost.Update(Current.stage);
                }
            }

            if (IsRunning)
                Log("renderer has exited unexpectedly.");

        }
        #endregion
        #region Physics Functions
        private void PhysicsLoop(object? sender, DoWorkEventArgs e)
        {
            while (physicsWorker != null)
            {
                if (IsDiposing)
                    return;


                if (IsRunning)
                {
                    if (Current.stage is null)
                        continue;

                    StagingHost.FixedUpdate(Current.stage);

                    Physics.Step();

                    if (Application.Current is null)
                        return;

                    Thread.Sleep(ProjectSettings.PhysicsTimeStep);
                }
            }
            Log("Physics simulation has ended.");

        }
        public static void Toggle()
        {
            if (IsRunning)
            {
                IsRunning = false;
                return;
            }
            IsRunning = true;
            if (Current.GetStage() is Stage stage)
                Log($"Stage is fully awake: {stage.Awake()}");

        }
        #endregion
        #region Editor Events
        public static void Log(object obj, bool includeDateTime = false, bool clearConsole = false)
        {
            EditorEvent e = new(EditorEventFlags.PRINT, obj.ToString(), includeDateTime, clearConsole);
            InspectorEventRaised?.Invoke(e);
        }
        public static void Error(object obj)
        {
            EditorEvent e = new(EditorEventFlags.DO_NOT_PRINT | EditorEventFlags.PRINT_ERR, obj.ToString(), true, false);
            InspectorEventRaised?.Invoke(e);
        }
        public static void RaiseInspectorEvent(EditorEvent e)
        {
            InspectorEventRaised?.Invoke(e);
        }

        #endregion
        #region Interop Wrappers

        private Project Interop_OnProjectGotten()
        {
            return project;
        }

        private void Interop_OnDrawLine(System.Numerics.Vector2 arg1, System.Numerics.Vector2 arg2, Pixel arg3)
        {
            ShapeDrawer.DrawLine(arg1, arg2, arg3);
        }

        private void Interop_OnDrawCircle(System.Numerics.Vector2 arg1, int arg2, Pixel arg3)
        {
            ShapeDrawer.DrawCircle(arg1, arg2, arg3);
        }

        private void Interop_OnDrawGraphicsFinalize(RendererBase arg1, System.Numerics.Matrix3x2 arg2, System.Numerics.Matrix3x2 arg3)
        {
            ShapeDrawer.DrawGraphics(arg1, arg2, arg3);
        }

        private bool Interop_OnIsInitializedQuery()
        {
            return Initialized;
        }

        private bool Interop_OnIsRunningQuery()
        {
            return IsRunning;
        }
        #endregion
        #region Ctor
        private Runtime(Project project)
        {
            // for the editor.
            Interop.OnImport += GetAllTypes;
            GetAllTypes();

            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            current = this;
            this.project = project;

            Interop.OnIsRunningQuery += Interop_OnIsRunningQuery;
            Interop.OnIsInitializedQuery += Interop_OnIsInitializedQuery;
            Interop.OnDrawGraphicsFinalize += Interop_OnDrawGraphicsFinalize;
            Interop.OnDrawCircle += Interop_OnDrawCircle;
            Interop.OnDrawLine += Interop_OnDrawLine;
            Interop.OnProjectGotten += Interop_OnProjectGotten;
            Interop.OnProjectSet += SetProject;
            Interop.OnStageGotten += GetStage;
            Interop.OnStageSet += SetStage;
            Interop.OnFileViewer_SelectedMetadata_Query += GetSelectedFileMetadataAsync;
            Interop.OnEditorEventRaised += RaiseInspectorEvent;
            Interop.OnDefaultStageRequested += () => SetStage(StagingHost.Standard());

            renderHost = new();
            renderThread = new(RenderLoop);
            Task.Run(() => renderThread.Start());

            physicsWorker = new();
            physicsWorker.DoWork += PhysicsLoop;
            physicsWorker.RunWorkerAsync();

            Initialized = true;
            project.TryLoadStage(0);
            GetProjectSettings();

            Importer.Import();
        }
        #endregion

    }
}