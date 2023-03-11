using System;
using Key = System.Windows.Input.Key;
using System.Drawing;
using System.Numerics;
using static pixel_renderer.Input;
using pixel_renderer.ShapeDrawing;

namespace pixel_renderer
{
    public class ProjectileSource : Component
    {
        [Field]
        public Key fireKey = Key.F,
                   reloadKey = Key.R;
        [Field]
        public int ammoCt = 300, 
                   magazineSize = 16,
                   currentMag = 16;

        const int initAmmoCt = 300;

        [Field]
        public Node projectile = new(); 

        [Field]
        public Vector2 aimDirection = new(1, -3);

        [Field]
        private bool fired;
        public float aimDistance = 30f;
        

        public override void Awake()
        {
            ammoCt = initAmmoCt;
            projectile = Rigidbody.Standard("Projectile Original");
            projectile.AddComponent<Projectile>();
        }
        public override void FixedUpdate(float delta)
        {
            bool fireDown = Get(fireKey);
            bool fireUp = Get(fireKey, InputEventType.KeyUp);

            if (!fired && fireDown)
                Fire();
            else if (fireUp)
                fired = false;

            if (Get(reloadKey))
                Reload();
        }
        
        private void Fire()
        {   
            fired = true;
            var proj = Projectile.Standard(this.node, out var rb);
            rb.ApplyImpulse(new Vector2(1f, 0));
        }

        private void Reload()
        {
            ammoCt -= magazineSize;
            currentMag = magazineSize; 
        }

        public override void OnDrawShapes()
        {
        }
    }
}