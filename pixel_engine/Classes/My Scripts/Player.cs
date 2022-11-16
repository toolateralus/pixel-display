using pixel_renderer;
using Color = System.Drawing.Color;
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
        sprite.isCollider = true; 

        sprite.DrawSquare(Vec2.one * 8, Color.NavajoWhite, true);
    }
    public override void FixedUpdate(float delta)
    {
        if (!takingInput) return;
        var move = Input.GetMoveVector();
        Jump(move);
        Move(move);
    }
    public override void OnTrigger(Rigidbody other) {}
    public override void OnCollision(Rigidbody collider) => RandomizeSpriteColor();

    private void Move(Vec2 moveVector) => rb.velocity.x += moveVector.x * speed; 
    
    private void Jump(Vec2 moveVector)
    {
        var jumpVel = speed * 2;
        rb.velocity.y = moveVector.y * jumpVel; 
    }
    
    private void RandomizeSpriteColor()
    {
        int x = (int)sprite.size.x;
        int y = (int)sprite.size.y;

        var colorData = new Color[x, y];
        for (int j = 0; j < y; j++)
            for (int i = 0; i < x; i++)
                colorData[i, j] = JRandom.Color();

        sprite.DrawSquare(sprite.size, colorData, sprite.isCollider);
    }

}
