using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pixel_editor
{
    public abstract class Tool
    {
        public abstract void Awake();
        public abstract void Update(float delta);
    }
}
