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
            sprite.Type = SpriteType.Image;
            sprite.texture = new(meta.Path);
            sprite.IsDirty = true;
            node.Transform = Matrix3x2.CreateScale(35);

            var col = node.AddComponent<Collider>();
            col.untransformedPolygon = new Circle().DefiningGeometry;
            col.Scale = new(25, 25);
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
            if (sender != null)
                Position = sender.Position + Vector2.One * 2; 
        }
        public override void OnCollision(Collider collider)
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

        public override void OnTrigger(Collider other)
        {
        }
    }
}