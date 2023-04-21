using System.Windows.Controls;

namespace pixel_editor
{
    public class FileViewer
    {
        public FileViewer(Grid fileViewerGrid, ListBox fileViewerListBox)
        {
            grid = fileViewerGrid;
            listBox = fileViewerListBox;
        }
        private ListBox listBox;
        private Grid grid;
        public void Insert(string name)
        {
            listBox.Items.Add(name);
        }
    }
}