using pixel_core.types.Components;
using pixel_core.FileIO;
using static pixel_core.Assets.AssetLibrary;

namespace pixel_core
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
        public override void Dispose()
        {
            sender = null;
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
    }
}