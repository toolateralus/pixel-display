using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Animation;
using static pixel_renderer.Input;
using static pixel_renderer.ShapeDrawing.ShapeDrawer;
namespace pixel_renderer
{
    public class MouseShooter : ParticleSystem
    {
                 
        const int initAmmoCt = 300;
        [Field] int ammoCt = 300, magazineSize = 16, currentMag = 16;
        [Field] private bool usingAmmo = false;
        [Field] private Vector2 offsetFromCenter = new(0.6f, 0.6f);
        public override void Cycle(Particle p)
        {
            p.position += p.velocity;
            p.velocity *= 0.99f;
            p.size = Vector2.One;
            p.color = Pixel.Random;

            if(p.velocity.Length() < minVelLength && particlesDieFromLowVelocity)
                p.onDeath?.Invoke(p);
        }
        public override void Awake()
        {
            RegisterAction(() =>
            { 
                Vector2 vel = (CMouse.GlobalPosition - Position) / speed;
                GetParticle(vel); 
                if (usingAmmo)
                {
                    currentMag--;
                    if (currentMag <= 0)
                    {
                        Runtime.Log("You must reload.");
                        return;
                    }
                }
            } , Key.G);
            RegisterAction(Reload, Key.R);
            
            if(TryGetComponent<Sprite>(out var sprite)) 
                RemoveComponent(sprite);
            
            ammoCt = initAmmoCt;
            currentMag = magazineSize;
        }
        public override void OnParticleDied(Particle p)
        {
            p.dead = true;
        }
        private void Reload()
        {
            if (!usingAmmo)
                return;

            if (ammoCt <= 0)
            {
                Runtime.Log("Ammo Replenished! (+500)");
                ammoCt += 500; 
            }

            ammoCt -= currentMag - magazineSize;
            currentMag = magazineSize;
           
        }
        public override void FixedUpdate(float delta)
        {
            var cam = Camera.First;
           
            if (cam is null)
                return; 

            Position = (cam.Position + (offsetFromCenter * cam.Scale));
        }
        public override void OnDrawShapes()
        {
            if (Runtime.IsRunning)
            {
                foreach(var particle in particles)
                {
                    particle?.Next();
                    if (particle is null || particle.dead) 
                        continue;

                    DrawCircle(particle.position, particle.size.X, particle.color);
                }
            }
        }
        public override void Update()
        {

        }
    }
}
