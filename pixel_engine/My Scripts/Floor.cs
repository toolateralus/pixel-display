namespace pixel_renderer
{
    /// <summary>
    /// temporary script to keep the floor in place while there is no Kinematic Body (non rigidbodies cannot participate in collision)
    /// </summary>
    internal class Floor : Component
    {
        public Vec2 startPos = new(256, 256);
        public override void Update() => parent.position = startPos;
        public override void OnCollision(Collider collider) => GetComponent<Sprite>().Randomize();
    }
}
