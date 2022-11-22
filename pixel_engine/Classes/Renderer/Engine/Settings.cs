using System;

namespace pixel_renderer
{
    public static class Settings
    {
        public const int FramesBetweenFramerateChecks = 60;
        public const int ScreenHeight = 256;
        public static char[] unsupported_char_array = { '_', '-', '.', '`' };
        public static char[] int_chars_array = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', };
        public const int ScreenWidth = 256;
        /// <summary>
        /// this value determines the area of a Broad Collision Cell, which segments the worlds physics into chunks. The area of the largest Object in the stage must be smaller than this value.
        /// </summary>
        public const int CollisionCellSize = 16;
        /// <summary>
        /// FixedUpdate / Physics refresh interval,
        /// NOTE: for some reason this seems to have absolutely no effect other than it cannot be zero. 
        /// this is preventing us from implementing a time scale.
        /// </summary>
        public const float PhysicsRefreshInterval = .01f;
        // Font Images asset directory (Probably shouldnt be a constant, for text rendering.)
        public const string FontDirectory = ImagesDirectory + "\\Font";
        // Images Import folder (temporary solution until assets are done, for importing backgrounds)
        public const string ImagesDirectory = AssetsDirectory + "\\Images";
        // Asset resources (user - created)
        public const string AssetsDirectory = "\\Pixel\\Assets";
        // Root directory for resources
        public static string Appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        // pxad {Pixel Asset Data}
        public const string AssetsFileExtension = ".pxad";
        internal const float MaxDepenetrationForce = 3f;
        internal static int terminalVelocity = 4;

        internal static Vec2 TerminalVec2()
        {
            return new Vec2()
            {
                x = terminalVelocity,
                y = terminalVelocity,
            };
        }
        internal static Vec3 TerminalVec3()
        {
            return new Vec3()
            {
                x = terminalVelocity,
                y = terminalVelocity,
                z = terminalVelocity,
            };
        }
        internal static bool WithinTerminalVelocity(Vec2 velocity)
        {
            return velocity.x <= terminalVelocity && velocity.x >= -terminalVelocity &&
                velocity.y <= terminalVelocity && velocity.y >= -terminalVelocity;
        }
        internal static bool WithinTerminalVelocity(Vec3 velocity)
        {
            return velocity.x <= terminalVelocity && velocity.x >= -terminalVelocity &&
                velocity.y <= terminalVelocity && velocity.y >= -terminalVelocity &&
                 velocity.z <= terminalVelocity && velocity.z >= -terminalVelocity;
        }
        internal static bool WithinTerminalVelocity(Rigidbody rigidbody) => WithinTerminalVelocity(rigidbody.velocity);

    }

}


