using pixel_renderer;
using System.Drawing;
using System.Numerics;

namespace pixel_editor
{
    public static class Constants
    {
        public const float MouseZoomSensitivityFactor = 1.001f;
        public static float DragCursorRadius { get; internal set; }
        
        public const int EditorEventQueueMaxLength = 12;
        public const int ConsoleMaxLines = 100; 
        public static int InspectorHeight = 10;
        public static int InspectorWidth = 6;

        public static Pixel DragCursorColor = Pixel.Green;
        public static Pixel DragBoxColor { get; internal set; }
        
        public static Vector2 MouseSensitivity = new(1.0f, 1.0f);
        public static Vector2 InspectorPosition = new(14, 3);
    }
}
