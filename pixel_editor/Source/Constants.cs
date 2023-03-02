using pixel_renderer;
using System.Numerics;

namespace pixel_editor
{
    public static class Constants
    {
        public static Vector2 InspectorPosition = new(16, 1);
        public const float MouseZoomSensitivityFactor = 1.001f;
        public const int EditorEventQueueMaxLength = 12;
        public static int InspectorWidth = 6;
        public static int InspectorHeight = 10;

        public static Vector2 MouseSensitivity = new(1.5f, 1.5f);
    }
}
