using Microsoft.VisualBasic;
using pixel_renderer.Assets;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pixel_renderer
{
    public class TexturePool
    {
        protected internal static volatile HashSet<Texture> Textures = new();
        public TexturePool()
        {
            //AssetLibrary.OnTextureRegistered += OnTextureRegister;
            //AssetLibrary.OnTextureQueried += OnTextureQuery;
        }

        public static void OnTextureQuery(string name, Action<Texture?> returnAction)
        {
            var x = TryGet(name); 
            returnAction?.Invoke(x);
        }

        public static Texture? TryGet(string name)
        {
            foreach (Texture tex in Textures)
                if (tex.Name == name)
                    return tex;
            return null; 
        }
        private static void OnTextureRegister(Texture obj)
        {
            if (Textures.Contains(obj))
                return; 
            else Textures.Add(obj);
        }
    }
}
