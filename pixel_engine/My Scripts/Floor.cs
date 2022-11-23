namespace pixel_renderer
{
    /// <summary>
    /// temporary script to keep the floor in place while there is no Kinematic Body (non rigidbodies cannot participate in collision)
    /// </summary>
    internal class Floor : Component
    {
        public Vec2 startPos = new(256, 256);
        public override void Update() => parentNode.position = startPos;
        public override void OnCollision(Rigidbody collider) => GetComponent<Sprite>().Randomize();
    }
}
