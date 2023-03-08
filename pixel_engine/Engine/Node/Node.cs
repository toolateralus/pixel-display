using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace pixel_renderer
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Node
    {
        #region Json Constructor
        [JsonConstructor]
        public Node(bool Enabled, Stage parentStage, Dictionary<Type, List<Component>> Components, string name, string tag, Vector2 position, Vector2 Scale, Node? parentNode, Dictionary<Vector2, Node> children, string nodeUUID)
        {
            this.ParentStage = parentStage;
            Name = name;
            _uuid = nodeUUID;
            this.Position = position;
            this.Scale = Scale;
            this.parent = parentNode;
            this.children = children;
            this.tag = tag;
            this.Components = Components;
            this.Enabled = Enabled;

        }
        #endregion
        #region Other Constructors
        public Node()
        {
            _uuid = pixel_renderer.UUID.NewUUID();
        }

        public Node(string name) : this() => Name = name;
        private protected Node Clone() { return (Node)MemberwiseClone(); }
        public Node(string name, Vector2 pos, Vector2 Scale) : this(name)
        {
            Position = pos;
            this.Scale = Scale;
        }

        #endregion


        [JsonProperty]
        public Stage parentStage;
        public Stage ParentStage 
        {
            get
            { 
                parentStage ??= Runtime.Current.GetStage();
                return parentStage ?? throw new NullStageException();
            } 
            set => parentStage = value; 
        }


        [JsonProperty]
        public bool Enabled { get { return _enabled; } set => _enabled = value; }

        [JsonProperty]
        public string UUID { get { return _uuid; } set { _uuid = value; } }

        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public string tag = "Untagged";

        private bool _enabled = true;
        private string _uuid = "";

        internal protected int hiearchyLevel = 0; 


        Rigidbody? rb;
        public void Move(Vector2 destination)
        {
            Position = destination;
        }
        [JsonProperty] public Vector2 localPos = new();
        
        [JsonProperty] public Node? parent;
       
        [JsonProperty] public Dictionary<Vector2, Node> children = new();
        [JsonProperty] public Dictionary<Type, List<Component>> Components { get; set; } = new Dictionary<Type, List<Component>>();
        [Field]
        [JsonProperty] public Matrix3x2 Transform = Matrix3x2.Identity;
        private bool awake;

        public Vector2 Position
        {
            get => Transform.Translation;
            set
            {
                Transform.Translation = value;
                UpdateTransform(this); 
            }
        }
        [JsonProperty]
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

                UpdateTransform(this);
            }
        }
        [JsonProperty]
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

                UpdateTransform(this);
            }
        }
     
        static void UpdateTransform(Node node)
        {
            var parentMatrix = Matrix3x2.Identity; 

            if (node.parent != null)
            {
                parentMatrix = node.parent.Transform; 
                UpdateTransform(node.parent); 
            }

            var rotationMatrix = Matrix3x2.CreateRotation(node.Rotation);
            var scaleMatrix = Matrix3x2.CreateScale(node.Scale);
            var translationMatrix = Matrix3x2.CreateTranslation(node.Position);
            
            var transformMatrix = parentMatrix * rotationMatrix * scaleMatrix * translationMatrix;

            node.Transform = transformMatrix;
        }

        public void Child(Node child)
        {

            if (ContainsCycle(child))
            {
                Runtime.Log("A cyclic resource inclusion was detected.");
                return;
            }

            if (!Runtime.Current.GetStage().nodes.Contains(child))
                    Runtime.Current.GetStage().AddNode(child);

            var distance = Vector2.Distance(child.Position, Position);
            var direction = child.Position - Position;

            children ??= new();

            if (children.ContainsKey(direction * distance))
                return;

            _ = child.parent?.TryRemoveChild(child);

            child.localPos = child.Position - Position;

            children.Add(direction * distance, child);

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
                foreach (var child in current.children.Values)
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
            foreach (var kvp in children)
                if (kvp.Value == child)
                {
                    children.Remove(kvp.Key);
                    child.parent = null;
                    return true;
                }
            return false; 
        }
        public void Awake()
        {
            for (int i = 0; i < Components.Count; i++)
            {
                var pair = Components.ElementAt(i);
                var key = pair.Key;

                for (int j = 0; j < Components[key].Count; ++j)
                        Components[key].ElementAt(j).init_component_internal();
                awake = true; 
            }
        }
        public void FixedUpdate(float delta)
        {
            if (!awake) 
                return;
            for (int i = 0; i < Components.Count; i++)
            {
                var pair = Components.ElementAt(i);
                var key = pair.Key;

                for (int j = 0; j < Components[key].Count; ++j)
                    Components[key].ElementAt(j)?.FixedUpdate(delta);
            }
        }
        public void Update()
        {
            if (!awake) 
                return; 

            for (int i = 0; i < Components.Count; i++)
            {
                var pair = Components.ElementAt(i);
                var key = pair.Key; 

                for (int j = 0; j < Components[key].Count; ++j)
                    Components[key].ElementAt(j)?.Update();
            }
           

        }
        
        public void SetActive(bool value) => _enabled = value;
        public void Destroy() => ParentStage?.nodes.Remove(this);
        public void OnTrigger(Collider otherBody)
        {
            if (!awake)
                return;

            for (int i = 0; i < Components.Count; i++)
            {
                var pair = Components.ElementAt(i);
                var key = pair.Key;

                for (int j = 0; j < Components[key].Count; ++j)
                    Components[key].ElementAt(j)?.OnTrigger(otherBody);
            }
        }
        public void OnCollision(Collider otherBody)
        {
            if (!awake)
                return;

            for (int i = 0; i < Components.Count; i++)
            {
                var pair = Components.ElementAt(i);
                var key = pair.Key;

                for (int j = 0; j < Components[key].Count; ++j)
                    Components[key].ElementAt(j)?.OnCollision(otherBody);
            }
        }

        public T AddComponent<T>(T component) where T : Component
        {
            var type = component.GetType();
                
            if (type == typeof(Component))
                throw new InvalidOperationException("Generic type component was added.");

            if (type == typeof(Rigidbody))
                rb = component as Rigidbody;

            if (!Components.ContainsKey(type))
                Components.Add(type, new());

            Components[type].Add(component);
            component.node = this;

            if(Runtime.IsRunning)
                component.Awake();

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
            if (Components[type].Contains(component))
            {
                var compList = Components[type];
                var toRemove = new Component();
                foreach (var comp in from comp in compList
                                        where comp is not null &&
                                        comp.UUID.Equals(component.UUID)
                                        select comp)
                toRemove = comp;

                if (toRemove is not null)
                    compList.Remove(toRemove);
                if (compList.Count == 0) Components.Remove(type);
            }
        }
        public bool TryGetComponent<T>(out T? component, int? index = 0) where T : Component
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
        public bool HasComponent<T>() where T : Component
        {
            if (!Components.ContainsKey(typeof(T)))
            {
                return false;
            }
            return true;
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
