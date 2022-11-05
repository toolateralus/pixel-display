using System;
using System.Collections.Generic;
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

        public Dictionary<Type, Component> Components { get; set; } = new Dictionary<Type, Component>();

        public T? GetComponent<T>()
        {
            var component = Components[typeof(T)];
            return (T)Convert.ChangeType(component, typeof(T));
        }
        public void AddComponent(Component component)
        {
            Components.Add(component.GetType(), component);
            component.parentNode = this;
        }
        /// <summary>
        /// Attempts to look for a component and push out if found.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="component"></param>
        /// <returns>A boolean signifying the success of the operation, and out<T> instance of specified Component </returns>
        public bool TryGetComponent<T>(out T? component) where T : Component
        {
            if (Components.ContainsKey(typeof(T)))
            {
                component = (T)Components[typeof(T)];
                return true;
            }
            component = null;
            return false;
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

        public event Action OnAwakeCalled;
        public event Action OnFixedUpdateCalled;

        // awake - to be called before first update; 
        public void Awake()
        {
            OnAwakeCalled?.Invoke();
            foreach (var component in Components)
            {
                component.Value.Awake();
            }
        }
        // update - if(usingPhysics) Update(); every frame.
        public void FixedUpdate(float delta)
        {
            OnFixedUpdateCalled?.Invoke();
            foreach (var component in Components)
            {
                component.Value.FixedUpdate(delta);
            }
        }
        public static Node New = new("", Vec2.zero, Vec2.one); 

    }
}
