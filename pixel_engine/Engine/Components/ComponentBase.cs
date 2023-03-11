
using pixel_renderer.Assets;
using Newtonsoft.Json;
using System;
using System.Windows.Media.TextFormatting;
using System.Windows.Controls;
using System.Numerics;
using System.Dynamic;

namespace pixel_renderer
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Component
    {
        public Component Clone() => (Component)MemberwiseClone();
        public Vector2 Scale { get => node.Scale; set => node.Scale = value; }
        public float Rotation { get => node.Rotation; set => node.Rotation = value; }

        [JsonProperty]
        public Node node;
        [JsonProperty]
        public bool IsActive = true;
        [JsonProperty]
        public Vector2 Position
        {
            get
            {
                return node?.Position ?? default;
            }
            set
            {
                if (node is null)
                    return; 
                node.Position = value;
            }
        }
        public ref Matrix3x2 Transform { get => ref node.Transform; }
        public string Name { get; set; } = "";
        [JsonProperty]
        private string _uuid = "";
        public string UUID => _uuid;

        public Component()
        {
            _uuid = pixel_renderer.UUID.NewUUID();
        }
        public virtual void Awake() { }
        public virtual void Update() { }
        public virtual void FixedUpdate(float delta) { }
        public virtual void OnTrigger(Collision collision) {  }
        public virtual void OnCollision(Collision collision) {  }
        /// <summary>
        /// Performs a 'Get Component' call on the Parent node object of the component this is called from.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index"></param>
        /// <returns>A component of specified type and parent</returns>
        public T GetComponent<T>(int index = 0) where T : Component => node.GetComponent<T>(index);
        /// <summary>
        /// initializes UUID and other readonly info, should NEVER be called by the user.
        /// </summary>
        internal protected void init_component_internal()
        {
            Name = $"{GetType().Name}";
            Awake();
        }
        internal T GetShallowClone<T>() where T : Component => (T)MemberwiseClone();
        public virtual void OnFieldEdited(string field) { }
        public virtual void OnDrawShapes() { }
        public Vector2 LocalToGlobal(Vector2 local) => local.Transformed(Transform);
        internal Vector2 GlobalToLocal(Vector2 global) => global.Transformed(Transform.Inverted());

        public virtual void OnDestroy()
        {
            throw new NotImplementedException();
        }
    }

}
