using Newtonsoft.Json;
using pixel_renderer.Assets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Documents;

namespace pixel_renderer
{
    public class Node
    {
        #region Json Constructor
        [JsonConstructor] public Node(Stage parentStage, 
            string name, string tag, 
            Vec2 position, Vec2 scale,
            Node? parentNode, List<Node> children,
            string nodeUUID)
        {
            this.ParentStage = parentStage;
            Name = name;
            _uuid = nodeUUID;
            this.position = position;
            this.scale = scale;
            this.parentNode = parentNode;
            this.children = children;
            this.tag = tag;
        }
        #endregion
        #region Other Constructors
        public Node() =>  _uuid = pixel_renderer.UUID.NewUUID(); 
        public Node(string name) : this() => Name = name;
        public Node Clone() { return (Node)Clone(); }
        public Node(string name, Vec2 pos, Vec2 scale) : this(name)
        {
            position = pos;
            this.scale = scale;
        }
        public Node(string name, Vec2 pos, Vec2 scale, string UUID) : this(name, pos, scale)
        {
            position = pos;
            this.scale = scale;
        }
        #endregion

        [JsonIgnore]
        public Stage ParentStage { get; set; }
        
        public bool Enabled { get { return _enabled; } }
        private bool _enabled = true; 
        
        public string Name { get; set; }
        public string tag = "untagged - nyi";

        public string UUID { get { return _uuid; } set { } }
        private string _uuid = "";

        
        public Vec2 position = new();
        public Vec2 localPosition => parentNode == null ? position : parentNode.position - position;
        public Vec2 scale = new();
        
        public Node? parentNode;
        public List<Node> children= new() ;
        public static Node New = new("", Vec2.zero, Vec2.one);
        
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
        public Dictionary<Type, List<Component>> Components { get; set; } = new Dictionary<Type, List<Component>>();
        
        public void Awake()
        {
            for (int i = 0; i < ComponentsList.Count; i++)
                  ComponentsList[i].init_component_internal();
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
        public void Destroy() => ParentStage.Nodes.Remove(this);
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
                if (type is Component)
                    throw new Exception(); 
                if (!Components.ContainsKey(type))
                    Components.Add(type, new());

                Components[type].Add(component);
                component.parent = this;

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
        public T GetComponent<T>(int? index = 0) where T : Component
        {
            if (!Components.ContainsKey(typeof(T)))
            {
                throw new MissingComponentException();
            }
            T? component = Components[typeof(T)][index ?? 0] as T;
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

        public NodeAsset ToAsset() => new(this);
    }
}
