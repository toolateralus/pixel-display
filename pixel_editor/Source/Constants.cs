
using pixel_renderer;
namespace pixel_editor
{
    public static class Constants
    {
        public const int EditorEventQueueMaxLength = 12;

        public static Vec2Int InspectorPosition = new(16, 4);

        public const float MouseZoomSensitivityFactor = 1.001f;

        public static int InspectorWidth = 6;
        public static int InspectorHeight = 8;
    }
}
