using Newtonsoft.Json;
using Pixel.FileIO;
using Pixel.Statics;
using System;
using System.Drawing;
using System.Numerics;

namespace Pixel
{
    public class ProjectSettings : Asset
    {
        //TODO: THIS ABSOLUTELY CANNOT BE STATIC, THIS IS A HACK DURING THE MOVE OF A BUNCH OF CLASSES/EVERYTHING INTO SMALLER PROJECTS, THIS MUST BE REMOVED TODO TO DO TODO:
        [JsonProperty] public static Vector2 PhysicsArea = new Vector2(10000, 10000);
        [JsonProperty] public static Color EditorHighlightColor = System.Drawing.Color.Orange;
        [JsonProperty] public static string WorkingRoot = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Pixel";    // Root directory for resources
        [JsonProperty] public static int PhysicsTimeStep = 16;
        [JsonProperty] public static Vector2 CurrentResolution = new(512, 288);

        public ProjectSettings(string name, bool shouldUpload = false) : base(name, shouldUpload)
        {
        }
    }

}


