
using pixel_renderer.Assets;
using Newtonsoft.Json;
using System;

namespace pixel_renderer
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Component
    {
        internal T GetShallowClone<T>() where T : Component => (T)MemberwiseClone();

        public Component()
        {
            _uuid = pixel_renderer.UUID.NewUUID(); 
        }

        [JsonProperty]
        public Node parent { get { return _parent; } set { _parent = value; } }
        [JsonProperty]

        private protected Node _parent = null;
        [JsonProperty] public bool Enabled = true;
        public string UUID => _uuid; 
        [JsonProperty] private string _uuid = "";
        [JsonProperty] public string Name { get; set; } = "";
        public virtual void Awake() { }
        public virtual void Update() { }
        public virtual void FixedUpdate(float delta) { }
        public virtual void OnTrigger(Collider other) { }
        public virtual void OnCollision(Collider collider) { }
        /// <summary>
        /// Performs a 'Get Component' call on the Parent node object of the component this is called from.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index"></param>
        /// <returns>A component of specified type and parent</returns>
        public T GetComponent<T>(int index = 0) where T : Component => parent.GetComponent<T>(index);
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
