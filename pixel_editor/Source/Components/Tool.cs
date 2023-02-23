using System.Windows.Controls.Primitives;

namespace pixel_editor
{
    public abstract class Tool
    {
        public abstract void Awake();
        public abstract void Update(float delta);
        public static List<Tool> InitializedDerived()
        {
            List<Tool> list = new List<Tool>();
            var toolsTypes = pixel_renderer.Constants.GetInheritedTypesFromBase<Tool>();
            foreach (Type type in toolsTypes)
            {
                var obj = Activator.CreateInstance(type);
                if (obj is Tool tool)
                    list.Add(tool);
            }
            return list;
        }
    }
}
