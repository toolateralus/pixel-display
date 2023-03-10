using Newtonsoft.Json;
using pixel_renderer.FileIO;
using System;
using System.Drawing;
using System.Numerics;

namespace pixel_renderer
{
    public class ProjectSettings : Asset
    {
        [JsonProperty]
        public Vector2 PhysicsArea = new Vector2(10000, 10000);
        [JsonProperty]
        public Pixel EditorHighlightColor = Color.Orange;
        [JsonProperty]
        public string WorkingRoot = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Pixel";    // Root directory for resources
        [JsonProperty]
        public int PhysicsTimeStep = 16;
        [JsonProperty]
        public Vector2 CurrentResolution = Runtime.Current.renderHost.GetRenderer().Resolution;
        [JsonProperty]
        public Vector2 DefaultResolution => new(ScreenW, ScreenH);
        [JsonProperty]
        public int ScreenH = 256;
        [JsonProperty]
        public int ScreenW = 256;
    }

}


