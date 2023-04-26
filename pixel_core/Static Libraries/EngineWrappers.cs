using pixel_core.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace pixel_core
{
    public class Interop
    {

        #region Wrapper Actions/Functions
        public static event Action<Stage>? OnStageSet;
        public static event Func<Stage>? OnStageGotten;
        
        public static event Action<Project>? OnProjectSet;
        public static event Func<Project>? OnProjectGotten;
        
        public static event Action<EditorEvent>? OnEditorEventRaised;
        public static event Func<Task<Metadata>>? OnFileViewer_SelectedMetadata_Query;

        public static event Action<Vector2, int, Pixel>? OnDrawCircle;
        public static event Action<Vector2, Vector2, Pixel>? OnDrawLine;
        public static event Action<RendererBase, Matrix3x2, Matrix3x2>? OnDrawGraphicsFinalize;

        public static event Func<bool>? OnIsRunningQuery;
        public static event Func<bool>? OnIsInitializedQuery;

        #endregion

        public static bool IsRunning { get => OnIsRunningQuery.Invoke(); }
        public static bool Initialized { get => OnIsInitializedQuery.Invoke(); }
        public static Project Project 
        {
            get => OnProjectGotten?.Invoke();
            set => OnProjectSet?.Invoke(value); 
        }
        // these will have to be wrappers for functions in the engine
        public static Stage? GetStage()
        {
            return OnStageGotten?.Invoke();
        }
        internal static void SetStage(Stage stage)
        {
            OnStageSet?.Invoke(stage);
        }
        public static void Log(object obj, bool includeDateTime = false, bool clearConsole = false)
        {
            EditorEvent e = new(EditorEventFlags.PRINT, obj.ToString(), includeDateTime, clearConsole);
            RaiseInspectorEvent(e);
        }
        internal static Task<Metadata> GetSelectedFileMetadataAsync()
        {
            return OnFileViewer_SelectedMetadata_Query?.Invoke(); 
        }

        internal static void RaiseInspectorEvent(EditorEvent e)
        {
            OnEditorEventRaised?.Invoke(e);
        }

        public static void Error(object obj, bool includeDateTime = false, bool clearConsole = false)
        {
            EditorEvent e = new(EditorEventFlags.PRINT_ERR, obj.ToString(), includeDateTime, clearConsole);
            RaiseInspectorEvent(e);
        }
        public static void DrawLine(Vector2 a, Vector2 b, Pixel color)
        {
            OnDrawLine?.Invoke(a, b, color);
        }

        internal static void DrawCircle(Vector2 vector2, int v, Pixel blue)
        {
            OnDrawCircle?.Invoke(vector2, v, blue);
        }

        internal static void DrawGraphics(RendererBase renderer, Matrix3x2 matrix3x2, Matrix3x2 projectionMat)
        {
            OnDrawGraphicsFinalize?.Invoke(renderer, matrix3x2, projectionMat);
        }
    }
       
}
