using Pixel.Assets;
using Pixel.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using Pixel.Statics;
using Pixel.Types.Physics;
using System.Reflection;
using System.ComponentModel;
using Component = Pixel.Types.Components.Component;
using Pixel_Core.Types.Attributes;
using System.IO;

namespace Pixel
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
            {
                if (type.GetCustomAttributes(typeof(HideFromEditorAttribute), true).Any())
                    continue;
                AllComponents.Add(type);
            }

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
        
        public static List<Type> AllComponents = new();

        public static event Action<EditorEvent>? InspectorEventRaised;
        public static event Action<Project> OnProjectSet = new(delegate { });
        public static event Action<Stage> OnStageSet = new(delegate { });
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

            if (stage1 is null)
                 return;
            
            stage1.background  = new(new Color[25,25]);
            // TODO: set stage background to something/
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
            if (!project.stages.Contains(stage))
                project.AddStage(stage);
            this.stage = stage;
            OnStageSet?.Invoke(stage);
        }
        private void GetProjectSettings()
        {
            Metadata meta = new(Constants.WorkingRoot + "/obj/projectSettings.asset");

            var fetchedAsset = IO.ReadJson<ProjectSettings>(meta);

            if (fetchedAsset is null)
            {
                fetchedAsset = new("projectSettings", false);
                fetchedAsset.metadata = meta;
                IO.WriteJson(fetchedAsset, meta);
            }

            projectSettings = fetchedAsset;
            SetResolution();
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

                Current.renderHost?.Render();
                Input.Refresh();
                var renderer = Current.renderHost?.GetRenderer();
                
                CBit.RenderFromFrame(renderer.Frame, renderer.Stride, renderer.Resolution/* render data */);

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
                     
                    Current.stage.FixedUpdateMethod(0.1f);

                    Physics.Step();
                    
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
        }
        #endregion
        #region Editor Events
        public static void Log(object obj, bool includeDateTime = false)
        {
            EditorEvent e = new(EditorEventFlags.PRINT, obj.ToString(), includeDateTime);
            InspectorEventRaised?.Invoke(e);
        }
        public static void Error(object obj)
        {
            EditorEvent e = new(EditorEventFlags.PRINT_ERR, obj.ToString(), true);
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
        private void Interop_OnDrawLine(System.Numerics.Vector2 arg1, System.Numerics.Vector2 arg2, Color arg3)
        {
            ShapeDrawer.DrawLine(arg1, arg2, arg3);
        }
        private void Interop_OnDrawCircle(System.Numerics.Vector2 arg1, int arg2, Color arg3)
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
            Interop.OnDrawCircleFilled += (a, b, c) => ShapeDrawer.DrawCircleFilled(a, b, c);
            Interop.OnDrawCircle += Interop_OnDrawCircle;
            Interop.OnDrawLine += Interop_OnDrawLine;
            Interop.OnProjectGotten += Interop_OnProjectGotten;
            Interop.OnProjectSet += SetProject;
            Interop.OnStageGotten += GetStage;
            Interop.OnStageSet += SetStage;
            Interop.OnFileViewer_SelectedMetadata_Query += GetSelectedFileMetadataAsync;
            Interop.OnEditorEventRaised += RaiseInspectorEvent;
            Interop.OnDefaultStageRequested += StagingHost.AddStandard;
            Interop.OnStageAddedToProject += (stage) => project.AddStage(stage);

            renderHost = new();
            renderThread = new(RenderLoop);
            Task.Run(() => renderThread.Start());

            physicsWorker = new();
            physicsWorker.DoWork += PhysicsLoop;
            physicsWorker.RunWorkerAsync();

            Initialized = true;
            Importer.Import();

            project.TryLoadStage(0);
            GetProjectSettings();
        }
        #endregion

    }
}