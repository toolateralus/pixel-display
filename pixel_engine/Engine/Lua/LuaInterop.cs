using KeraLua;
using System;

namespace pixel_renderer
{
    public class LuaInterop
    {
        private Lua state;
        internal static LuaFunction printFunct = (p) =>
        {
            var state = Lua.FromIntPtr(p);
            var n = state.GetTop();
            for (int i = 1; i <= n; ++i)
            {
                LuaType type = state.Type(i);
                switch (type)
                {
                    case LuaType.None:
                        break;
                    case LuaType.Nil:
                        Runtime.Log("Lua: Nil");
                        break;
                    case LuaType.Boolean:
                        Runtime.Log($"Lua: {state.ToBoolean(i)}");
                        break;
                    case LuaType.Number:
                        Runtime.Log($"Lua: {state.ToNumber(i)}");
                        break;
                    case LuaType.String:
                        Runtime.Log($"Lua: {state.ToString(i)}");
                        break;
                    case LuaType.LightUserData:
                        Runtime.Log($"Lua: Light User Data");
                        break;
                    case LuaType.Table:
                        Runtime.Log($"Lua: Table");
                        break;
                    case LuaType.Function:
                        Runtime.Log($"Lua: Function");
                        break;
                    case LuaType.UserData:
                        Runtime.Log($"Lua: User Data");
                        break;
                    case LuaType.Thread:
                        Runtime.Log($"Lua: Thread");
                        break;
                    default:
                        break;
                }
            }
            return 0; 
        };
        
        public LuaInterop()
        {
            state = new();

            state.Register("print", printFunct);
        }
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