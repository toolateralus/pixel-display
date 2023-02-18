using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows;

namespace pixel_renderer
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Node
    {
        #region Json Constructor
        [JsonConstructor]
        public Node(bool Enabled, Stage parentStage, Dictionary<Type, List<Component>> Components, string name, string tag, Vec2 position, Vec2 scale, Node? parentNode, Dictionary<Vec2, Node> children, string nodeUUID)
        {
            this.ParentStage = parentStage;
            Name = name;
            _uuid = nodeUUID;
            this.Position = position;
            this.scale = scale;
            this.parentNode = parentNode;
            this.children = children;
            this.tag = tag;
            this.Components = Components;
            this.Enabled = Enabled;

        }
        #endregion
        #region Other Constructors
        public Node() => _uuid = pixel_renderer.UUID.NewUUID();
        public Node(string name) : this() => Name = name;
        public Node Clone() { return (Node)Clone(); }
        public Node(string name, Vec2 pos, Vec2 scale) : this(name)
        {
            Position = pos;
            this.scale = scale;
        }

        #endregion

        [JsonProperty]
        public Stage ParentStage { get; set; }

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

        Rigidbody? rb;
        public void Move(Vec2 destination)
        {
            Position = destination;
        }
        [JsonProperty] public Vec2 localPos = new();

        public Vec2 Position
        {
            get => parentNode == null ? localPos : localPos + parentNode.Position;
            set => localPos = parentNode == null ? value : value - parentNode.Position;
        }
        [JsonProperty] public Vec2 scale = new();

        [JsonProperty]
        public Node? parentNode;
        [JsonProperty]
        public Dictionary<Vec2, Node> children = new();

        public List<Component> ComponentsList
        {
            get
            {
                var list = new List<Component>();
                foreach (var componentType in Components)
                    foreach (var component in componentType.Value)
                        list.Add(component);
                return list ?? new();
            }
        }

        [JsonProperty] public Dictionary<Type, List<Component>> Components { get; set; } = new Dictionary<Type, List<Component>>();
        
        public void Child(Node child)
        {
            var distance = Vec2.Distance(child.Position, Position);
            var direction = child.Position - Position;
            if (children.ContainsKey(direction * distance))
                return;
            _ = child.parentNode?.TryRemoveChild(child);
            child.localPos = child.Position - Position;
            children.Add(direction * distance, child);
            child.parentNode = this;
        }
        public bool TryRemoveChild(Node child)
        {
            foreach (var kvp in children)
                if (kvp.Value == child)
                {
                    children.Remove(kvp.Key);
                    child.parentNode = null;
                    return true;
                }
            return false; 
        }
        public void Awake()
        {
            foreach(var Components in Components.Values)
                foreach(var component in Components)
                    component.init_component_internal();
        }
        public void FixedUpdate(float delta)
        {
            lock (Components)
            {
                for (int i = 0; i < ComponentsList.Count; i++)
                      ComponentsList[i].FixedUpdate(delta);
            }
        }
        public void Update()
        {
            lock (Components)
            {
                for (int i = 0; i < ComponentsList.Count; i++)
                    ComponentsList[i].Update();
            }
           

        }
        
        public void SetActive(bool value) => _enabled = value;
        public void Destroy() => ParentStage.nodes.Remove(this);
        public void OnTrigger(Collider otherBody)
        {
            lock (Components)
                for (int i = 0; i < ComponentsList.Count; i++)
                  ComponentsList[i].OnTrigger(otherBody);
        }
        public void OnCollision(Collider otherBody)
        {
            lock (Components)
                for (int i = 0; i < ComponentsList.Count; i++)
                  ComponentsList[i].OnCollision(otherBody);
        }

        public T AddComponent<T>(T component) where T : Component
        {
            lock (Components)
            {
                var type = component.GetType();
                
                if (type == typeof(Component))
                    throw new InvalidOperationException("Generic type component was added.");

                if (type == typeof(Rigidbody))
                    rb = component as Rigidbody;

                if (!Components.ContainsKey(type))
                    Components.Add(type, new());

                Components[type].Add(component);
                component.parent = this;
                component.Awake();
                return component;
            }
        }
        public T AddComponent<T>() where T : Component, new()
        {
            lock (Components)
            {
                Type type = typeof(T);
                if (!Components.ContainsKey(type))
                    Components.Add(type, new());

                T component = new T();
                AddComponent(component);
                return component;
            }
        }

        public void RemoveComponent(Component component)
        {
            lock (Components)
            {
                var type = component.GetType();
                if (ComponentsList.Contains(component))
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
                }
            }
        }
        public bool TryGetComponent<T>(out T? component, int? index = 0) where T : Component
        {
            lock (Components)
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
        }

        public IEnumerable<T> GetComponents<T>() where T : Component
        {
            lock (Components)
            {
                return from Type type in Components.Keys
                       where type.IsAssignableTo(typeof(T))
                       from T component in Components[type]
                       select component;
            }
        }
        public T GetComponent<T>(int index = 0) where T : Component
        {
            if (!Components.ContainsKey(typeof(T)))
            {
                throw new MissingComponentException();
            }
            T? component = Components[typeof(T)][index] as T;
            return component;
        }
        public bool HasComponent<T>() where T : Component
        {
            lock (Components)
            {
                if (!Components.ContainsKey(typeof(T)))
                {
                    return false;
                }
                return true;
            }
        }

        public static Node New => new("New Node", Vec2.zero, Vec2.one);
    }
}
