using Newtonsoft.Json;
using pixel_renderer.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;

namespace pixel_renderer
{
    public class Node
    {
        // Node Info
        [JsonIgnore]
        public Stage ParentStage { get; set; }
        public string Name { get; set; }
        public string tag = "untagged - nyi";
        public Vec2 position = new();
        public Vec2 localPosition => parentNode == null ? position : parentNode.position - position;
        public Vec2 scale = new();
        public Node? parentNode;
        public List<Node> children= new() ;

        private string _uuid = "";
        public string UUID { get { return _uuid; } set { } }
        public bool Enabled { get { return _enabled; } }
        private bool _enabled = true; 
        public Dictionary<Type, List<Component>> Components { get; set; } = new Dictionary<Type, List<Component>>();
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
        public static Node New = new("", Vec2.zero, Vec2.one);

        public void SetActive(bool value)=>_enabled = value; 
        public T GetComponent<T>(int? index = 0) where T : Component
        {
            if (!Components.ContainsKey(typeof(T)))
            {
                throw new MissingComponentException();
            }
            T? component = Components[typeof(T)][index ?? 0] as T;
            return component;
        }
        public void AddComponent(Component component)
        {
            lock (Components)
            {
                if (component is null) return; 

                var type = component.GetType();

                if (type.BaseType != typeof(Component)) 
                    throw new InvalidOperationException("Cannot add generic type Component to node."); 

                if (!Components.ContainsKey(type))
                    Components.Add(type, new());

                Components[type].Add(component);
                component.parentNode = this;
            }
        }
        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns> A list of components matching typeof(T), otherwise an empty list of same type </returns>
        internal List<T> GetComponents<T>() where T : Component
        {
            lock (Components)
            {
                List<T> output = new();
                foreach (var component in ComponentsList)
                {
                    if (component.GetType().Equals(typeof(T)))
                        output.Add((T)component);
                }
                return output;
            }
        }
        /// <summary>
        /// Attempts to look for a component and push out if found.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        /// <returns>A boolean signifying the success of the operation, and out<T> instance of specified Component </returns>
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
        
        [JsonConstructor]
        public Node(Stage parentStage, string name, string tag, Vec2 position, Vec2 scale, Node? parentNode, List<Node> children, string nodeUUID)
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
        public Node(string name)
        {
            Name = name;
            _uuid = pixel_renderer.UUID.NewUUID();

        }
        public Node() { _uuid = pixel_renderer.UUID.NewUUID(); }
        public Node(string name, Vec2 pos, Vec2 scale)
        {
            _uuid = pixel_renderer.UUID.NewUUID();
            Name = name;
            position = pos;
            this.scale = scale;
        }public Node(string name, Vec2 pos, Vec2 scale, string UUID)
        {
            _uuid = UUID;
            Name = name;
            position = pos;
            this.scale = scale;
        }
        /// <summary>
        /// Init called to avoid having to implement base.Awake calls for boiler plate component init code
        /// </summary>
        public void Awake()
        {
            for (int i = 0; i < ComponentsList.Count; i++)
                ComponentsList[i].Init();
        }
        public void FixedUpdate(float delta)
        {
            for (int i = 0; i < ComponentsList.Count; i++)
                ComponentsList[i].FixedUpdate(delta);
        }
        public void Update()
        {
            for (int i = 0; i < ComponentsList.Count; i++)
                     ComponentsList[i].Update();
        }
        internal void OnCollision(Rigidbody otherBody)
        {
            lock (Components)
                for (int i = 0; i < ComponentsList.Count; i++)
                    ComponentsList[i].OnCollision(otherBody);
        }
        internal void OnTrigger(Rigidbody otherBody)
        {
            lock (Components)
                for (int i = 0; i < ComponentsList.Count; i++)
                    ComponentsList[i].OnTrigger(otherBody);
        }
        internal NodeAsset ToAsset() => new(this);
        internal void RemoveComponent(Component component)
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
    }
}
