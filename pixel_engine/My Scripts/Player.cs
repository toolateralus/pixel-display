using pixel_renderer;
using Key = System.Windows.Input.Key;
internal class Player : Component
{
    [Field] public bool takingInput = true;
    [Field] public int speed = 2;
    [Field] Sprite sprite = new();
    [Field] Rigidbody rb = new();
    public override void Awake()
    {
       parent.TryGetComponent(out sprite);
       parent.TryGetComponent(out rb);
    }
    public override void FixedUpdate(float delta)
    {
        if (!takingInput) 
            return;

        var move = Input.GetMoveVector();
        string log = "Move Vector Sum =  " +move.Sum().ToString();
        Runtime.Log(log);
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
