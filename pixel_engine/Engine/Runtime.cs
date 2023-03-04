﻿using pixel_renderer.Assets;
using pixel_renderer.FileIO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime;
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
        private protected volatile static Runtime? current;
        private protected volatile Stage? stage;
        private protected volatile Thread renderThread; 
        public static event Action<EditorEvent>? InspectorEventRaised;
        public static event Action<Project> OnProjectSet = new(delegate { });
        public static event Action<Stage> OnStageSet = new(delegate { });
        public static List<Image> OutputImages = new();
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
            renderThread = new(OnRenderTick);
            Task.Run(() => renderThread.Start());
            Initialized = true;
            Project.LoadStage(0);
            Current.stage?.Awake();
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
           RaiseInspectorEvent(e);
        }
        public static void RaiseInspectorEvent(EditorEvent e) => InspectorEventRaised?.Invoke(e);
        private static void OnRenderTick()
        {
            while (Current.renderThread != null && Current.renderThread.IsAlive)
            {
                if (IsDiposing)
                    return;

               CMouse.Update();

                if (Application.Current is null)
                    return;

                Application.Current.Dispatcher.Invoke(() => { Input.Refresh(); });

                CMouse.MouseWheelDelta = 0;

                if (IsRunning)
                {
                    if (Current.stage is null) 
                        return; 

                    StagingHost.Update(Current.stage);
                    StagingHost.FixedUpdate(Current.stage);
                    Collision.Run();
                    Current.renderHost?.Render();

                    if (Application.Current is null)
                        return;

                    var renderer = Current.renderHost?.GetRenderer();

                    if (OutputImages.Count == 0 || OutputImages.First() is null || renderer is null)
                        continue;

                    Task.Run(()=>
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            CBit.RenderFromFrame(renderer.Frame, renderer.Stride, renderer.Resolution, OutputImages.First());
                    }));
                }
            }
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