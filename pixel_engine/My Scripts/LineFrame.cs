using pixel_renderer.ShapeDrawing;
using System.Numerics;
using System.Threading.Tasks;

namespace pixel_renderer
{
    public class LineFrame
    {
        public Vector2 velocity;

        public LineFrame(Vector2 velocity, Vector2 start, Vector2 end, Pixel color)
        {
            this.velocity = velocity;
            this.start = start;
            this.end = end;
            this.color = color;
        }

        public Line Next(Pixel colorInput, Vector2 additionalVelocity, Vector2 newEnd, Vector2 newStart)
        {
            adjustColor();
            move();
            return new Line(start, end); 

            async void adjustColor()
            {
                float j = 0;
                while (j <= 1)
                {
                    color = Pixel.Lerp(color, colorInput, j);
                    j += 0.01f;
                    await Task.Delay(1);
                }
            }
            void move()
            {
                start = newStart;
                end = newEnd;
                velocity += additionalVelocity;

                if (velocity != Vector2.Zero)
                {
                    start *= velocity;
                    end *= velocity;
                }
               
            }
        }

        public Vector2 start;
        public Vector2 end;
        public Pixel color;
    }
}
