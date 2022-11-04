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

        public Vec2 position = new Vec2();
        public Vec2 scale = new Vec2();

        public Node? parentNode;
        public Node[]? children;
        public Dictionary<Type, Component> Components { get; private set; } = new Dictionary<Type, Component>();


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
        public T? GetComponent<T>()
        {
            var component = Components[typeof(T)];
            return (T)Convert.ChangeType(component, typeof(T));
        }
        // Constructors 
        public Node(Stage parentStage, string name, string gUID, Vec2 position, Vec2 velocity, Vec2 scale, Node? parentNode, Node[]? children)
        {
            this.parentStage = parentStage;
            Name = name;
            UUID = gUID;
            this.position = position;
            this.scale = scale;
            this.parentNode = parentNode;
            this.children = children;
        }
        public Node(string name, string gUID)
        {
            UUID = gUID;
            Name = name;
        }
        public Node() { }
        public Node(string name, string gUID, Vec2 pos, Vec2 scale)
        {
            Name = name;
            UUID = gUID;
            position = pos;
            this.scale = scale;
        }

        public event Action OnAwakeCalled;
        public event Action OnFixedUpdateCalled;

        // awake - to be called before first update; 
        public void Awake(object? sender, EventArgs e)
        {
            OnAwakeCalled?.Invoke();
            foreach (var component in Components)
            {
                component.Value.Awake();
            }
        }
        // update - if(usingPhysics) Update(); every frame.
        public void FixedUpdate()
        {
            OnFixedUpdateCalled?.Invoke();
            foreach (var component in Components)
            {
                component.Value.FixedUpdate();
            }
        }

    }
}
