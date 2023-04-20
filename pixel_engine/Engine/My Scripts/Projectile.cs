using pixel_renderer.FileIO;
using pixel_renderer.ShapeDrawing;
using System;
using System.Drawing;
using System.Numerics;
using System.Threading;
using static pixel_renderer.Assets.AssetLibrary;
namespace pixel_renderer
{
    public class Projectile : Component
    {
        public float hitRadius;
        public Node? sender;
        const string defaultImagePath = "\\Assets\\other\\ball.bmp";
        internal static Node Standard(Node sender, out Rigidbody rb)
        {
            Node node = Node.New;
            
            var proj = node.AddComponent<Projectile>();
            proj.hitRadius = 125;
            proj.sender = sender;

            Metadata meta = FetchMetaRelative(defaultImagePath);
            var sprite = node.AddComponent<Sprite>();
            sprite.Type = ImageType.Image;
            sprite.texture = new(meta.Path);
            sprite.IsDirty = true;

            var col = node.AddComponent<Collider>();

            col.Scale = new(1, 1);
            rb = node.AddComponent<Rigidbody>();
            node.Position = sender.Position; 
            return node; 
        }

        public override void Awake()
        {
            Runtime.Log("Projectile Awoken");
        }
        public override void FixedUpdate(float delta)
        {
            
        }
        public override void OnCollision(Collision collider)
        {
        }
        public override void OnDrawShapes()
        {
            if (sender is null)
                ShapeDrawer.DrawCircle(Position + Scale / 2, 16, Pixel.Blue);
              else
              {
                  ShapeDrawer.DrawLine(Position + Scale / 2, sender.Position + sender.Scale / 2, Pixel.White);
                  ShapeDrawer.DrawCircle(Position + Scale / 2, 16, Pixel.White);
              }
        }

        public override void OnTrigger(Collision other)
        {
        }
    }
}