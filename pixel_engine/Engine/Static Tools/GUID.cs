namespace pixel_renderer
{
    using System;
    public class UUID
    {
        static int index = 0; 
        static int[] uuid_hashmap= new int[2500];

        public static string NewUUID()
        {
            var id = Guid.NewGuid().ToString();
            Cache(id);
            return id; 
        }
        private static void Cache(string UUID) => uuid_hashmap[index] = UUID.GetHashCode();  
          
        }
}
