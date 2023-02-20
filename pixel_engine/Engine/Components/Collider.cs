

using Newtonsoft.Json;
using System;
using System.Windows.Documents;

namespace pixel_renderer
{
    public class Collider : Component
    {
        [JsonProperty] Polygon mesh;
        public Polygon Mesh
        {
            get => mesh ??= new(GetVertices());
            set => mesh = value;
        }
        [JsonProperty] [Field] public Vec2 size = new(0,0);
        [JsonProperty] [Field] public Sprite? sprite;
        [JsonProperty] [Field] public TriggerInteraction InteractionType = TriggerInteraction.All;
        [JsonProperty]public bool IsTrigger { get; internal set; } = false;
        /// <summary>
        /// <code>
        /// Gets the colliders corners in a list organized as such
        /// Top Left, 
        /// Top Right,
        /// Bottom Right,
        /// Bottom Left,
        /// </code>
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Vec2[] GetVertices()
        {
            Vec2 position = new(parent.Position);
            Vec2 size = new(this.size);

            Vec2 topLeft = position;
            Vec2 topRight = position.WithValue(x: position.x + size.x);
            Vec2 bottomRight = position + size;
            Vec2 bottomLeft = position.WithValue(y: position.y + size.y);

            var vertices = new Vec2[] {
                    topLeft,
                    topRight,
                    bottomRight,
                    bottomLeft,
            };

            return vertices;
        }

        public override void Awake()
        { 

        }
        public override void Update()
        {
        }
        public override void FixedUpdate(float delta)
        {

        }
        public override void OnCollision(Collider collider)
        {


        }
        public override void OnTrigger(Collider other)
        {


        }
    }
}
