using Pixel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Pixel_Editor
{
    /// <summary>
    /// Interaction logic for FileViewerWindow.xaml
    /// </summary>
    public partial class FileViewerWindow : UserControl
    {
        public static ObservableCollection<string> Paths { get; set; } = new();
        static Action<string>? ScrollIntoViewAction;
        static Action? ClearPathsAction;
        static string NewestSelectedPath = "";
        public FileViewerWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            DataContext = this;
        }
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ClearPathsAction += OnClearPaths;
            ScrollIntoViewAction += OnScrollIntoView;
        }
        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            ClearPathsAction -= OnClearPaths;
            ScrollIntoViewAction -= OnScrollIntoView;
        }
        internal static string GetSelectedItem() => NewestSelectedPath;
        internal static void AddPath(string path) => Paths.Add(path);
        internal static void ClearPaths() => ClearPathsAction?.Invoke();
        internal static void ScrollIntoView(string path) => ScrollIntoViewAction?.Invoke(path);
        public void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListBox listBox ||
                listBox.SelectedItem is not string path)
                return;
            NewestSelectedPath = path;
        }
        private void OnClearPaths() => Paths.Clear();
        private void OnScrollIntoView(string path) => fileViewerListBox.ScrollIntoView(path);
    }
}
