using Newtonsoft.Json;
using pixel_core.FileIO;
using pixel_core.Statics;
using System;
using System.Drawing;
using System.Numerics;

namespace pixel_core
{
    public class ProjectSettings : Asset
    {
        //TODO: THIS ABSOLUTELY CANNOT BE STATIC, THIS IS A HACK DURING THE MOVE OF A BUNCH OF CLASSES/EVERYTHING INTO SMALLER PROJECTS, THIS MUST BE REMOVED TODO TO DO TODO:
        [JsonProperty] public static Vector2 PhysicsArea = new Vector2(10000, 10000);
        [JsonProperty] public static Pixel EditorHighlightColor = Color.Orange;
        [JsonProperty] public static string WorkingRoot = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Pixel";    // Root directory for resources
        [JsonProperty] public static int PhysicsTimeStep = 16;
        [JsonProperty] public static Vector2 CurrentResolution = new(256, 256);

        public override void Sync()
        {
            Metadata = new("Project Settings", Constants.WorkingRoot + "projectSettings.asset", Constants.AssetsFileExtension);
        }
    }

}


