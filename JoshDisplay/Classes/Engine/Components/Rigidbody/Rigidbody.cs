namespace PixelRenderer.Components
{
    public class Rigidbody : Component
    {
        private float _drag = 0.0f;
        public float drag = .2f; 
        public bool usingGravity = true;
        public Vec2 velocity = new();
        public Sprite sprite; 

        public override void Awake()
        {
            sprite = parentNode.GetComponent<Sprite>(); 
            base.Awake();
        }
        public override void FixedUpdate()
        {
            if(usingGravity) velocity.y += CMath.Gravity;
            Collision.ViewportCollision(parentNode, sprite, this); 
            _drag = (float)GetDrag().Clamp(-drag, drag);
            ApplyVelocity();
            ApplyPosition();
           
        }
        const double dragCoefficient = 1;
        public double GetDrag()
        {
            Sprite sprite = parentNode.GetComponent<Sprite>() ?? new Sprite(Vec2.one, JRandom.GetRandomColor(), true);
            double velocity = this.velocity.Length;
            double drag = (velocity * velocity) * dragCoefficient;
            if (drag < 0) drag = -drag; 
            return drag; 
        }
        public string GetDebugs()
        {
            return $" \n VELOCITY__X = {velocity.x} \n VELOCITY__Y = {velocity.y} \n POSITION__X = {parentNode.position.x} \n POSITION__Y {parentNode.position.y} \n NODE : {parentNode.Name}";
        }
        void ApplyPosition()
        {
            parentNode.position.y += velocity.y;
            parentNode.position.x += velocity.x;
        }
        void ApplyVelocity()
        {
            if (velocity.x > 15 || velocity.x < -15)
            {
                _drag = 0;
                velocity = Vec2.zero;
                return;
            }

            velocity.y *= _drag;
            velocity.x *= _drag;
        }
        
    }
}

