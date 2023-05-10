using System;
using System.Collections.Generic;
using System.Linq;
using Pixel;
using Component = Pixel.Types.Components.Component;
using System.Reflection;

namespace Pixel_Editor.Source
{
    public record ComponentEditorData
    {
        public readonly IReadOnlyCollection<FieldInfo> Fields;
        public readonly IReadOnlyCollection<MethodInfo> Methods;
        public readonly List<object> Values;
        public readonly WeakReference<Component> Component;

        public ComponentEditorData(Component component)
        {
            Component = new(component);
            Fields = InspectorControl.GetSerializedFields(component).ToList();
            Methods = InspectorControl.GetSerializedMethods(component).ToList();

            var values = new object[Fields.Count];

            for (int i = 0; i < Fields.Count; i++)
            {
                FieldInfo? field = Fields.ElementAt(i);
                values[i] = GetValueAtIndex(i);
            }

            Values = values.ToList();
        }
        public IReadOnlyCollection<object> GetAllValues(out int count)
        {
            count = Values.Count;
            return Values;
        }
        public object? GetValueAtIndex(int index)
        {
            if (IsReferenceAlive(out var component))
                return Fields.ElementAt(index)?.GetValue(component);
            return false;
        }
        public void UpdateChangedValues(object[] data)
        {
            if (data.Length != Values.Count)
            {
                Runtime.Log("component update invalidated : input array was the wrong size.");
                return;
            }

            for (int i = 0; i < Values.Count; ++i)
            {
                object localVal = Values.ElementAt(i);
                object newVal = data.ElementAt(i);

                if (localVal == newVal)
                    continue;

                Values[i] = newVal;
            }
        }
        public bool SetValueAtIndex(int index, object value)
        {
            if (IsReferenceAlive(out var component))
            {
                Fields.ElementAt(index).SetValue(component, value);
                return true;
            }
            return false;
        }
        public bool IsReferenceAlive(out Component component)
        {
            if (!Component.TryGetTarget(out component))
                return false;
            return true;
        }
        public bool HasValueChanged(int index, object value, out object newValue)
        {
            newValue = GetValueAtIndex(index);
            if (newValue == value)
                return true;
            return false;
        }

    }
}