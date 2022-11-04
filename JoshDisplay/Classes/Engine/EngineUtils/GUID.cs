
namespace pixel_renderer
{
    using System;
    using System.Collections.Generic;
    public static class UUID
    {
        public static Dictionary<string, int> GUIDs = new Dictionary<string, int>();
        public static int index = 0;
        public static string NewUUID()
        {
            index++;
            var guid = Guid.NewGuid().ToString();
            GUIDs.Add(guid, index);
            return guid;
        }
    }
}
