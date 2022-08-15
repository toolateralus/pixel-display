using System;
using System.Collections.Generic;
using System.Media;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace JoshDisplay.Classes
{
    public class Sprite
    {
        public Vec2 size = new Vec2();
        public Color color; 
        public bool isCollider = false;
        
        public Sprite(Vec2 size, Color color, bool isCollider)
        {
            this.size = size;
            this.color = color; 
            this.isCollider = isCollider; 
        }
    }
}
