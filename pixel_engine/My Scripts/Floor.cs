namespace pixel_renderer
{
    public class Floor : Component
    {
        public const float height = 10; 
        public const float width = Constants.CollisionCellSize - 10;
        private Polygon poly;

        public static Node Standard()
        {
            Node node = new("Floor");
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
