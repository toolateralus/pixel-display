using Newtonsoft.Json;
using Pixel.FileIO;
using Pixel.Statics;
using System;
using System.Drawing;
using System.Numerics;

namespace Pixel
{
    public class EditorSettings : Asset
    {
        [JsonProperty]
        public float DragCursorRadius = 0.2f;
        [JsonProperty]
        public Vector2 MouseSensitivity = new(1.0f, 1.0f);
        [JsonProperty]
        public Vector2 InspectorPosition = new(14, 1);
        [JsonProperty]
        public float MouseZoomSensitivityFactor = 1.001f;
        [JsonProperty]
        public int EditorEventQueueMaxLength = 6;
        [JsonProperty]
        public int ConsoleMaxLines = 150;
        [JsonProperty]
        public int InspectorHeight = 10;
        [JsonProperty]
        public int InspectorWidth = 6;
        [JsonProperty]
        public Color DragCursorColor = System.Drawing.Color.White;
        [JsonProperty]
        public Color DragBoxColor = System.Drawing.Color.MediumPurple;
        [JsonProperty]
        public Color EditorHighlightColor = System.Drawing.Color.Orange;

        public EditorSettings(string name, bool shouldUpload = false) : base(name, shouldUpload)
        {
           
        }

        public override void Sync()
        {
            Metadata = new(Constants.WorkingRoot + "editorSettings.asset");
        }

    }
}