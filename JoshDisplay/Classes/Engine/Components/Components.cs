
namespace pixel_renderer
{

    public abstract class Component
    {
        public Node parentNode = new();
        public virtual void FixedUpdate()
        {

        }
        public virtual void Awake()
        {

        }
    }

}
