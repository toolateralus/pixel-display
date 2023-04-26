using Newtonsoft.Json;
using pixel_core.Types.Components;
using pixel_core.Assets;
using pixel_core.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.TextFormatting;
using pixel_core.Statics;

namespace pixel_core
{
    public class Lua : Component
    {
        [Field]
        [InputField]
        [JsonProperty]
        string value = "print(\"string\") \n return 0 \n end";

        [Field]
        [JsonProperty]
        string path = "no path selected.";

        internal bool lastExecutionResult;
        internal string lastErr;
        [Method]
        public async Task SetScriptFromFileViewer()
        {
            var search = Interop.GetSelectedFileMetadataAsync();
            await search;
            var meta = search.Result;

            if (meta is not null && AssetLibrary.FetchByMeta(meta) is string value)
            {
                this.value = value;
                path = meta.pathFromProjectRoot;

                EditorEvent refresh_event = new(EditorEventFlags.COMPONENT_EDITOR_UPDATE);
                Interop.RaiseInspectorEvent(refresh_event);
            }
            else Interop.Log($"Meta {meta} not found from GetSelectedFileMetadataAsync call result");

        }
        [Method]
        public void SaveScriptToPath()
        {
            if (AssetLibrary.FetchMetaRelative(path) is Metadata meta)
            {
                IO.Write(value, meta);
                Importer.Import();
            } else
            {
                meta = new("Script_Test", Constants.WorkingRoot + "\\" + path, Constants.LuaExt);
                AssetLibrary.Register(meta, value);
                IO.Write(value, meta);
                Importer.Import();
            }
        }
        [Method]
        public void Run()
        {
            var execution = LUA.FromString(value);
            if (execution.result)
            {
                lastExecutionResult = true;
                lastErr = execution.err;
                Interop.Log(lastErr);
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
