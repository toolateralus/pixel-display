using Newtonsoft.Json;
using Pixel.Assets;
using Pixel.FileIO;
using Pixel.Statics;
using Pixel.Types.Components;
using PixelLang.Tools;
using System.Threading.Tasks;

namespace Pixel
{
    public enum ScriptingLanguage { PixelLang, LUA};
    public class Interpreter : Component
    {
        [Field]
        [InputField]
        [JsonProperty]
        string value = "print(\"Hello Squirreld\")";

        [JsonProperty] [Field]
        ScriptingLanguage language = ScriptingLanguage.PixelLang;

        [Field] 
        public string Language = "PL || LUA";

        [Field]
        [JsonProperty]
        string path = "no path selected.";

        internal bool lastExecutionResult;
        internal string lastErr = "";
        [Method]
        public async Task SetScriptFromFileViewer()
        {
            var search = Interop.GetSelectedFileMetadataAsync();
            await search;
            var meta = search.Result;

            if (meta is not null && Library.FetchByMeta(meta) is string value)
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
            if (Library.FetchMetaRelative(path) is Metadata meta)
            {
                IO.Write(value, meta);
                Importer.Import();
            }
            else
            {
                meta = new(Constants.WorkingRoot + "\\" + path);
                Library.Register(meta, value);
                IO.Write(value, meta);
                Importer.Import();
            }
        }


        
        public override void OnFieldEdited(string field)
        {
            switch (field)
            {
                case "Language":
                {
                        switch (Language)
                        {
                            case "PL":
                                language = ScriptingLanguage.PixelLang;
                                break;
                            case "LUA":
                                language = ScriptingLanguage.LUA;
                                break;
                        }
                        break;
                }


            }
        }

        [Method]
        public async void Run()
        {
            switch (language)
            {
                case ScriptingLanguage.PixelLang:
                    InputProcessor.TryCallLine(value);
                    break;
                case ScriptingLanguage.LUA:
                    var lua_exe = LUA.FromString(value);
                    if (lua_exe.result)
                    {
                        lastExecutionResult = true;
                        lastErr = lua_exe.err;
                        Interop.Log(lastErr);
                    }
                    else
                    {
                        lastExecutionResult = false;
                        lastErr = "nil";
                    }
                    break;
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
