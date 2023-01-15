

using Newtonsoft.Json;

namespace pixel_renderer
{
    public class Collider : Component
    {
        [JsonProperty][Field]public Vec2 size = new(0,0);
        [JsonProperty][Field]public Sprite? sprite;
        [JsonProperty][Field]public TriggerInteraction InteractionType = TriggerInteraction.All;
        [JsonProperty]public bool IsTrigger { get; internal set; } = false;

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
