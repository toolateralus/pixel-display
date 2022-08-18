using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixelRenderer.Components; 
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
        public Vec2 velocity = new Vec2();
        public Vec2 scale = new Vec2();

        // hierarchy info
        public Node? parentNode;
        public Node[]? children;
        public Dictionary<Type, Component> Components { get; private set; } = new Dictionary<Type, Component>();
        public Rigidbody? rb;
        public Sprite? sprite;

        public void AddComponent(Component component) { Components.Add(component.GetType(),component); }
        public Component GetComponent<T1>() 
        {
            if (Components.ContainsKey(typeof(T1)))
            {
                return Components[key: typeof(T1)];
            }
            throw new Exception("invalid getComponent call"); 
        }
        // Constructors 
        public Node(Stage parentStage, string name, string gUID, Vec2 position, Vec2 velocity, Vec2 scale, Node? parentNode, Node[]? children, bool usingPhysics)
        {
            this.parentStage = parentStage;
            Name = name;
            UUID = gUID;
            this.position = position;
            this.velocity = velocity;
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
        public Node(string name, string gUID, Vec2 pos, Vec2 scale, bool usingPhysics)
        {
            this.Name = name;
            this.UUID = gUID;
            this.position = pos;
            this.scale = scale;
        }
        
        // awake - to be called before first update; 
        public void Awake(object? sender, EventArgs e)
        {
            // cross reference components and node
            foreach (object component in Components)
            { 
                if (component is Rigidbody)
                {
                    rb = component as Rigidbody;
                    if(rb != null) rb.parentNode = this;
                }
                if (component is Sprite)
                {
                    sprite = component as Sprite;
                    if(sprite != null) sprite.parentNode = this; 
                }
            }
            if (parentStage == null) return;
            if (UUID == null) this.UUID = PixelRenderer.Components.UUID.NewUUID();
            if (Name == null) Name = string.Empty;
            if (position == null) position = new Vec2(0, 0);
            if (velocity == null) velocity = new Vec2(0, 0);
            
        }

        // update - if(usingPhysics) Update(); every frame.
        public void Update()
        {
           if(rb!=null) rb.Update(); 
        }


    }
}
