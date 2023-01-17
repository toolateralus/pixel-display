

using Newtonsoft.Json;
using System;
using System.Windows.Documents;

namespace pixel_renderer
{
    public class Collider : Component
    {
        [JsonProperty] [Field] public Vec2 size = new(0,0);
        [JsonProperty] [Field] public Sprite? sprite;
        [JsonProperty] [Field] public TriggerInteraction InteractionType = TriggerInteraction.All;
        [JsonProperty]public bool IsTrigger { get; internal set; } = false;
        public Vec2[] normals => GetNormals();
        /*
          * CORNERS
     

         NORMALS
         Top,
         Left,
         Bottom,
         Right

         */
        /// <summary>
        /// <code>
        /// returns a list of the normals organizes as such
        /// Top
        /// Left
        /// Bottom
        /// Right
        /// </code>
        /// </summary>
        /// <returns></returns>
        public Vec2[] GetNormals()
        {
            Vec2 pos = parent.position;
            var corners = GetVertices();
         
            return new Vec2[]
            {
                (corners[1] - corners[0]).Normal_RHS.Normalize(),
                (corners[2] - corners[1]).Normal_RHS.Normalize(),
                (corners[3] - corners[2]).Normal_RHS.Normalize(),
                (corners[0] - corners[3]).Normal_RHS.Normalize(),
            };
        }
        /// <summary>
        /// <code>
        /// Gets the colliders corners in a list organized as such
        /// Top Left, 
        /// Top Right,
        /// Bottom Left,
        /// Bottom Right
        /// </code>
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Vec2[] GetVertices()
        {
            Vec2 position = new(parent.position);
            Vec2 size = new(this.size);

            Vec2 topLeft = position;
            Vec2 topRight = position.WithValue(x: position.x + size.x);
            Vec2 bottomLeft = position.WithValue(y: position.y + size.y);
            Vec2 bottomRight = position + size;

            var vertices = new Vec2[] {
                    topLeft,
                    topRight,
                    bottomLeft,
                    bottomRight,
            };

            return vertices;
        }
        /// <summary>
        /// returns the center of the polygon the collider represents.
        /// </summary>
        /// <returns></returns>
        internal Vec2 GetCentroid()
        {
            var corners = GetVertices();
            Vec2 centroid = new();
            for (int i = 0; i < corners.Length; i++)
            {
                Vec2 vec = corners[i];
                centroid += vec;
            }
            return centroid / corners.Length;
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
