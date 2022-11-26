using pixel_renderer;
using Key = System.Windows.Input.Key;
internal class Player : Component
{
    public bool takingInput = true;
    public int speed = 2;
    Sprite sprite = new();
    Rigidbody rb = new();
    public override void Awake()
    {
        sprite = GetComponent<Sprite>();
        rb = GetComponent<Rigidbody>();
    }
    public override void FixedUpdate(float delta)
    {
        if (!takingInput) return;
        if (Input.GetKeyDown(Key.Q))
            Staging.ReloadCurrentStage(); 

        var move = Input.GetMoveVector() * delta;
        Jump(move);
        Move(move);
    }
    public override void OnTrigger(Rigidbody other) { }
    public override void OnCollision(Rigidbody collider) => sprite.Randomize();
    private void Move(Vec2 moveVector)
    {
        rb.velocity.x += moveVector.x * speed;
    }
    private void Jump(Vec2 moveVector)
    {
        var jumpVel = speed * 2;
        rb.velocity.y = moveVector.y * jumpVel;
    }
}
