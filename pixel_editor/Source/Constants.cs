using pixel_renderer;
using System.Drawing;
using System.Numerics;

namespace pixel_editor
{
    public static class Constants
    {
        public const float MouseZoomSensitivityFactor = 1.001f;
        
        public const int EditorEventQueueMaxLength = 12;
        public const int ConsoleMaxLines = 100; 
        public static int InspectorHeight = 10;
        public static int InspectorWidth = 6;

        public static Pixel DragCursorColor = Pixel.Green;
        public static Pixel DragBoxColor = Pixel.Red;
        public static float DragCursorRadius = 3f;
        
        public static Vector2 MouseSensitivity = new(1.0f, 1.0f);
        public static Vector2 InspectorPosition = new(14, 3);
    }
}
