using System;
using System.Diagnostics.PerformanceData;
using System.Reflection.Metadata;

namespace pixel_renderer
{
    public  class Settings
    {
        /// <summary>
        /// FixedUpdate / Physics refresh interval,
        /// NOTE: for some reason this seems to have absolutely no effect other than it cannot be zero. 
        /// this is in the way of implementing a time scale.
        /// </summary>  
        public const float PhysicsRefreshInterval = .01f;
        public const float MaxDepenetrationForce = 3f;
        public static int TerminalVelocity = 4;
        public const int CollisionCellSize = 16; // this value determines the area of a Broad Collision Cell, which segments the worlds physics into chunks. The area of the largest Object in the stage must be smaller than this value.
        public static Vec2 TerminalVec2()
        {
            return new Vec2()
            {
                x = TerminalVelocity,
                y = TerminalVelocity,
            };
        }
        public static Vec3 TerminalVec3()
        {
            return new Vec3()
            {
                x = TerminalVelocity,
                y = TerminalVelocity,
                z = TerminalVelocity,
            };
        }
        public static bool WithinTerminalVelocity(Vec2 velocity)
        {
            return velocity.x <= TerminalVelocity && velocity.x >= -TerminalVelocity &&
                velocity.y <= TerminalVelocity && velocity.y >= -TerminalVelocity;
        }
        public static bool WithinTerminalVelocity(Vec3 velocity)
        {
            return velocity.x <= TerminalVelocity && velocity.x >= -TerminalVelocity &&
                velocity.y <= TerminalVelocity && velocity.y >= -TerminalVelocity &&
                 velocity.z <= TerminalVelocity && velocity.z >= -TerminalVelocity;
        }
        public static bool WithinTerminalVelocity(Rigidbody rigidbody) => WithinTerminalVelocity(rigidbody.velocity);

        public const int FramerateSampleThreshold = 60;
        public const int ScreenH = 256;
        public const int ScreenW = 256;
            
        public static char[] unsupported_chars = { '_', '-', '.', '`'};
        public static char[] int_chars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};
            
        public const string FontsDir = ImagesDir + "\\Font";  // Font Images asset directory (Probably shouldnt be a constant, for text rendering.)
        public const string ImagesDir = AssetsDir + "\\Images";  // Images Import folder (temporary solution until assets are done, for importing backgrounds)
        public const string ProjectsDir = "\\Pixel\\Projects"; // Project files 
        public const string AssetsDir = "\\Pixel\\Assets";   // Asset resources (user - created)
        public const string AssetsFileExtension = ".pxad";  // pxad {Pixel Asset Data}
        public const string ProjectFileExtension = ".pxpj";   // pxpj {Pixel Project}
        public static string AppDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);    // Root directory for resources
    }

}


