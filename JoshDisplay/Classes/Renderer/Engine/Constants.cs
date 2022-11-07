namespace pixel_renderer
{
    public static class Constants
    {
        internal const int frameRateCheckThresh = 60;
        internal const int screenWidth = 256;
        internal const int screenHeight = 256;
        internal const int collisionCellSize = 4;

        internal const float depenetrationForce = 25f;

        // Not fully implemented, not used ATM.
        internal static float terminalVelocity = .1f;
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


