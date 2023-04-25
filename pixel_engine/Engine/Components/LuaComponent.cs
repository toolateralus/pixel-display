using pixel_renderer.Assets;
using pixel_renderer.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.TextFormatting;

namespace pixel_renderer
{
    public class LuaComponent : Component
    {
        [Field]
        [InputField]
        string value = "Write your script here.";

        [Field]
        string path = "no path selected.";

        internal bool lastExecutionResult;
        internal string lastErr;
        [Method]
        public async Task SetScriptFromFileViewer()
        {
            var search = Runtime.GetSelectedFileMetadataAsync();
            await search;
            var meta = search.Result;

            if (meta is not null && AssetLibrary.FetchByMeta(meta) is string value)
            {
                this.value = value;
                path = meta.pathFromProjectRoot;

                EditorEvent refresh_event = new(EditorEventFlags.COMPONENT_EDITOR_UPDATE);
                Runtime.RaiseInspectorEvent(refresh_event);
            }
            else Runtime.Log($"Meta {meta} not found from GetSelectedFileMetadataAsync call result");

        }
        [Method]
        public void SaveScriptToPath()
        {
            if (AssetLibrary.FetchMetaRelative(path) is Metadata meta)
            {
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
                Runtime.Log(lastErr);
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
