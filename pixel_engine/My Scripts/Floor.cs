using System.Drawing;
using System.Windows.Media.Media3D;

namespace pixel_renderer
{
    /// <summary>
    /// temporary script to keep the floor in place while there is no Kinematic Body (non rigidbodies cannot participate in collision)
    /// </summary>
    public class Floor : Component
    {
        public const float height = 10; 
        public const float width = Constants.CollisionCellSize - 10;
        private Polygon poly;

        public static Node Standard()
        {
            Node node = new("Floor Node");
            node.AddComponent<Floor>();
            return node;
        }
        Vec2 zero = Vec2.zero;
        public override void FixedUpdate(float delta) => parent.Position = zero;
        public override void Awake()
        {
            Collider col = parent.AddComponent<Collider>();
            poly = Polygon.Rectangle(width, height);
            col.Mesh = poly;
        }
    }
}
