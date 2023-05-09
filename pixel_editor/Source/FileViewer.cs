using Pixel;
using Pixel.Assets;
using Pixel.FileIO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Pixel_Editor
{
    public class FileViewer
    {
        public FileViewer()
        {
            Refresh();
        }

        public void Refresh()
        {
            List<Metadata> lib = Library.GetAllKeys();

            FileViewerWindow.ClearPaths();

            string lastExt = "";
            if (lib != null)
            {
                // Sort the list of Metadata objects by their file extension
                lib = lib.OrderBy(m => GetExtensionIndex(m.extension)).ToList();

                foreach (var item in lib)
                {
                    if (item.extension != lastExt)
                    {
                        FileViewerWindow.AddPath(GetExtensionFullName(item.extension));
                        lastExt = item.extension;
                    }
                    var path = item.pathFromProjectRoot;
                    FileViewerWindow.AddPath(path);
                }
                FileViewerWindow.ScrollIntoView(lib.First().pathFromProjectRoot);
            }
        }
        private static System.Drawing.Color GetColorFromExtension(string extension)
        {
            switch (extension)
            {
                case ".bmp":
                    return System.Drawing.Color.Red;
                case ".png":
                    return System.Drawing.Color.Green;
                case ".jpg":
                    return System.Drawing.Color.Blue;
                case ".mp3":
                    return System.Drawing.Color.Orange;
                case ".asset":
                    return System.Drawing.Color.Purple;
                case ".lua":
                    return System.Drawing.Color.Teal;
                default:
                    return System.Drawing.Color.Black;
            }
        }
        private string GetExtensionFullName(string ext)
        {
            switch (ext)
            {
                case ".bmp":
                    return "\t- - - Bitmap Image Files - - -";
                case ".png":
                    return "\t- - - Png Image Files - - -";
                case ".jpg":
                    return "\t- - - Jpeg Image Files - - -";
                case ".mp3":
                    return "\t- - - Mp3 Audio Files - - -";
                case ".asset":
                    return "\t- - - User Asset Files - - -";
                case ".lua":
                    return "\t- - - Lua Script Files - - -";
                default:
                    return "\t- !! - Unknown Files - !! -"; // Sort unknown file extensions to the end
            }
        }
        private int GetExtensionIndex(string extension)
        {
            switch (extension)
            {
                case ".bmp":
                    return 0;
                case ".png":
                    return 1;
                case ".jpg":
                    return 2;
                case ".mp3":
                    return 3;
                case ".asset":
                    return 4;
                case ".lua":
                    return 5;
                default:
                    return int.MaxValue; // Sort unknown file extensions to the end
            }
        }
        /// <summary>
        /// see AssetLibrary.FetchByMeta for more info on why this returns an object and not an asset.
        /// </summary>
        /// <returns></returns>
        public object? GetSelectedObject()
        {
            var item = Pixel_Editor.FileViewerWindow.GetSelectedItem();
            if (item is string path)
            {
                var meta = Library.FetchMetaRelative(path);
                if (meta != null)
                {
                    var asset = Library.FetchByMeta(meta);
                    return asset;
                }
            }
            return null; 
        }
        public Metadata? GetSelectedMeta()
        {
            var item = FileViewerWindow.GetSelectedItem();
            if (item is string path)
            {
                var meta = Library.FetchMetaRelative(path);
                if (meta != null)
                {
                    return meta;
                }
            }
            return null;
        }
    }
}