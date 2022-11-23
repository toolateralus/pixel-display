using System;
using System.Collections.Generic;
using System.Drawing;

namespace pixel_renderer.Assets
{
    public class FontAsset : Asset
    {
        new public string Name = "New Font Asset";
        public Dictionary<char, Bitmap> characters = new();
        internal static List<Bitmap> GetCharacterImages(FontAsset asset, string text)
        {
            List<Bitmap> output = new();
            int i = 0;

            foreach (char character in text)
            {
                // cache here to force uppercase without modifying the asset.
                var _char = character;
                
                if (char.IsLower(character))
                    _char = char.ToUpper(character);

                if (asset.characters.ContainsKey(_char))
                {
                    var img = (Bitmap)asset.characters[_char].Clone();
                    output.Add(img);
                }
                i++;
            }
            return output;
        }
        internal static List<Vec2> GetCharacterPosition(FontAsset asset)
        {
            List<Vec2> positions = new();
            foreach (var x in asset.characters.Values)
                positions.Add(new(x.Width, x.Height));

            return positions;
        }
        public FontAsset(string name) : base(name, typeof(FontAsset))
        {
            Name = name; 
        }
    }
}