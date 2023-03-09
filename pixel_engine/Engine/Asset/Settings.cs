﻿using Newtonsoft.Json;
using pixel_renderer.FileIO;
using System;
using System.Drawing;
using System.Numerics;

namespace pixel_renderer
{
    public class Settings : Asset
    {
        public Vector2 PhysicsArea = new Vector2(10000, 10000);

        public int ScreenH = 256;
        public int ScreenW = 256;

        // 16 == 60FPS
        public int PhysicsTimeStep = 160;
        public int FramerateSampleThreshold = 30;

        public Vector2 CurrentResolution => Runtime.Current.renderHost.GetRenderer().Resolution;

        public string ImagesDir = "\\Images";  // Images Import folder (temporary solution until assets are done, for importing backgrounds)
        public string ProjectsDir = "\\Projects"; // Project files 
        public string AssetsDir = "\\Assets";   // Asset files (user - created)
        public string StagesDir = "\\Stages"; //Stage files

        public string WorkingRoot = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Pixel";    // Root directory for resources
    }
    public class EditorSettings : Asset
    {
        [JsonProperty]
        public float DragCursorRadius = 3f;
        [JsonProperty]
        public Vector2 MouseSensitivity = new(1.0f, 1.0f);
        [JsonProperty]
        public Vector2 InspectorPosition = new(14, 3);
        [JsonProperty]
        public float MouseZoomSensitivityFactor = 1.001f;
        [JsonProperty]
        public int EditorEventQueueMaxLength = 12;
        [JsonProperty]
        public int ConsoleMaxLines = 100;
        [JsonProperty]
        public int InspectorHeight = 10;
        [JsonProperty]
        public int InspectorWidth = 6;
        [JsonProperty]
        public Pixel DragCursorColor = Pixel.Green;
        [JsonProperty]
        public Pixel DragBoxColor = Pixel.Red;
        [JsonProperty]
        public Pixel EditorHighlightColor = Color.Orange;
    }
}