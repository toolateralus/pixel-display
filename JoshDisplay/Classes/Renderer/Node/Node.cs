using System;
using System.Collections.Generic;
using System.Reflection;

namespace pixel_renderer
{
    public class Node
    {
        // Node Info
        public Stage parentStage { get; set; }
        public string Name { get; set; }
        public string UUID { get; private set; }

        public Vec2 position = new();
        public Vec2 localPosition => GetLocalPosition(position);
        public Vec2 GetLocalPosition(Vec2 position)
        {
            Vec2 local = new();
            if (parentNode == null) return position;
            local = parentNode.position - position; 
            return local;
        }
        public Vec2 scale = new();

        public Node? parentNode;
        public Node[]? children;

        public Dictionary<Type, List<Component>> Components { get; set; } = new Dictionary<Type, List<Component>>();

        public T? GetComponent<T>(int? index = 0) where T : Component
        {
            if(!Components.ContainsKey(typeof(T))) 
            {
                throw new MissingComponentException(); 
            }
            var component = Components[typeof(T)][index ?? 0] as T;
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
        
        // Constructors 
        public Node(Stage parentStage, string name, Vec2 position, Vec2 scale, Node? parentNode, Node[]? children)
        {
            this.parentStage = parentStage;
            Name = name;
            UUID = UuID.NewUUID();
            this.position = position;
            this.scale = scale;
            this.parentNode = parentNode;
            this.children = children;
        }
        public Node(string name)
        {
            UUID = UuID.NewUUID();
            Name = name;
        }
        public Node() 
        {
            UUID = UuID.NewUUID(); 
        }
        public Node(string name, Vec2 pos, Vec2 scale)
        {
            Name = name;
            UUID = UuID.NewUUID();
            position = pos;
            this.scale = scale;
        }

        public void Awake()
        {
            foreach (var list in Components.Values)
                foreach(var component in list) component.Awake();
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
        public static Node New = new("", Vec2.zero, Vec2.one); 

    }
}
