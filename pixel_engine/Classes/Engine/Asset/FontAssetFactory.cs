using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;

namespace pixel_renderer.Assets
{
    public static class FontAssetFactory
    {
        public static FontAsset CreateFont(int start, int end, Bitmap[] characters)
        {
            FontAsset fontAsset = new($"fontAsset{start}");
            char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            for (int i = start; i < end; i++)
            {
                if (i > alpha.Length || i > characters.Length)
                {
                    MessageBox.Show("Font asset could not be created, Index out of range.");
                    return null;
                }
                fontAsset.characters.Add(alpha[i], characters[i]);
            }
            if (fontAsset.characters.Count <= 0)
            {
                MessageBox.Show("Font is empty.");
            }
            return fontAsset;
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="text"></param>
        /// <returns> a List of Bitmap objects ordered by their Character value in accordance to the Text passed in.</returns>
        internal static void InitializeDefaultFont()
        {
            var path = Settings.Appdata + Settings.FontDirectory;

            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            IEnumerable<string> files = Directory.GetFiles(path);

            int i = 0;

            foreach (string file in files)
            {
                BitmapAsset bitmap = new($"{'a' + i}")
                {
                    RuntimeValue = new(file),
                    fileType = typeof(Bitmap)
                };
                Library.Register(typeof(BitmapAsset), bitmap);
                i++;
            }
        }
    }
}