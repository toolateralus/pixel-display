using Pixel.FileIO;
using Pixel.Types.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;

namespace Pixel
{
    public class Interop
    {
        #region Wrapper Actions/Functions

        public static Action? OnImport;
        public static event Action? OnDefaultStageRequested;
        public static event Action<Stage>? OnStageSet;
        public static event Action<Project>? OnProjectSet;
        public static event Action<EditorEvent>? OnEditorEventRaised;
        public static event Action<Vector2, int, Color>? OnDrawCircle;
        public static event Action<Vector2, Vector2, Color>? OnDrawLine;
        public static event Action<RendererBase, Matrix3x2, Matrix3x2>? OnDrawGraphicsFinalize;
        public static event Action<Vector2, float, Color>? OnDrawCircleFilled;

        public static event Func<Project>? OnProjectGotten;
        public static event Func<Stage>? OnStageGotten;
        public static event Func<Task<Metadata>>? OnFileViewer_SelectedMetadata_Query;
        public static event Func<bool>? OnIsRunningQuery;
        public static event Func<bool>? OnIsInitializedQuery;
        public static List<Type> AllComponents = new();
        static Interop()
        {
            OnImport += GetAllTypes; 
        }

        public static void GetAllTypes()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type[] types = assembly.GetTypes();

            foreach (Type type in types.Where(t => t.IsSubclassOf(typeof(Component))))
                AllComponents.Add(type);
        }
        public static Stage? Stage
        {
            get
            {
                return OnStageGotten?.Invoke();
            }
            set 
            {
                if(value != null)
                    OnStageSet?.Invoke(value);
            }
        }

        public static void ClearConsole()
        {
            EditorEvent e = new(EditorEventFlags.CLEAR_CONSOLE);
            RaiseInspectorEvent(e);
        }
        public static void Log(object obj)
        {
            EditorEvent e = new(EditorEventFlags.PRINT, obj.ToString(), false);
            RaiseInspectorEvent(e);
        }
        internal static Task<Metadata> GetSelectedFileMetadataAsync()
        {
            return OnFileViewer_SelectedMetadata_Query?.Invoke();
        }
        internal protected static void InstantiateDefaultStageIntoProject()
        {
            Log("No stage found, either the requested index was out of range or no stages were found in the project." +
                " A Default will be instantiated and added to the project at the requested index.");

            OnDefaultStageRequested?.Invoke();
        }
        internal static void RaiseInspectorEvent(EditorEvent e)
        {
            OnEditorEventRaised?.Invoke(e);
        }
        public static void Error(object obj, bool includeDateTime = false)
        {
            EditorEvent e = new(EditorEventFlags.PRINT_ERR, obj.ToString(), includeDateTime);
            RaiseInspectorEvent(e);
        }
        public static void DrawLine(Vector2 a, Vector2 b, Color color)
        {
            OnDrawLine?.Invoke(a, b, color);
        }
        internal static void DrawCircle(Vector2 vector2, int v, Color blue)
        {
            OnDrawCircle?.Invoke(vector2, v, blue);
        }
        internal static void DrawGraphics(RendererBase renderer, Matrix3x2 matrix3x2, Matrix3x2 projectionMat)
        {
            OnDrawGraphicsFinalize?.Invoke(renderer, matrix3x2, projectionMat);
        }

        internal static void DrawCircleFilled(Vector2 endPt, float radius, Color currentColor)
        {
            OnDrawCircleFilled?.Invoke(endPt, radius, currentColor);
        }
        #endregion
        public static bool IsRunning
        {
            get
            {
                var x = OnIsRunningQuery?.Invoke();
                return x.HasValue && x.Value;
            }
        }
        public static bool Initialized
        {
            get
            {
                var x = OnIsInitializedQuery?.Invoke();
                return x.HasValue && x.Value;
            }
        }
        public static Project Project
        {
            get => OnProjectGotten?.Invoke();
            set => OnProjectSet?.Invoke(value);
        }
        public static float GetLastFrameTime { get; internal set; } = 0.1f;
    }

}
