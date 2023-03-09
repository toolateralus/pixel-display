using pixel_renderer.FileIO;
using System;
using System.Drawing;
using System.Numerics;

namespace pixel_renderer
{
    public class ProjectSettings : Asset
    {
        public Vector2 PhysicsArea = new Vector2(10000, 10000);
        public Pixel EditorHighlightColor = Color.Orange;
        public string WorkingRoot = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Pixel";    // Root directory for resources
        public int PhysicsTimeStep = 16;
        public Vector2 CurrentResolution => Runtime.Current.renderHost.GetRenderer().Resolution;
        public Vector2 DefaultResolution => new(ScreenW, ScreenH);
        public int ScreenH = 256;
        public int ScreenW = 256;
       
    }

}


