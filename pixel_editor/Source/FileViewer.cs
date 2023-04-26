using pixel_core;
using pixel_core.Assets;
using pixel_core.FileIO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace pixel_editor
{
    public class FileViewer
    {
        private ListBox listBox;
        private Grid grid;
        public FileViewer(Grid fileViewerGrid, ListBox fileViewerListBox)
        {
            grid = fileViewerGrid;
            listBox = fileViewerListBox;

            Refresh();
        }

        public void Refresh()
        {
            List<Metadata> lib = AssetLibrary.GetAllKeys();

            listBox.Items.Clear();

            string lastExt = "";
            if (lib != null)
            {
                // Sort the list of Metadata objects by their file extension
                lib = lib.OrderBy(m => GetExtensionIndex(m.extension)).ToList();

                foreach (var item in lib)
                {
                    if (item.extension != lastExt)
                    {
                        listBox.Items.Add(GetExtensionFullName(item.extension));
                        lastExt = item.extension;
                    }
                    var path = item.pathFromProjectRoot;
                    listBox.Items.Add(path);
                }
                listBox.ScrollIntoView(lib.First().pathFromProjectRoot);
            }
        }
        private static Color GetColorFromExtension(string extension)
        {
            switch (extension)
            {
                case ".bmp":
                    return Color.Red;
                case ".png":
                    return Color.Green;
                case ".jpg":
                    return Color.Blue;
                case ".mp3":
                    return Color.Orange;
                case ".asset":
                    return Color.Purple;
                case ".lua":
                    return Color.Teal;
                default:
                    return Color.Black;
            }
        }
        private string GetExtensionFullName(string ext)
        {
            switch (ext)
            {
                case ".bmp":
                    return "\n\t- - - Bitmap Image Files - - -\n";
                case ".png":
                    return "\n\t- - - Png Image Files - - -\n";
                case ".jpg":
                    return "\n\t- - - Jpeg Image Files - - -\n";
                case ".mp3":
                    return "\n\t- - - Mp3 Audio Files - - -\n";
                case ".asset":
                    return "\n\t- - - User Asset Files - - -\n";
                case ".lua":
                    return "\n\t- - - Lua Script Files - - -\n";
                default:
                    return "\n\t- !! - Unknown Files - !! -\n"; // Sort unknown file extensions to the end
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
            var item = listBox.SelectedItem;
            if (item is string path)
            {
                var meta = AssetLibrary.FetchMetaRelative(path);
                if (meta != null)
                {
                    var asset = AssetLibrary.FetchByMeta(meta);
                    return asset;
                }
            }
            return null; 
        }
        public Metadata? GetSelectedMeta()
        {
            var item = listBox.SelectedItem;
            if (item is string path)
            {
                var meta = AssetLibrary.FetchMetaRelative(path);
                if (meta != null)
                {
                    return meta;
                }
            }
            return null;
        }
    }
}