#include <lua.h>
#include <lauxlib.h>
#include <lualib.h>

using namespace System::Runtime::InteropServices;
using namespace System;

public ref class LuaWrapper
{
public:
    LuaWrapper()
    {
        // Create a new Lua state
        L = luaL_newstate();

        // Load Lua libraries
        luaL_openlibs(L);
    }

    ~LuaWrapper()
    {
        // Close the Lua state
        lua_close(L);
    }

    bool LoadScript(String^ script)
    {
        // Convert the script string to a char*
        const char* str = (const char*)(Marshal::StringToHGlobalAnsi(script)).ToPointer();

        // Load the script into the Lua state
        int result = luaL_loadstring(L, str);

        // Free the memory allocated for the char*
        Marshal::FreeHGlobal(IntPtr((void*)str));

        // Check for errors
        if (result != LUA_OK)
        {
            // Get the error message
            const char* error = lua_tostring(L, -1);

            // Print the error message
            Console::WriteLine(gcnew String(error));

            // Pop the error message from the stack
            lua_pop(L, 1);

            return false;
        }

        return true;
    }

    bool RunScript()
    {
        // Call the Lua script
        int result = lua_pcall(L, 0, LUA_MULTRET, 0);

        // Check for errors
        if (result != LUA_OK)
        {
            // Get the error message
            const char* error = lua_tostring(L, -1);

            // Print the error message
            Console::WriteLine(gcnew String(error));

            // Pop the error message from the stack
            lua_pop(L, 1);

            return false;
        }

        return true;
    }

private:
    lua_State* L;
};
