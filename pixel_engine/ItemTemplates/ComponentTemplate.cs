using static Pixel.Input; 
using static Pixel.Runtime; 
using static Pixel.ShapeDrawer; 
using Pixel.Types.Components;
using Pixel.Types;
using System.Windows.Input;
using Pixel.Types.Physics;
using Pixel_Core.Types.Attributes;

namespace Pixel
{
    [HideFromEditor]
    public class ComponentTemplate : Component
    {
        // you must dispose of any references to nodes and components here, simply set them as null.
        public override void Dispose()
        {
        }
        // called right before the first Update/FixedUpdate
        public override void Awake()
        {
        }
        // called every physics frame.
        public override void FixedUpdate(float delta)
        {

        }
        // for debug gizmos/visualizations
        public override void on_draw_shapes_internal()
        {
        }
    }
}
