using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace pixel_renderer
{


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
            this.ParentStage = parentStage;
            this.parent = parentNode;
            this.children = children;

            // components
            this.Components = Components;


        }
        #endregion
        #region Other Constructors
        public Node()
        {
            if (Runtime.IsRunning && Runtime.Current.GetStage() is Stage stage)
                stage.AddNode(this);

            _uuid = pixel_renderer.UUID.NewUUID();
        }
        public Node(string name) : this() => Name = name;
        public Node Clone() { return (Node)MemberwiseClone(); }
        public Node(string name, Vector2 pos, Vector2 Scale) : this(name)
        {
            Position = pos;
            this.Scale = Scale;
        }
        #endregion
        private bool _enabled = true;
        private string _uuid = "";
        internal protected bool awake;
        public bool ComponentsBusy { get; private set; }
        internal Rigidbody? rb;
        internal Collider col;

        public Action<Collision> OnCollided;
        public Action<Collision> OnTriggered;

        public Action OnDestroyed; 
        public Queue<Action> ComponentActionQueue { get; private set; } = new();
        [JsonProperty] public Matrix3x2 Transform = Matrix3x2.Identity;
        [JsonProperty] public Stage parentStage;
        [JsonProperty] public Stage ParentStage 
        {
            get
            { 
                parentStage ??= Runtime.Current.GetStage();
                return parentStage ?? throw new NullStageException();
            } 
            set => parentStage = value; 
        }
        [JsonProperty] public bool Enabled { get { return _enabled; } set => _enabled = value; }
        [JsonProperty] public string UUID { get { return _uuid; } set { _uuid = value; } }
        [JsonProperty] public string Name { get; set; }
        [JsonProperty] public string tag = "Untagged";
        [JsonProperty] public Node? parent;
        [JsonProperty] public List<Node> children = new();
        [JsonProperty] public Dictionary<Type, List<Component>> Components { get; set; } = new Dictionary<Type, List<Component>>();
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
        public void Move(Vector2 destination)
        {
            Position = destination;
        }
        public void Child(Node child)
        {
            if (ContainsCycle(child))
            {
                Runtime.Log("A cyclic resource inclusion was detected.");
                return;
            }

            if (!Runtime.Current.GetStage().nodes.Contains(child))
                    Runtime.Current.GetStage()?.AddNode(child);

            children ??= new();
            
            if (children.Contains(child)) return;

            
            _ = child.parent?.TryRemoveChild(child);
            
            children.Add(child);
            
            child.parent = this;

        }
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
        public bool TryRemoveChild(Node child)
        {
            ComponentsBusy = true;

            foreach (var kvp in children)
                if (kvp == child)
                {
                    children.Remove(kvp);
                    child.parent = null;
                    return true;
                }
            ComponentsBusy = false;
            return false;

        }
        public void SetActive(bool value) => _enabled = value;
        public void Awake()
        {
            ComponentsBusy = true;
            for (int i = 0; i < Components.Count; i++)
            {
                var pair = Components.ElementAt(i);
                var key = pair.Key;

                for (int j = 0; j < Components[key].Count; ++j)
                        Components[key].ElementAt(j).init_component_internal();
            }
            awake = true; 
            ComponentsBusy = false;
        }
        public void FixedUpdate(float delta)
        {
            if (!awake) 
                return;
            ComponentsBusy = true;

            for (int i = 0; i < Components.Count; i++)
            {
                var pair = Components.ElementAt(i);
                var key = pair.Key;

                for (int j = 0; j < Components[key].Count; ++j)
                    Components[key].ElementAt(j)?.FixedUpdate(delta);
            }
            ComponentsBusy = false;

        }
        public void Update()
        {
            if (!awake) 
                return;

            ComponentsBusy = true;

            for (int i = 0; i < Components.Count; i++)
            {
                var pair = Components.ElementAt(i);
                var key = pair.Key; 

                for (int j = 0; j < Components[key].Count; ++j)
                    Components[key].ElementAt(j)?.Update();
            }


            while(ComponentActionQueue.Count > 0)
                ComponentActionQueue.Dequeue().Invoke();

            ComponentsBusy = false;
        }
        public void Destroy()
        {
            ComponentsBusy = true;

            foreach (var kvp in children)
                kvp.parent = null;

            if (parent?.children != null)
                foreach (var kvp in parent.children)
                    if (kvp == this) { 
                        parent.children.Remove(this);
                    }

            ParentStage?.nodes.Remove(this);

            foreach (var component in Components)
                foreach(var comp in component.Value){
                    comp.on_destroy_internal();
                }

            Dispose();
            OnDestroyed?.Invoke();
            ComponentsBusy = false;

            
        }
        public virtual void Dispose()
        {
            rb = null;
            col = null;
        }
        public void OnTrigger(Collision otherBody)
        {
            if (!awake)
                return;
            ComponentsBusy = true;

            for (int i = 0; i < Components.Count; i++)
            {
                var pair = Components.ElementAt(i);
                var key = pair.Key;

                for (int j = 0; j < Components[key].Count; ++j)
                    Components[key].ElementAt(j)?.OnTrigger(otherBody);
            }
            OnTriggered?.Invoke(otherBody);
            ComponentsBusy = false;

        }
        public void OnCollision(Collision otherBody)
        {
            if (!awake)
                return;

            ComponentsBusy = true;
            for (int i = 0; i < Components.Count; i++)
            {
                var pair = Components.ElementAt(i);
                var key = pair.Key;

                for (int j = 0; j < Components[key].Count; ++j)
                    Components[key].ElementAt(j)?.OnCollision(otherBody);
            }
            OnCollided?.Invoke(otherBody);
            ComponentsBusy = false;
        }
        public T AddComponent<T>(T component) where T : Component
        {
            var type = component.GetType();

            if (type == typeof(Component))
                throw new InvalidOperationException("Generic type component was added.");

            if (type == typeof(Rigidbody))
                rb = component as Rigidbody;

            if (type == typeof(Collider))
                col = component as Collider;

            void addComponent<T>() where T : Component
            {
                if (!Components.ContainsKey(type))
                    Components.Add(type, new());

                Components[type].Add(component);
                component.node = this;

                if (Runtime.IsRunning)
                    component.Awake();
            }

            if (ComponentsBusy)
            {
                ComponentActionQueue.Enqueue(addComponent<T>);
            }else 
                addComponent<T>();


            return component;

           
        }
        public T AddComponent<T>() where T : Component, new()
        {
            Type type = typeof(T);
            if (!Components.ContainsKey(type))
                Components.Add(type, new());

            T component = new T();
            AddComponent(component);
            return component;
        }
        public bool HasComponent<T>() where T : Component
        {
            if (!Components.ContainsKey(typeof(T)))
            {
                return false;
            }
            return true;
        }
        public bool TryGetComponent<T>(out T component, int? index = 0) where T : Component
        {
            if (!Components.ContainsKey(typeof(T)))
            {
                component = null;
                return false;
            }
            component = Components[typeof(T)][index ?? 0] as T;
                
            if (component is null) 
                return false;

            return true;
        }
        public IEnumerable<T> GetComponents<T>() where T : Component
        {
            return from Type type in Components.Keys
                    where type.IsAssignableTo(typeof(T))
                    from T component in Components[type]
                    select component;
        }
        public T? GetComponent<T>(int index = 0) where T : Component
        {
            if (!Components.ContainsKey(typeof(T)))
                return null;
            T? component = Components[typeof(T)][index] as T;
            return component;
        }
        public void RemoveComponent(Component component)
        {
            var type = component.GetType();

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
        public void OnDrawShapes()
        {
            var compTypes = Components.Values;
            var typesCount = compTypes.Count;
            for (int iType = 0; iType < typesCount; iType++)
            {
                var compList = compTypes.ElementAt(iType);
                var compCount = compList.Count;
                for (int iComp = 0; iComp < compCount; iComp++)
                {
                    compList[iComp].OnDrawShapes();
                }
            }
        }
        internal static Node Instantiate(Node projectile)
        {
            
            var clone = projectile.Clone();
            var stage = Runtime.Current.GetStage();

            if (stage is null)
                throw new EngineInstanceException("Stage was not initialized during a clone or instantiate call.");
            clone.UUID = pixel_renderer.UUID.NewUUID();

            stage.AddNode(clone);

            if (clone.ParentStage is null || clone.parentStage != stage)

            clone.Awake();
            return clone; 
        }
        internal static Node Instantiate(Node projectile, Vector2 position)
        {
            var clone = Instantiate(projectile);
            clone.Position = position;
            return clone; 
        }
        public static Node New => new("New Node", Vector2.Zero, Vector2.One);
        
    }
}
