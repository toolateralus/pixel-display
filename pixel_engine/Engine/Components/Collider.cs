

using Newtonsoft.Json;
using System.Windows.Documents;

namespace pixel_renderer
{
    public class Collider : Component
    {
        [JsonProperty] [Field] public Vec2 size = new(0,0);
        [JsonProperty] [Field] public Vec2[] normals;
        [JsonProperty] [Field] public Sprite? sprite;
        [JsonProperty] [Field] public TriggerInteraction InteractionType = TriggerInteraction.All;
        [JsonProperty]public bool IsTrigger { get; internal set; } = false;
        public Vec2[] GetNormals()
        {
            Vec2 pos = parent.position;
            var corners = GetCorners(pos);
            return new Vec2[]
            {
                (corners[1] - corners[0]).Normal_RHS,
                (corners[2] - corners[1]).Normal_RHS,
                (corners[3] - corners[2]).Normal_RHS,
                (corners[0] - corners[3]).Normal_RHS,
            };
        }

        private Vec2[] GetCorners(Vec2 pos) => new Vec2[] {
                    pos,
                    pos.WithValue(x: pos.x + size.x),
                    pos.WithValue(y: pos.y + size.y),
                    pos + size,
        };
        

        public override void Awake()
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
        public override void Update()
        {
        }
    }
}
