
namespace pixel_renderer
{
    using System;
    public static class UuID
    {
        public static string NewUUID()
        {
            var guid = Guid.NewGuid().ToString();
            return guid;
        }
    }
}
