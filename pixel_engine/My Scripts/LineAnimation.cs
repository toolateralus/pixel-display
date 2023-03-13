using pixel_renderer.ShapeDrawing;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;

namespace pixel_renderer
{

    public class LineAnimation
    {
        public bool IsAlive = true;
        public List<LineFrame> frames = new();

        int index = 0;
        private Vector2 origin;
        private Vector2 extents;
        public List<Pixel> Pallette = new() { Color.Purple, Color.MediumSeaGreen, Color.MediumPurple, Color.MediumBlue };

        public void SetLifeCycle(Vector2 origin, Vector2 extents, Curve velocity, List<Pixel> colors)
        {
            if (velocity.points.Count < colors.Count)
                return;

            this.origin = origin;
            this.extents = extents;
            var zero = Vector2.Zero;
            foreach (var color in colors)
            {
                var pos = velocity.Next();
                var end = pos + velocity.Next();
                frames.Add(new(zero, pos, end, color));
            }
        }

        Vector2 accStart;
        Vector2 accEnd;

        public Line? Next()
        {
            if (frames.Count == 0)
                return null; 

            if (frames.Count <= index)
                index = 0;

            var frame = frames[index];
            index++;

            Pixel colorToLerpTo = JRandom.Pixel();
            Vector2 newPosOffset = new Vector2(0.003f, -0.001f);
            Vector2 newEndOffset = new Vector2(0.001f, -0.001f);
            Vector2 additionalVel = new Vector2(0.001f, -0.001f);


            if (accStart.IsWithin(origin, extents))
                accStart += newPosOffset;
            else accStart = origin;

            if (accEnd.IsWithin(origin, extents))
                accEnd += newEndOffset;
            else accEnd = origin;
            

            return frame.Next(colorToLerpTo, additionalVel,  accEnd, accStart);
        }

        int colorIndex= 0;

        internal Pixel NextColor()
        {
            if (Pallette.Count == 0) return Pixel.Black;
            if (colorIndex >= Pallette.Count)
                colorIndex = 0;
            return Pallette[colorIndex++];
        }

    }
}
