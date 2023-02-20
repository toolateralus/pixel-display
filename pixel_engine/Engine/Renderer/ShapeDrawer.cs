using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace pixel_renderer
{
    public static class ShapeDrawer
    {

        static Dictionary<object, Action> DrawEvents = new();  

        public static bool TryRegister(object sender, Action action)
        {
            if (DrawEvents.ContainsKey(sender))
                return false;

            DrawEvents.Add(sender, action);
            return true;
        }

        public static void Refresh()
        {
            lines.Clear();
            foreach (var x in DrawEvents)
            {
                if (x.Key != null)
                    x.Value.Invoke();
                else DrawEvents.Remove(x);
            }

        }

        /// <summary>
        /// start point of each line will always have an x value less than or equal to the end point
        /// </summary>
        internal static List<Line> lines = new();
        public static void DrawLine(Vec2 startPoint, Vec2 endPoint, Color? color = null)
        {
            Vec2 leftSide = startPoint.x < endPoint.x ? startPoint : endPoint;
            Vec2 rightSide = startPoint.x >= endPoint.x ? startPoint : endPoint;
            lines.Add(new Line(leftSide, rightSide, color ?? Color.White));
        }
    }
    public class Line
    {
        public Color color;
        public Vec2 startPoint;
        public Vec2 endPoint;
        public Line(Vec2 startPoint, Vec2 endPoint, Color color)
        {
            this.startPoint = startPoint;
            this.endPoint = endPoint;
            this.color = color;
        }
        public float GetSlope()
        {
            if (startPoint.x == endPoint.x)
                return float.PositiveInfinity;
            return (startPoint.y - endPoint.y) / (startPoint.x - endPoint.x);
        }
    }
}
