﻿using System;
using System.Collections.Generic;
namespace PixelRenderer.Components
{
    public class Node
    {
        // Node Info
        public Stage parentStage { get; set; }
        public string Name { get; set; }
        public string UUID { get; private set; }

        // Node Transform Info NYI
        public Vec2 position = new Vec2();
        public Vec2 scale = new Vec2();

        // hierarchy info
        public Node? parentNode;
        public Node[]? children;
        public Dictionary<Type, Component> Components { get; private set; } = new Dictionary<Type, Component>();
        public Rigidbody? rb;
        public Sprite? sprite;

        public void AddComponent(Component component)
        {
            Components.Add(component.GetType(), component);
            component.parentNode = this;
        }

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
            this.UUID = gUID;
            this.Name = name;
        }
        public Node() { }
        public Node(string name, string gUID, Vec2 pos, Vec2 scale)
        {
            this.Name = name;
            this.UUID = gUID;
            this.position = pos;
            this.scale = scale;
        }

        public event Action OnAwakeCalled;
        public event Action OnUpdateCalled;

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
        public void Update()
        {
            OnUpdateCalled?.Invoke(); 
            foreach (var component in Components)
            {
                component.Value.Update();  
            }
        }

    }
}
