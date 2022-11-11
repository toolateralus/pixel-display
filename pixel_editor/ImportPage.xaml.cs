using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace pixel_editor
{
    /// <summary>
    /// Interaction logic for ImportPage.xaml
    /// </summary>
    public partial class ImportPage : Page
    {
        Main mainWnd; 
        public ImportPage(Main main)
        {
            mainWnd = main; 
            InitializeComponent();
        }
    }
}
