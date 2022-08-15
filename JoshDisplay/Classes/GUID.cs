using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoshDisplay.Classes
{

    public static class GUID
    {
        public static Dictionary<string, int> GUIDs = new Dictionary<string, int>();
        public static int index = 0; 
        public static string GetGUID()
        {
            index++; 
            var guid = Guid.NewGuid().ToString();
            GUIDs.Add(guid, index);
            return guid; 
        }
    }
}
