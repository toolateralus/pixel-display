﻿using pixel_renderer.Assets;
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
        private protected static BackgroundWorker physicsWorker;
     
        public static object? Editor = null;

        public static bool IsRunning { get; private set; }
        public static bool PhysicsStopping { get; private set; }
        public static bool physicsRunning { get; private set; }
        public static bool Initialized { get; private set; }
        public static bool IsTerminating { get; private set; }

        private Runtime(EngineInstance mainWnd, Project project)
        {
            current = this;
            this.mainWnd = mainWnd;

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
            if (!physicsRunning)
            {
                physicsRunning = true;
                StartPhysicsWorker();
                return;
            }
            physicsRunning = false;
            StopPhysicsWorker();
        }

        private static void StopPhysicsWorker()
        {
            physicsWorker.DoWork -= OnPhysicsTick;
            physicsWorker.Dispose();
        }

        private static void StartPhysicsWorker()
        {
            physicsWorker ??= new BackgroundWorker();
            physicsWorker.DoWork += OnPhysicsTick;
            physicsWorker.RunWorkerAsync();
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
        private static void OnRenderTick()
        {
            while (Current.renderThread != null && Current.renderThread.IsAlive)
            {
                if (IsTerminating)
                    return;

                CMouse.Update();

                if (Application.Current is null)
                    return;

                Application.Current.Dispatcher.Invoke(() => { Input.Refresh(); });
                CMouse.MouseWheelDelta = 0;

                if (IsRunning)
                {
                    StagingHost.Update(Current.stage);
                    Current.renderHost?.Render();

                    if (Application.Current is null)
                        return;

                    var renderer = Current.renderHost?.GetRenderer();

                    if (OutputImages.Count == 0 || OutputImages.First() is null || renderer is null)
                        continue;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        CBit.RenderFromFrame(renderer.Frame, renderer.Stride, renderer.Resolution, OutputImages.First());
                    });
                    Thread.Sleep(1);
                }
            }
        }
        private static void OnPhysicsTick(object sender, DoWorkEventArgs e)
        {
            while (physicsRunning)
            {
                if (IsTerminating)
                    return;
                if (Application.Current == null)
                    return;
                
                Thread.Sleep(Constants.PhysicsIntervalMs);

                if (!IsRunning)
                    continue;

                if (Current.stage is null)
                    continue;

                StagingHost.FixedUpdate(Current.stage);
                Collision.Run();
            }
        }
        public static void Initialize(EngineInstance mainWnd, Project project)
        {
            current ??= new(mainWnd, project);
        }
        private void InitializePhysics()
        {
         
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