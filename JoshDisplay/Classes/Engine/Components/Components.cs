
namespace pixel_renderer
{

    public abstract class Component
    {
        public Node parentNode = new();
        public abstract string UUID { get; set; }
        public abstract string Name { get; set; }
        public virtual void FixedUpdate(float delta)
        {

        }
      
        public virtual void Awake()
        {

        }
    }

}
