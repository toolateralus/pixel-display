using Newtonsoft.Json;
using Pixel.Types.Components;
using Pixel.Types.Physics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Component = Pixel.Types.Components.Component;

namespace Pixel
{
    /// <summary>
    /// The node represents a GameObject, which always includes a Transform (position , scale, rotation).
    /// Components are modules added to a Node which are routinely updated auto with subscribed events and
    /// overriden virutal methods.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public partial class Node
    {
        #region Json Constructor
        [JsonConstructor]
        public Node(bool Enabled,
                    Stage parentStage,
                    Dictionary<Type, List<Component>> Components,
                    string name,
                    string tag,
                    Node? parentNode,
                    Matrix3x2 Transform,
                    List<Node> children,
                    string nodeUUID)
        {
            //utils
            this.Transform = Transform;
            this.Enabled = Enabled;

            // strings
            Name = name;
            _uuid = nodeUUID;
            this.tag = tag;

            // nodes
            this.parent = parentNode;
            this.children = children;

            // components
            this.Components = Components;


        }
        #endregion
        #region Other Constructors
        public Node()
        {
            if (Interop.IsRunning && Interop.Stage is Stage stage)
                stage.AddNode(this);

            _uuid = Statics.UUID.NewUUID();
        }
        public Node(string name) : this() => Name = name;
        public Node Clone() { return (Node)MemberwiseClone(); }
        public Node(string name, Vector2 pos, Vector2 Scale) : this(name)
        {
            Position = pos;
            this.Scale = Scale;
        }
        #endregion

        // Events
        /// <summary>
        /// Is triggered when this node or any of is components participate in a collision.
        /// </summary>
        public Action<Collision> OnCollided;
        /// <summary>
        /// Is triggered when this node or any of it's components participate in a collision where
        /// one or more of the colliders had "IsTriggered" set to true.
        /// </summary>
        public Action<Collision> OnTriggered;
        /// <summary>
        /// Is triggered when this node is called to be destroyed, after <see cref="Dispose"/> is called.
        /// </summary>
        public Action OnDestroyed;
        
        private bool _enabled = true;
        private string _uuid = "";
        public bool awake;

        /// <summary>
        /// if this is false the node won't be moved or updated, nor visible.
        /// </summary>
        [JsonProperty] public bool Enabled { get { return _enabled; } set => _enabled = value; }
        /// <summary>
        /// a Universally Unique Identifier.
        /// </summary>
        [JsonProperty] public string UUID { get { return _uuid; } set { _uuid = value; } }
        /// <summary>
        /// The name of the node.
        /// </summary>
        [JsonProperty] public string Name { get; set; }
        /// <summary>
        /// A way to group nodes and query them without looking for components.
        /// </summary>
        [JsonProperty] public string tag = "Untagged";
        /// <summary>
        /// The node directly above this one in the hierarchy.
        /// </summary>
        [JsonProperty] public Node? parent;
        /// <summary>
        /// Every child node of this node.
        /// </summary>
        [JsonProperty] public List<Node> children = new();
        /// <summary>
        /// A list of the offset between <see cref="this"/> <see cref="Node"/> and it's children to maintain during physics updates.
        /// </summary>
        internal Dictionary<string, Vector2> child_offsets = new();
        /// <summary>
        /// A cached <see cref="Rigidbody"></see> to save <see cref="GetComponent{T}(int)"></see> calls/allocations.
        /// </summary>
        public Rigidbody? rb;
        /// <summary>
        /// A cached <see cref="Collider"></see> to save <see cref="GetComponent{T}(int)"></see> calls/allocations.
        /// </summary>
        public Collider col;
        /// <summary>
        /// A cached <see cref="Sprite"></see> to save <see cref="GetComponent{T}(int)"></see> calls/allocations.
        /// </summary>
        internal Sprite sprite;
        /// <summary>
        /// A dictionary of lists of <see cref="Component"/> stored by <see cref="Type"/>
        /// </summary>
        [JsonProperty] public Dictionary<Type, List<Component>> Components { get; set; } = new Dictionary<Type, List<Component>>();
        // Transform
        /// <summary>
        /// A transform matrix updated with Position,Rotation,Scale frequently.
        /// </summary>
        [JsonProperty] public Matrix3x2 Transform = Matrix3x2.Identity;
        public Vector2 Position
        {
            get => Transform.Translation;
            set
            {
                Transform.Translation = value;
            }
        }
        public float Rotation
        {
            get => MathF.Atan2(Transform.M21, Transform.M11);
            set
            {
                var cos = MathF.Cos(value);
                var sin = MathF.Sin(value);

                Transform.M11 = cos;
                Transform.M12 = sin;
                Transform.M21 = -sin;
                Transform.M22 = cos;
            }
        }
        public Vector2 Scale
        {
            get
            {
                var sx = MathF.Sqrt(Transform.M11 * Transform.M11 + Transform.M12 * Transform.M12);
                var sy = MathF.Sqrt(Transform.M21 * Transform.M21 + Transform.M22 * Transform.M22);
                return new Vector2(sx, sy);
            }
            set
            {
                Transform.M11 = value.X;
                Transform.M22 = value.Y;
            }
        }
        #region Hierarchy Functions
        /// <summary>
        /// The method used to insert a node as child of this one.
        /// </summary>
        /// <param name="child"></param>
        public void Child(Node child)
        {
            if (ContainsCycle(child))
            {
                Interop.Log("A cyclic resource inclusion was detected.");
                return;
            }
            if (!Interop.Stage.nodes.Contains(child))
                Interop.Stage?.AddNode(child);
            children ??= new();
            if (children.Contains(child))
                return;

            _ = child.parent?.TryRemoveChild(child);
            children.Add(child);
            Vector2 offset = Position + child.Position;

            if (child_offsets.ContainsKey(child.UUID))
                child_offsets[child.UUID] = offset;
            else child_offsets.Add(child.UUID, offset);

            child.parent = this;
        }
        /// <summary>
        /// A way to remove children if they exist under this one.
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        public bool TryRemoveChild(Node child)
        {
            foreach (var kvp in children)
                if (kvp == child)
                {
                    children.Remove(kvp);
                    child.parent = null;
                    return true;
                }
            return false;

        }
        /// <summary>
        /// Used to check whether theres a cyclic inclusion in the hierarchy above and below this node.
        /// </summary>
        /// <param name="newNode"></param>
        /// <returns></returns>
        public bool ContainsCycle(Node newNode)
        {
            // Check for cycles in child nodes

            var visited = new HashSet<Node>();
            var stack = new Stack<Node>();
            stack.Push(newNode);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                visited.Add(current);
                foreach (var child in current.children)
                {
                    if (child == newNode)
                    {
                        // A cycle is detected
                        return true;
                    }
                    if (!visited.Contains(child))
                    {
                        stack.Push(child);
                    }
                }
            }

            // Check for cycles in parent nodes
            visited.Clear();
            stack.Clear();
            stack.Push(this);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                visited.Add(current);
                if (current == newNode)
                {
                    // A cycle is detected
                    return true;
                }
                if (current.parent != null && !visited.Contains(current.parent))
                {
                    stack.Push(current.parent);
                }
            }

