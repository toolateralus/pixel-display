using Newtonsoft.Json;
using Pixel.Types.Components;
using Pixel.Types.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Pixel
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
        private bool _enabled = true;
        private string _uuid = "";
        public bool awake;
        public bool ComponentsBusy { get; private set; }
        public Rigidbody? rb;
        public Collider col;

        public Action<Collision> OnCollided;
        public Action<Collision> OnTriggered;

        public Action OnDestroyed;
        public Queue<Action> ComponentActionQueue { get; private set; } = new();
        [JsonProperty] public Matrix3x2 Transform = Matrix3x2.Identity;
        [Field]
        [JsonProperty] public Stage parentStage;
        [JsonProperty]
        public Stage ParentStage
        {
            get
            {
                if (parentStage is null)
                    parentStage = Interop.Stage
                        ;
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
        internal Dictionary<string, Vector2> child_offsets = new();
        internal Sprite sprite;

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
        public void SetActive(bool value) => _enabled = value;

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

        #region Events/Messages

        public void Awake()
        {
            if (awake)
                return;

            ComponentsBusy = true;
            for (int i = 0; i < children.Count; i++)
            {
                Node? child = children[i];
                child.Awake();
            }
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
            if (!awake || !Enabled)  return;
            for (int i = 0; i < children.Count; i++)
            {
                Node? child = children[i];
                
                if (child_offsets.Count < i || !(child_offsets.ElementAt(i) is var kvp && kvp.Key != UUID))
                    continue;
                child.FixedUpdate(delta);
                child.Position = Position + kvp.Value;
            }
            ComponentsBusy = true;
            for (int i = 0; i < Components.Count; i++)
            {
                var pair = Components.ElementAt(i);
                for (int j = 0; j < pair.Value.Count; ++j)
                {
                    Component c = pair.Value.ElementAt(j);
                    if (c is not null && c.Enabled)
                        c.FixedUpdate(delta);
                }
            }
            ComponentsBusy = false;
        }
        public void Update()
        {
            if (!awake || !Enabled) return;

            ComponentsBusy = true;
            for (int i = 0; i < children.Count; i++)
            {
                Node? child = children[i];
                child.Update();
            }
            for (int i = 0; i < Components.Count; i++)
            {
                var pair = Components.ElementAt(i);
                var key = pair.Key;

                for (int j = 0; j < pair.Value.Count; ++j)
                {
                    Component c = pair.Value.ElementAt(j);
                    if (c is not null && c.Enabled)
                        c?.Update();
                }
            }
            while (ComponentActionQueue.Count > 0)
                ComponentActionQueue.Dequeue().Invoke();
            ComponentsBusy = false;
        }

        public void Destroy()
        {
            ComponentsBusy = true;

            foreach (var node in children)
            {
                node.parent = null;
                node.Destroy();
            }

            if (parent?.children != null)
                for (int i = 0; i < parent.children.Count; i++)
                {
                    Node? kvp = parent.children[i];
                    if (kvp == this)
                        parent.children.Remove(this);

                }

            ParentStage?.nodes.Remove(this);

            foreach (var component in Components)
                foreach (var comp in component.Value)
                {
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
            sprite = null;
            foreach (var component in Components)
                foreach (var c in component.Value)
                    c.Dispose();
        }

        public void OnTrigger(Collision otherBody)
        {
            if (!awake) return;
            ComponentsBusy = true;
            for (int i = 0; i < children.Count; i++)
            {
                Node? child = children[i];
                child.OnTrigger(otherBody);
            }
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
            if (!awake) return;
            ComponentsBusy = true;
            for (int i = 0; i < children.Count; i++)
            {
                Node? child = children[i];
                child.OnCollision(otherBody);
            }
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

        public void OnDrawShapes()
        {
            var compTypes = Components.Values;
            var typesCount = compTypes.Count;

            for (int i = 0; i < children.Count; i++)
            {
                Node? child = children[i];
                child.OnDrawShapes();
            }

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
        #endregion

        public T AddComponent<T>(T component) where T : Component
        {
            var type = component.GetType();

            if (type == typeof(Component))
                throw new InvalidOperationException("Generic type component was added.");

            if (type == typeof(Sprite))
                sprite = component as Sprite; 

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

                if (Interop.IsRunning)
                    component.Awake();
            }

            if (ComponentsBusy)
            {
                ComponentActionQueue.Enqueue(addComponent<T>);
            }
            else
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

        public bool HasComponent<T>() where T : Component
        {
            if (!Components.ContainsKey(typeof(T)))
            {
                return false;
            }
            return true;
        }

        public bool TryGetComponent<T>(out T component, int index = 0) where T : Component
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

        internal static Node Instantiate(Node projectile)
        {

            var clone = projectile.Clone();
            var stage = Interop.Stage;

            if (stage is null)
                throw new EngineInstanceException("Stage was not initialized during a clone or instantiate call.");
            clone.UUID = Pixel.Statics.UUID.NewUUID();

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

        internal void SubscribeToEngine(bool v, Stage stage)
        {
            if (v)
            {
                stage.Awake += Awake;
                stage.Update += Update;
                stage.FixedUpdate += FixedUpdate;
            }
            else
            {
                stage.Awake -= Awake;
                stage.Update -= Update;
                stage.FixedUpdate -= FixedUpdate;
            }

        }

        public static Node New => new("New Node", Vector2.Zero, Vector2.One);

    }
}
