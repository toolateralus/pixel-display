using PixelRenderer;
using PixelRenderer.Components;
using System.Windows.Input;
using Color = System.Drawing.Color; 
internal class Player : Component
{
    Rigidbody rb;
    public bool TakingInput { get; set; } = true;
    bool jumpHeld = false;
    const float speed = .5f; 
    public override void Awake()
    {
        rb = parentNode.GetComponent<Rigidbody>(); 
    }
    public override void FixedUpdate()
    {
        RandomizeSpriteColor();
        if (!TakingInput) return;
        GetJumpInput();
        GetMove();
    }
   
    private void GetMove()
    {
        if (Input.GetKeyDown(Key.A)) rb.velocity.x += -speed;
        if (Input.GetKeyDown(Key.D)) rb.velocity.x += speed;
    }
    private void GetJumpInput()
    {
        if (Input.GetKeyDown(Key.W) || Input.GetKeyDown(Key.Space))
        {
            if (rb.isGrounded && !jumpHeld) rb.velocity.y = -20;
            jumpHeld = true;
        }
        else if (rb.isGrounded) jumpHeld = false;
        if (Input.GetKeyDown(Key.S)) rb.velocity.y = speed;
    }
    private void RandomizeSpriteColor()
    {
        if (parentNode.sprite != null)
        {
            Sprite sprite = parentNode.sprite;
            int x = (int)sprite.size.x;
            int y = (int)sprite.size.y;
            var colorData = new Color[x, y];
            for (int i = 0, j = 0; i < x && j < y; i++, j++)
            {
                colorData[i, j] = JRandom.GetRandomColor();
            }
            parentNode.sprite.DrawSquare(Vec2.one * 6, colorData, true);
        }
    }

}
