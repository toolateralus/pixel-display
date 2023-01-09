
using Newtonsoft.Json;
using pixel_renderer.Assets;
using System;

namespace pixel_renderer
{
    [JsonObject(MemberSerialization.OptOut)]
    public class Component
    {
        [JsonIgnore]
        public Node parent { get { return _parent; } set { _parent = value; } }
        public bool Enabled = true;

        private protected Node _parent = null;
        private string _uuid = "";
        public string UUID { get { return _uuid; } init => _uuid = pixel_renderer.UUID.NewUUID(); }
        public string Name { get; set; } = "";

        public virtual void Awake() { }
        public virtual void Update() { }
        public virtual void FixedUpdate(float delta) { }

        public virtual void OnTrigger(Rigidbody other) { }
        public virtual void OnCollision(Rigidbody collider) { }

        public ComponentAsset ToAsset() => new(Name, this, GetType());  
        /// <summary>
        /// Performs a 'Get Component' call on the Parent node object of the component this is called from.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index"></param>
        /// <returns>A component of specified type and parent</returns>
        public T GetComponent<T>(int? index = 0) where T : Component => parent.GetComponent<T>(index);
        /// <summary>
        /// initializes UUID and other readonly info, should NEVER be called by the user.
        /// </summary>
        internal protected void init_component_internal()
        {
            Name = parent.Name + $" {GetType()}";
            Awake();
        }
    }

}