            return false;
        }
        #endregion
        #region Events/Messages
        /// <summary>
        /// A way for a node to subscribe/unsubscribe from events like Update/FixedUpdate/Awake/OnCollision etc. this does not need to be called by the user.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="stage"></param>
        internal void SubscribeToEngine(bool v)
        {
            if (v)
            {
                if (Interop.Stage != null)
                    Interop.Stage.FixedUpdate += update_transform_hierarchy_internal;

                Parallel.ForEach(Components, (comp) => 
                {
                    foreach (Component _component in comp.Value)
                        SubscribeComponent(_component);
                });
             
                foreach (var node in children) 
                    node.SubscribeToEngine(v);
            }
            else
            {
                if (Interop.Stage != null)
                    Interop.Stage.FixedUpdate -= update_transform_hierarchy_internal;

                Parallel.ForEach(Components, (comp) => {
                    foreach (Component _component in comp.Value)
                        UnsubscribeComponent(_component);
                });

                foreach (var node in children)
                    node.SubscribeToEngine(v);
            }
        }
        /// <summary>
        /// Destroys this node and all of it's components.
        /// </summary>
        public void Destroy()
        {
            OnDestroyed?.Invoke();
            Dispose();
            if (Interop.Stage is not null)
                Interop.Stage.RemoveNode(this);
        }
        /// <summary>
        /// Cleans up any managed resources referring to Node/Component as they don't have a great disposal system yet.
        /// </summary>
        public virtual void Dispose()
        {
            rb = null;
            col = null;
            sprite = null;
            foreach (var component in Components)
                foreach (var c in component.Value)
                    c.Dispose();
        }
        internal protected void update_transform_hierarchy_internal(float delta)
        {
            for (int i = 0; i < children.Count; i++)
            {
                Node? child = children[i];

                if (child_offsets.Count < i || !child_offsets.ContainsKey(child.UUID))
                    continue;

                Vector2 offset = child_offsets[child.UUID];

                if (child != null)
                    child.Position = Position + offset;
            }
        }
        #endregion
        #region Component Functions
        public Component AddComponent(Type type)
        {
            if (type.IsAbstract)
                throw new AccessViolationException("Can't create an instance of an abstract type");


            Component? v = (Component)Activator.CreateInstance(type);
            v.node = this;
            
            if(Components.ContainsKey(type))
                Components[type].Add(v);
            else Components.Add(type, new() { v });

            if (Interop.IsRunning && !v.awake)
                v.init_component_internal();

            SubscribeComponent(v);

            return v;
        }
        public T AddComponent<T>(T component) where T : Component
        {
            var type = component.GetType();

            if (type == typeof(Component))
                throw new InvalidOperationException("Generic type component was added.");

            if (type == typeof(Sprite))
                sprite = component as Sprite; 

            if (type == typeof(Rigidbody))
                rb = component as Rigidbody;

            // TODO: fix this - really refactor all of collision... this is a massive oversight - this completely ruins the functionality of multiple colliders and it makes adding triggers to nodes that use collision impossible.
            if (type == typeof(Collider))
                col = component as Collider;

            if (!Components.ContainsKey(type))
                Components.Add(type, new());

            Components[type].Add(component);
            component.node = this;

            if (Interop.IsRunning && !component.awake)
                component.init_component_internal();

            SubscribeComponent(component);

            return component;
        }
        private void SubscribeComponent(Component component)
        {
            if (Interop.Stage is Stage stage)
            {
                OnDestroyed         += component.on_destroy_internal;
                OnCollided          += component.on_collision_internal;
                OnTriggered         += component.on_trigger_internal;
                stage.Awake         += component.init_component_internal;
                stage.Update        += component.update_internal;
                stage.FixedUpdate   += component.fixed_update_internal;
                stage.OnDrawShapes  += component.on_draw_shapes_internal;
            }
        }
        private void UnsubscribeComponent(Component component)
        {
            if (Interop.Stage is Stage stage)
            {
                OnDestroyed         -= component.on_destroy_internal;
                OnCollided          -= component.on_collision_internal;
                OnTriggered         -= component.on_trigger_internal;   
                stage.Awake         -= component.init_component_internal;
                stage.Update        -= component.update_internal;
                stage.FixedUpdate   -= component.fixed_update_internal;
                stage.OnDrawShapes  -= component.on_draw_shapes_internal;
            }
        }
        /// <summary>
        /// Used to add a component of type <see cref="T"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T AddComponent<T>() where T : Component, new()
        {
            Type type = typeof(T);

            if (!Components.ContainsKey(type))
                Components.Add(type, new());

            T component = new T();
            AddComponent(component);
            return component;
        }
        /// <summary>
        /// Used to remove the specified instance of a component from this node.
        /// </summary>
        /// <param name="component"></param>
        public void RemoveComponent(Component component)
        {
            var type = component.GetType();

            // frees resources, could do this with reflection at the cost of computational expense, deletion is already kinda expensive.
            component.Dispose(); 

            UnsubscribeComponent(component);

            if (Components.ContainsKey(type))
            {
                if (type == typeof(Rigidbody))
                    rb = null;

                if (type == typeof(Collider))
                    col = null;

                if (Components[type].Contains(component))
                {
                    var compList = Components[type];

                    Component toRemove = null;

                    foreach (Component? comp in from comp in compList
                                                where comp is not null &&
                                                comp.UUID.Equals(component.UUID)
                                                select comp)
                    {
                        toRemove = comp;
                    }

                    if (toRemove is not null)
                        compList.Remove(toRemove);
                    if (compList.Count == 0) Components.Remove(type);
                }
            }
        }
        /// <summary>
        /// Check whether a node does or doesn't have a certain type of component.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>True if exists, false if doesn't.</returns>
        public bool HasComponent<T>() where T : Component
        {
            if (!Components.ContainsKey(typeof(T)))
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// Fetches a component only if it exists in this node.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        /// <param name="index"></param>
        /// <returns>False and null if component didn't exist, else true and component will be the component found.</returns>
        public bool TryGetComponent<T>([NotNullWhen(true)]out T component, int index = 0) where T : Component
        {
            if (!Components.ContainsKey(typeof(T)))
            {
                component = null;
                return false;
            }
            component = Components[typeof(T)][index] as T;

            if (component is null)
                return false;

            return true;
        }
        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>A list of components matching type T</returns>
        public IEnumerable<T> GetComponents<T>() where T : Component
        {
            return from Type type in Components.Keys
                   where type.IsAssignableTo(typeof(T))
                   from T component in Components[type]
                   select component;
        }
        /// <summary>
        /// Gets a component by type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index"></param>
        /// <returns>A component of type T if exists, else null.</returns>
        public T? GetComponent<T>(int index = 0) where T : Component
        {
            if (!Components.ContainsKey(typeof(T)))
                return null;
            T? component = Components[typeof(T)][index] as T;
            return component;
        }
        #endregion
    }
}
