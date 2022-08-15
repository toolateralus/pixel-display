using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JoshDisplay.Classes; 
namespace JoshDisplay.Classes
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
        public List<object> Components = new List<object>();
        public Rigidbody? rb;
        public Sprite? sprite; 

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

        // rigidbody info
        
        // awake - to be called before first update; 
        public void Awake(object? sender, EventArgs e)
        {
            foreach (object component in Components)
            { 
                if (component as Rigidbody != null)
                {
                    rb = component as Rigidbody;
                    rb.node = this;
                }
                if (component as Sprite != null)
                {
                    sprite = component as Sprite;
                }
            }
            if (parentStage == null) return;
            if (UUID == null) this.UUID = Classes.UUID.NewUUID();
            if (Name == null) Name = string.Empty;
            if (position == null) position = new Vec2(0, 0);
            if (velocity == null) velocity = new Vec2(0, 0);
            
        }


        // update - if(usingPhysics) Update(); every frame.
        public void Update()
        {
            rb.Update(); 
        }


    }
}
