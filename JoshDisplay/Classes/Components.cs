namespace PixelRenderer.Components
{

    public abstract class Component
    {
        public Node parentNode = new(); 
        public virtual void Update()
        {

        }
        public virtual void Awake()
        {

        }
    }
}
