using pixel_renderer;
 
namespace pixel_editor
{
    public static class Constants
    {
        public static Vec2Int InspectorPosition = new(16, 1);
        public const float MouseZoomSensitivityFactor = 1.001f;
        public const int EditorEventQueueMaxLength = 12;
        public static int InspectorWidth = 6;
        public static int InspectorHeight = 10;
    }
}
