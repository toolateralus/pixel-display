using static pixel_renderer.Input;
using Key = System.Windows.Input.Key;
using System.Drawing;
using System.Numerics;
using System;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

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
        public Vector2 aimDirection = new(1, 1);

        [Field]
        public float aimDistance = 30f;
        private bool fired;
        private float power = 1f;

        public override void Awake()
        {
            ammoCt = initAmmoCt;
            projectile = Rigidbody.Standard("Projectile Original");
            projectile.AddComponent<Projectile>();
        }

        public override void FixedUpdate(float delta)
        {

            bool fireDown = Get(ref fireKey);
            bool fireUp = Get(ref fireKey, InputEventType.KeyUp);

            if (!fired && fireDown)
                Fire();
            else if (fireUp)
                fired = false;

            if (Get(ref reloadKey))
                Reload();


        }

        private void Fire()
        {
            var proj = Node.Instantiate(this.projectile);
            proj.Position = Vector2.Zero;


            if (!proj.TryGetComponent(out Rigidbody rb))
            {
                proj?.Destroy();
                return;
            }
            rb?.ApplyImpulse(aimDirection * power);
            
            if (proj.TryGetComponent(out Projectile projectile))
            {
                projectile.sender = node;
                projectile.hitRadius = 16; 
            }

            Task followNodeTask = new(async delegate 
            {
                FocusNodeEvent e = new(proj);
                Runtime.RaiseInspectorEvent(e);
                await Task.Delay(1000 * 5);
                proj?.Destroy();
            });

            followNodeTask.Start(); 
        }

        private void Reload()
        {
            ammoCt -= magazineSize;
            currentMag = magazineSize; 
        }

        public override void OnDrawShapes()
        {
            ShapeDrawer.DrawLine(Position, aimDirection * aimDistance, Color.Red);
        }
    }
}