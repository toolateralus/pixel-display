using Newtonsoft.Json;
using Pixel.Types.Physics;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Pixel.Types.Components
{
    /// <summary>
    /// The base class for all Components, which are modules added to nodes to exend behavior.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class Component
    {
        /// <summary>
        /// the owner of this component.
        /// </summary>
        [JsonProperty] public Node node;
        /// <summary>
        /// will this component be updated or this next frame?
        /// </summary>
        [JsonProperty] public bool Enabled = true;
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
        public bool awake;

        public string UUID => _uuid;
        public Component()
        {
            _uuid = Pixel.Statics.UUID.NewUUID();
        }
        // begin comment
        // idk why this is implented twice nor do I know which ones preferable, I think clone works fine.
        public Component Clone() => (Component)MemberwiseClone();
        internal T GetShallowClone<T>() where T : Component => (T)MemberwiseClone();
        // end comment
       
        
        internal protected void fixed_update_internal(float frameTime)
        {
            FixedUpdate(frameTime);
        }
        internal protected void update_internal() => Update();
        internal protected void on_draw_shapes_internal() => OnDrawShapes();
        internal protected void on_trigger_internal(Collision collision) => OnTrigger(collision);
        internal protected void on_collision_internal(Collision collision) => OnCollision(collision);
        internal protected void on_destroy_internal()
        {
            Dispose();
            OnDestroy();
            node.RemoveComponent(this);
        }
        internal protected void init_component_internal()
        {
            if (awake)
                return;

            awake = true;
            Name = $"{GetType().Name}";
            Awake();
        }

        #region User Overrides

        /// <summary>
        /// Will be called before <see cref="Update"/>
        /// </summary>
        public virtual void Awake() { }
        /// <summary>
        /// Will be called after <see cref="Awake"/> and each subsequent render frame while running.
        /// </summary>
        public virtual void Update() { }
        /// <summary>
        /// Will be called after <see cref="Awake"/> and each subsequent physics frame while running.
        /// </summary>
        public virtual void FixedUpdate(float delta) { }
        /// <summary>
        /// Will be called in the event that this Component's parent <see cref="Node"/>'s components participated in a collision where one or more of the colliders were flagged IsTrigger.
        /// </summary>
        /// 
        public virtual void OnTrigger(Collision collision) { }
        /// <summary>
        /// Will be called in the event that this Component's parent <see cref="Node"/>'s components participated in a collision.
        /// </summary>
        public virtual void OnCollision(Collision collision) { }
        /// <summary>
        /// Will be called in the event that a field is edited by reflection and the Editor's Component Editor
        /// </summary>
        /// <param name="field"> the field that was called to be edited.</param>
        public virtual void OnFieldEdited(string field) { }
        /// <summary>
        /// Will be called each frame by the renderer at the time that the shape drawer is collecting a cycle's worth of debug data.
        /// </summary>
        /// 
        public virtual void OnDrawShapes() { }
        /// <summary>
        /// is called before Destroy to perform Dispose.
        /// </summary>
        /// <summary>
        /// Destroy's this component.
        /// </summary>
        public virtual void OnDestroy() { }
        /// <summary>
        /// You must release all references to any components, nodes, or other engine objects as of now, otherwise unexpected behavior is iminent
        /// </summary>
        public abstract void Dispose();

        #endregion
        public Vector2 LocalToGlobal(Vector2 local) => local.Transformed(Transform);
        internal Vector2 GlobalToLocal(Vector2 global) => global.Transformed(Transform.Inverted());
        public bool TryGetComponent<T>(out T result, int index = 0) where T : Component
        {
            if (node.HasComponent<T>())
            {
                result = node.GetComponent<T>(index);
                return true;
            }
            result = default!;
            return false; 
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
        /// <summary>
        /// A wrapper for <see cref="Node.RemoveComponent"/>
        /// </summary>
        /// <param name="component"></param>
        public void RemoveComponent(Component component)
        {
            node?.RemoveComponent(component);
        }
    }

}
