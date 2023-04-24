using pixel_renderer;
using pixel_renderer.Assets;
using pixel_renderer.FileIO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

            if (lib != null)
                foreach (var item in lib)
                    listBox.Items.Add(item.pathFromProjectRoot);

            listBox.ScrollIntoView(lib.First().pathFromProjectRoot);
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