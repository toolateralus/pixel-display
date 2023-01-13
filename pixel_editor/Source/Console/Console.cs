﻿using pixel_renderer;
using pixel_editor;
using System;
using System.Threading.Tasks;
namespace pixel_editor
{
    public static class Console
    {
        static Inspector? inspector = Editor.Current.Inspector; 

        public static void Print(object? o, bool includeDateTime = false)
        {
            var msg = o.ToString();
            var e = new InspectorEvent(msg, includeDateTime);
            Editor.QueueEvent(e); 
        }
        public static void Error(object? o = null, int? textColorAlterationDuration = null)
        {
            string? msg = o.ToString();
            InspectorEvent e = new(msg, true);
           
                if (textColorAlterationDuration is not null)
                    e.action = RedTextForMsAsync( (int)textColorAlterationDuration);
            Editor.QueueEvent(e);
        }
        public static Action<object[]?> RedTextForMsAsync(int delay)
        {
            Action<object?> c = async (o) =>
            {
                Editor.Current.RedText(null);
                await Task.Delay(delay);
                Editor.Current.BlackText(null);
            };
            return c;
        }
    }
}