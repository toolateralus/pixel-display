using Pixel;
using PixelLang.Tools;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Pixel_Editor.Source;
using Component = Pixel.Types.Components.Component;
using System.Windows.Media;

namespace Pixel_Editor
{
    /// <summary>
    /// Interaction logic for ComponentEditorControl.xaml
    /// </summary>
    public partial class HierarchyControl : UserControl
    {
        public ObservableCollection<Node>? Hierarchy => Runtime.Current?.GetStage()?.nodes.TraverseHierarchy(); 

    }
}
