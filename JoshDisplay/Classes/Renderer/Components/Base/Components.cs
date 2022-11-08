
using System;

namespace pixel_renderer
{

    public abstract class Component
    {
        public Node parentNode = new();
        public string UUID { get; set; }
        public string Name { get; set; }
        public virtual void OnTrigger(Rigidbody other)
        {

        }
        public virtual void OnCollision(Rigidbody collider)
        {

        }
        public virtual void FixedUpdate(float delta)
        {

        }
        public virtual void Update()
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
