using KeraLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;





namespace pixel_renderer
{
    public class LuaInterop
    {
        Lua state;

        public bool Run(string fileName)
        {
            state ??= new();

            var result = state.LoadFile(fileName);

            if (result != LuaStatus.OK)
            {
                Runtime.Log(state.ToString(1));
                return false; 
            }

            result = state.PCall(0, -1, 0);

            if (result != LuaStatus.OK)
            {
                Runtime.Log(state.ToString(1));
                return false;
            }
            return true;

        }
    }
}