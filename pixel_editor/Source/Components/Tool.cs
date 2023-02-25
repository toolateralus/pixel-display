﻿using pixel_renderer;
using System;
using System.Collections.Generic;

namespace pixel_editor
{
    public class Tool
    {

        internal protected void init_internal()
        {
            ShapeDrawer.DrawShapeActions += OnDrawShapes;
        }
        public virtual void OnDrawShapes()
        {

        }
        public virtual void Awake()
        {

        }
        public virtual void Update(float delta)
        {

        }
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