using pixel_renderer;
using System.Numerics;

namespace pixel_editor
{
    public static class Constants
    {
        public static Vector2 InspectorPosition = new(16, 2);
        public const float MouseZoomSensitivityFactor = 1.001f;
        public const int EditorEventQueueMaxLength = 12;
        public static int InspectorWidth = 6;
        public static int InspectorHeight = 10;
        public const int ConsoleMaxLines = 100; 
        public static Vector2 MouseSensitivity = new(1.0f, 1.0f);

    }
}
