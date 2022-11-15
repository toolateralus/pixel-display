
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
       
        public virtual void Awake()
        {
            UUID = pixel_renderer.UUID.NewUUID();
            Name = parentNode.Name + $" {GetType()}"; 
        }

    }

}
