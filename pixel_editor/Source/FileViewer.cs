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

            List<Metadata> lib = AssetLibrary.GetAllKeys();

            if (lib != null)
                foreach (var item in lib)
                    listBox.Items.Add(item.pathFromProjectRoot);

            listBox.ScrollIntoView(lib.First().pathFromProjectRoot); 
        }
        public Asset? GetSelectedAsset()
        {
            var item = listBox.SelectedItem;
            if (item is string path)
            {
                var meta = AssetLibrary.FetchMetaRelative(path);
                if (meta != null)
                    return AssetLibrary.FetchByMeta(meta);
            }
            return null; 
        }
    }
}