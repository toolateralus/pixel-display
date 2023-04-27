using Newtonsoft.Json;
using pixel_core.types.physics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace pixel_core.types.Components
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class Component
    {
        [JsonProperty] public Node node;
        [JsonProperty] public bool IsActive = true;
        [JsonProperty]
        public Vector2 Position
        {
            get
            {
                return node?.Position ?? default;
            }
            set
            {
                if (node != null)
                    node.Position = value;
            }
        }
        [JsonProperty] private string _uuid = "";
        public Vector2 Scale
        {
            get => node.Scale; set
            {
                if (node != null)
                    node.Scale = value;
            }
        }
        public float Rotation
        {
            get => node.Rotation; set
            {
                if (node != null)
                    node.Rotation = value;
            }
        }
        public ref Matrix3x2 Transform { get => ref node.Transform; }
        public string Name { get; set; } = "";
        public bool selected_by_editor;
        public string UUID => _uuid;
        public Component()
        {
            _uuid = pixel_core.Statics.UUID.NewUUID();
        }
        // begin comment
        // idk why this is implented twice nor do I know which ones preferable, I think clone works fine.
        public Component Clone() => (Component)MemberwiseClone();
        internal T GetShallowClone<T>() where T : Component => (T)MemberwiseClone();
        // end comment
        public virtual void Awake() { }
        public virtual void Update() { }
        public virtual void FixedUpdate(float delta) { }
        public virtual void OnTrigger(Collision collision) { }
        public virtual void OnCollision(Collision collision) { }
        public virtual void OnFieldEdited(string field) { }
        public virtual void OnDrawShapes() { }
        internal protected void on_destroy_internal()
        {
            Dispose();
            OnDestroy();
        }
        public virtual void OnDestroy() { }
        /// <summary>
        /// You must release all references to any components, nodes, or other engine objects as of now, otherwise unexpected behavior is iminent
        /// </summary>
        public abstract void Dispose();
        public Vector2 LocalToGlobal(Vector2 local) => local.Transformed(Transform);
        internal Vector2 GlobalToLocal(Vector2 global) => global.Transformed(Transform.Inverted());
        public bool TryGetComponent<T>([NotNullWhen(true)] out T result, int index = 0) where T : Component
        {
            result = node.GetComponent<T>(index);
            return result != null;
        }
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
        public void RemoveComponent(Component component)
        {
            node?.RemoveComponent(component);
        }
    }

}
