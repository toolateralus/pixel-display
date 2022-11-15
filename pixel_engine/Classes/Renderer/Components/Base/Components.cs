
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
            
        }

        internal void Init()
        {
            UUID = pixel_renderer.UUID.NewUUID();
            Name = parentNode.Name + $" {GetType()}";
            Awake(); 
        }
        /// <summary>
        /// Performs a 'Get Component' call on the Parent node object of the component this is called from.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index"></param>
        /// <returns>A component of specified type and parent</returns>
        public T GetComponent<T>(int? index = 0) where T : Component => parentNode.GetComponent<T>(index);
    }

}
