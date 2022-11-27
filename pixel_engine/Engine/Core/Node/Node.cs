using Newtonsoft.Json;
using pixel_renderer.Assets;
using System;
using System.Collections.Generic;

namespace pixel_renderer
{
    public class Node
    {
        // Node Info
        [JsonIgnore]
        public Stage ParentStage { get; set; }
     
        public string Name { get; set; }

        private string _uuid = "";
        public string UUID { get { return _uuid; } set { } }

        public bool Enabled { get { return _enabled; } }
        private bool _enabled = true; 
        public void SetActive(bool value)=>_enabled = value; 

        public Vec2 position = new();
        public Vec2 localPosition
        {
            get => parentNode == null ? position : parentNode.position - position;
        }

        public Vec2 scale = new();

        public Node? parentNode;
        public Node[]? children;

        // goal - make private
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
            var type = component.GetType();
            if (!Components.ContainsKey(type))
            {
                Components.Add(type, new());
            }
            Components[type].Add(component);
            component.parentNode = this;
        }
        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns> A list of components matching typeof(T), otherwise an empty list of same type </returns>
        internal List<T> GetComponents<T>() where T : Component
        {
            List<T> output = new(); 
           foreach (var component in ComponentsList)
            {
                if (component.GetType().Equals(typeof(T)))
                    output.Add((T)component);
            }
            return output; 
        }
        /// <summary>
        /// Attempts to look for a component and push out if found.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        /// <returns>A boolean signifying the success of the operation, and out<T> instance of specified Component </returns>
        public bool TryGetComponent<T>(out T? component, int? index = 0) where T : Component
        {
            if (!Components.ContainsKey(typeof(T)))
            {
                component = null;
                return false;
            }
            component = Components[typeof(T)][index ?? 0] as T;
            return true;
        }
        /// <summary>
        /// Nameless, Position of (0,0), Scale of (1,1);
        /// </summary>
        public static Node New = new("", Vec2.zero, Vec2.one);
        public string tag = "untagged";

        public Node(Stage parentStage, string name, string tag, Vec2 position, Vec2 scale, Node? parentNode, Node[]? children)
        {
            this.ParentStage = parentStage;
            Name = name;
            _uuid = pixel_renderer.UUID.NewUUID();
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
        }
        /// <summary>
        /// Init called to avoid having to implement base.Awake calls for boiler plate component init code
        /// </summary>
        public void Awake()
        {
            foreach (var list in Components.Values)
                foreach (var component in list)
                    component.Init();

        }
        public void FixedUpdate(float delta)
        {
            foreach (var list in Components.Values)
                foreach (var component in list) component.FixedUpdate(delta);
        }
        public void Update()
        {
            foreach (var list in Components.Values)
                foreach (var component in list) component.Update();
        }
        internal void OnCollision(Rigidbody otherBody)
        {
            foreach (var list in Components.Values)
                foreach (var component in list) component.OnCollision(otherBody);
        }
        internal void OnTrigger(Rigidbody otherBody)
        {
            foreach (var list in Components.Values)
                foreach (var component in list) component.OnTrigger(otherBody);
        }
        public static void CreateGenericNode(List<Node> nodes, int i)
        {
            var pos = JRandom.ScreenPosition();
            var node = new Node($"NODE {i}", pos, Vec2.one);
            node.AddComponent(new Sprite(JRandom.Vec2(Vec2.one, Vec2.one * 15f), JRandom.Color(), JRandom.Bool()));
            node.AddComponent(new Rigidbody()
            {
                IsTrigger = false,
                usingGravity = true,
                drag = .1f
            });
            var randomDirection = JRandom.Direction();
            node.AddComponent(new Wind(randomDirection));
            nodes.Add(node);
        }
        internal NodeAsset ToAsset() => new(this);
           
    }
}
