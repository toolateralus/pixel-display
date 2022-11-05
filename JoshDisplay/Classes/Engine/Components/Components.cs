
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
        /// <summary>
        /// Best Practice: call base.Awake at the beginning of this method override to get a UUID and ComponentName.
        /// </summary>
        public virtual void Awake()
        {
            UUID = UuID.NewUUID();
            Name = parentNode.Name + $" {GetType()}"; 
        }
    }

}
