using KeraLua;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace pixel_renderer
{
    public class LUA
    {
        #region pixel_engine lua library (C# functions)
        public static LuaFunction dispose() => new((p) => 
        {
            var ct = env_vars.Count;
            env_vars.Clear();
            FromString($"{ct} environment variables released.");
            return 0;
        }); 
        public static LuaFunction print() => new((p) =>
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
        });
        public static LuaFunction getnode() => new((p) =>
        {
            var state = Lua.FromIntPtr(p);
            var n = state.GetTop();
            for (int i = 1; i <= n; ++i)
            {
                LuaType type = state.Type(i);
                switch (type)
                {
                    case LuaType.Number:
                        break;
                    case LuaType.String:
                        string name = state.ToString(i);
                        Stage stage = Runtime.Current.GetStage();
                        Node result = stage?.FindNode(name);
                        if (result != null)
                        {
                            env_vars.Add(result.Name, result);
                            var print = $"{result.Name} found..loaded at index {env_vars.Count}";
                            PrintLUA(print);
                        }
                        break;
                    default:
                        break;
                }
            }
            return 0;
        });

        private static void PrintLUA(string print)
        {
            FromString($"print(\"{print}\")");
        }

        public static LuaFunction list_env() => new((p) =>
        {
            var state = Lua.FromIntPtr(p);
            var n = state.GetTop();
            for (int i = 1; i <= n; ++i)
            {
                LuaType type = state.Type(i);
                switch (type)
                {
                    case LuaType.None:
                        foreach (var obj in env_vars)
                        {
                            PrintLUA(obj.Key + '\n');
                        }

                        break;
                    default:
                        break;
                }
            }
            return 0;
        });
        #endregion


        static Dictionary<string, object> env_vars = new();
        private static readonly Lua state = new();
        public LUA() => RefreshFunctions();
        public void RefreshFunctions()
        {
            var type = GetType();
            var methods = type.GetRuntimeMethods();
            foreach (var method in methods)
                if (method.ReturnType == typeof(LuaFunction))
                    state.Register(method.Name, (LuaFunction)method.Invoke(null, null));
        }
        public static (bool result, string err) FromString(string luaString)
        {
            if (luaString is null || luaString is "")
            {
                Runtime.Log("Lua component was called to run but it had no valid lua code to run.");
                return(true, "No code found"); 
            }
            var result = state.DoString(luaString);
            if (result)
                return (false, "nil");
            return (true, state.ToString(1));
        }
        public static bool FromFile(string fileName)
        {
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