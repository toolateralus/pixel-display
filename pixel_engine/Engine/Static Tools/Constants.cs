using System;
using System.Collections.Generic;
using System.Linq;

namespace pixel_renderer
{
    public class Constants
    {
        #region Physics / Collision Constants
        /// <summary>
        /// FixedUpdate / Physics refresh interval,
        /// NOTE: for some reason this seems to have absolutely no effect other than it cannot be zero. 
        /// this is in the way of implementing a time scale.
        /// </summary>  
        public const float PhysicsRefreshInterval = .01f;
        public const float MaxDepenetrationForce = 3f;
        public static int TerminalVelocity = 4;
        public const int CollisionCellSize = 24; // this value determines the area of a Broad Collision Cell, which segments the worlds physics into chunks. The area of the largest Object in the stage must be smaller than this value.

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
        #endregion
        #region General Constants
        public const int FramerateSampleThreshold = 60;
        public const int ScreenH = 256;
        public const int ScreenW = 256;
        public const byte maxClickDistanceInPixels = 25; 
        public static Vec2 ScreenVec => new Vec2(ScreenW, ScreenH);
        #endregion
        #region File IO and String/Char Constants
        public static char[] unsupported_chars = { '_', '-', '.', '`'};
        public static char[] int_chars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};
        #endregion
        #region Paths and Directories
        public const string ImagesDir = "\\Images";  // Images Import folder (temporary solution until assets are done, for importing backgrounds)
        public const string ProjectsDir = "\\Projects"; // Project files 
        public const string AssetsDir = "\\Assets";   // Asset resources (user - created)
        public const string AssetsFileExtension = ".pxad";  // pxad {Pixel Asset Data}
        public const string ProjectFileExtension = ".pxpj";   // pxpj {Pixel Project}
        public static string WorkingRoot = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Pixel";    // Root directory for resources
        #endregion
        private static List<Type> GetInheritedTypesFromBase<T>()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
               .SelectMany(domainAssembly => domainAssembly.GetTypes())
               .Where(type => typeof(T).IsAssignableFrom(type)).ToList();
            return types;
        }

    }

}


