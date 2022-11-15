using System;

namespace pixel_renderer
{
    public static class Constants
    {
        public const int FramesBetweenFramerateChecks = 60;
        
        public const int ScreenHeight = 256;
        
        public const int ScreenWidth = 256;
        
        public const int CollisionCellSize = 6;
        /// <summary>
        /// FixedUpdate / Physics refresh interval,
        /// NOTE: for some reason this seems to have absolutely no effect other than it cannot be zero. 
        /// this is preventing us from implementing a time scale.
        /// </summary>
        public const float PhysicsRefreshInterval = .01f;
        
        public const string FontDirectory = ImagesDirectory + "\\Font";
        
        public const string ImagesDirectory = AssetsDirectory + "\\Images";
        
        public const string AssetsDirectory = "\\Pixel\\Assets";
        
        public static string WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); 
        
        internal const float depenetrationForce = 25f;
        // Not fully implemented, not used ATM.

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
            if (velocity.x > terminalVelocity || velocity.x < -terminalVelocity || 
                velocity.y > terminalVelocity || velocity.y < -terminalVelocity) { 
                return false;
            }
            return true; 
        }
        
        internal static bool WithinTerminalVelocity(Vec3 velocity)
        {
            if (velocity.x > terminalVelocity || velocity.x < -terminalVelocity ||
                velocity.y > terminalVelocity || velocity.y < -terminalVelocity ||
                 velocity.z > terminalVelocity || velocity.z < -terminalVelocity){
                return false;
            }
            return true;
        }
        
        internal static bool WithinTerminalVelocity(Rigidbody rigidbody)
        {
            if (rigidbody.velocity.x > terminalVelocity || rigidbody.velocity.x < -terminalVelocity ||
                rigidbody.velocity.y > terminalVelocity || rigidbody.velocity.y < -terminalVelocity)
            {
                return false;
            }
            return true;
        }
    }

}


