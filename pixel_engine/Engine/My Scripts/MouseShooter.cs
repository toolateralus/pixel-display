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
        [Field] bool makeTransparent = false;

        public override void InstantiateParticle(Vector2 vel)
        {
            Particle particle = new(Pixel.Random, vel, Position, Vector2.One, Cycle, OnParticleDied);
            var col = particle.AddComponent<Collider>();
            col.SetModel(Polygon.Square(1));
            particle.AddComponent<Rigidbody>();
            var spr = particle.AddComponent<Sprite>();
            particles.Add(particle);
        }

        public override void Awake()
        {
            RegisterAction(this, () =>
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
                GetParticle(vel * 200); 
            } , Key.G);
            

            RegisterAction(this, Reload, Key.R);

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
        public override void Update()
        {
            foreach (var particle in particles)
                particle?.Next(); 
        }
        public override void OnDrawShapes()
        {
            if (Runtime.IsRunning)
            {
                foreach(var particle in particles)
                {
                    if (particle is null || particle.dead) 
                        continue;

                    DrawCircle(particle.Position, particle.Scale.X, particle.color);
                }
            }
        }
       
    }
}
