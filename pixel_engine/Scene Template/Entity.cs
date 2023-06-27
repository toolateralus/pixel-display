using Pixel.Types.Components;
using Pixel.Types.Physics;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Shell;

namespace Pixel
{
    /// <summary>
    ///  base class for all actors/ entities
    /// </summary>
    public class Entity : Component
    {
        const int startHp = 100;
        [Field]
        int health = startHp;
        bool dead = false;

        public override void Awake()
        {
            AddTrigger(); 
        }

        internal List<Entity> hit_list = new();

        public override void OnTrigger(Collision collision)
        {
            if (collision.collider.TryGetComponent<Entity>(out var ent) && !hit_list.Contains(ent) && ent != this)
            {
                hit_list.Add(ent);
                Runtime.Log($"entity {ent.Name} added to {Name}'s hit-list (for melee)");
            }
        }

        public void Damage(int value)
        {
            if (dead)
                return;

            health -= value;

            if (health <= 0)
                node.Destroy();
        }

       

        public override void Dispose()
        {
            hit_list.Clear();    
        }
        public static Node Standard()
        {
            var node = Rigidbody.Standard().AddComponent<Entity>();
            // lel
            return node.node; 
        }

        public void AddTrigger()
        {
            Node triggerNode = Rigidbody.StaticBody();

            Sprite spr = triggerNode.GetComponent<Sprite>();
            triggerNode.RemoveComponent(spr);

            Collider collider = triggerNode.GetComponent<Collider>();

            collider.IsTrigger = true;
            collider.drawCollider = true;
            
            triggerNode.Scale = new Vector2(10, 5);
            triggerNode.OnTriggered += OnTrigger;
            

            triggerNode.tag = "INTANGIBLE";

            node.Child(triggerNode);
            node.UpdateChildLocal(triggerNode, Vector2.Zero);
        }
    }
}
