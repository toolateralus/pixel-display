using Pixel.Types.Components;
using Pixel.Types.Physics;
using System.Collections.Generic;

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
                Die();

        }

        private void Die()
        {
            health = 0;
            dead = true; 
        }

        public override void Dispose()
        {
             
        }
        public static Node Standard()
        {
            var node = Rigidbody.Standard().AddComponent<Entity>();
            // lel
            return node.node; 
        }
    }
}
