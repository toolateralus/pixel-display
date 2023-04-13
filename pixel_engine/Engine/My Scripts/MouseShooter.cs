using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
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
        [Field] private Vector2 offsetFromCenter = new(0f, 0f);
        [Field] bool makeTransparent = false;
      

        public override void Cycle(Particle p)
        {
            p.position += p.velocity;
            p.velocity *= 0.99f;
            p.size = new Vector2(0.1f, 0.1f);
            p.color = Pixel.Random;

            if(p.velocity.Length() < minVelLength && particlesDieFromLowVelocity)
                p.onDeath?.Invoke(p);
        }
        public override void InstantiateParticle(Vector2 vel)
        {
            Particle particle = new(Pixel.Random, vel, Position, Vector2.One, Cycle, OnParticleDied);
            particle.polygon = Polygon.nGon(32);
            particles.Enqueue(particle);
        }

        public override void Awake()
        {
            RegisterAction(() =>
            {
                if (usingAmmo)
                {
                    currentMag--;
                    if (currentMag <= 0)
                    {
                        Runtime.Log("You must reload.");
                        return;
                    }
                }
                Vector2 vel = (CMouse.GlobalPosition - Position) / speed;
                GetParticle(vel); 
            } 
            , Key.G);

            RegisterAction(Reload, Key.R);

            if (makeTransparent)
            {

                if (TryGetComponent<Sprite>(out var sprite))
                    sprite.texture.GetImage().NormalizeAlpha(75);

                if (TryGetComponent<Animator>(out var anim))
                    if (anim.GetAnimation() is Animation clip)
                        foreach (var frame in clip.frames)
                            frame.Value?.NormalizeAlpha(75);
            }

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
       
    }
}
