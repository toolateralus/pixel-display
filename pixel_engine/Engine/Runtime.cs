﻿using pixel_renderer.Assets;
using pixel_renderer.FileIO;
using System;
using System.Collections.Generic;
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
        private StageAsset? m_stageAsset;
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
            Importer.Import(false);

            AssetLibrary.Fetch(out StageAsset fetched_asset);
            List<StageAsset> found_stages = new List<StageAsset>();
            StageAsset? stageAsset = null;

            if (fetched_asset == null)
            {
                foreach (var sMeta in project.stagesMeta)
                {
                    int stageCt = 0;
                    AssetIO.ReadAndRegister(out Asset? registeredStage, sMeta);
                    if (registeredStage != null)
                    {
                        stageCt++;
                        Log($"{registeredStage.Name}  + {stageCt}");
                        found_stages.Add((StageAsset)registeredStage);
                    }
                }
                if (found_stages == null || found_stages.Count == 0)
                {
                    Log("No stage data was read from the project selected - a default stage has been instantiated.");
                    Instance.SetStageAsset(StageAsset.Default);
                    return;
                }
                stageAsset = found_stages[0];
               
            }
            else found_stages[0] = fetched_asset;
            
            if (stageAsset is null)
                throw new NullStageException($"{stageAsset.Name} not found.");

            Instance.SetStageAsset(stageAsset);
            Log($"Stage asset {stageAsset.Name} loaded.");
        }
        private void InitializePhysics()
        {
            PhysicsInitialized = true;
        }
        public StageAsset? GetStageAsset() => m_stageAsset;
        public void SetProject(Project project) => LoadedProject = project;
        public void ReloadStage()
        {
            if (m_stageAsset is null) throw new Exception("Stage asset NULL");
            m_stage = m_stageAsset.Copy();
            renderHost.MarkDirty();
        }


        /// <summary>
        /// Prints a message in the editor console.
        /// </summary>
        /// <param name="message"></param>
        public static void Log(object obj, bool includeDateTime = false, bool clearConsole = false)
        {
           EditorEvent e = new(obj.ToString() ?? "", includeDateTime, clearConsole);
           RaiseInspectorEvent(e);
        }
        public static void RaiseInspectorEvent(EditorEvent e) => InspectorEventRaised?.Invoke(e);

        public void AddStageToProject(StageAsset stageAsset)
        {
            if (LoadedProject is null) 
                throw new NullReferenceException("Loaded Project reference to a null instance of an object");
            
            LoadedProject.stages ??= new();
            LoadedProject.stages.Add(stageAsset);
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
            ReloadStage();
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