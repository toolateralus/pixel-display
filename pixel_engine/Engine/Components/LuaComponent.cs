using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pixel_renderer
{
    public class LuaComponent : Component
    {
        [Field]
        [InputField]
        string value = "Write your script here.";
        internal bool lastExecutionResult;
        internal string lastErr;
        [Method]
        public void Run()
        {

            var script = Runtime.Current.Lua.Script(value);
            if (script.result)
            {
                lastExecutionResult = true;
                lastErr = script.err;
            }
            else
            {
                lastExecutionResult = false;
                lastErr = "nil";
            }
        }
        /// <summary>
        /// this should only need to include references to components or nodes.
        /// </summary>
        public override void Dispose()
        {
        }
    }
}
