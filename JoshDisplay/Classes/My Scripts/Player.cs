using pixel_renderer;
using System.Windows.Documents;
using Color = System.Drawing.Color;
internal class Player : Component
{
    Rigidbody rb;
    Sprite sprite;
    public bool takingInput = true; 
    public const float speed = 1f;
    int i = 0;
    public override void Awake()
    {
        base.Awake(); 
        rb = parentNode.GetComponent<Rigidbody>();
        sprite = parentNode.GetComponent<Sprite>();
        if (sprite == null || rb == null) return; 
        sprite.isCollider = true;
        sprite.DrawSquare(Vec2.one * 8, Color.AliceBlue, true);
    }
    public override void FixedUpdate(float delta)
    {
        if (!takingInput) return;
        RandomizeSpriteColor();
        var move = Input.GetMoveVector();
        GetJumpInput(move);
        GetMove(move);
    }
    public override void OnTrigger(Rigidbody other)
    {

    }
    private void GetMove(Vec2 moveVector)
    {
        rb.velocity.x += moveVector.x * speed; 
    }
    private void GetJumpInput(Vec2 moveVector)
    {
        if (moveVector.y == 0) return;
        rb.velocity.y = moveVector.y * speed * 6; 
    }
    private void RandomizeSpriteColor()
    {
        i++;
        if (i > 60)
        {
            i = 0;
            int x = (int)sprite.size.x;
            int y = (int)sprite.size.y;
            var colorData = new Color[x, y];
            for(int j = 0; j < y; j++)
            for (int i = 0; i < x; i++)
            {
                colorData[i, j] = JRandom.Color();
            }
            sprite.DrawSquare(sprite.size, colorData, true);
        }
    }

}
